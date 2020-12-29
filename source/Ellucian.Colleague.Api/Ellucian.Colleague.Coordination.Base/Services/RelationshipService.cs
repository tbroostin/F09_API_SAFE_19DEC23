// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class RelationshipService : BaseCoordinationService, IRelationshipService
    {
        #region class OrderedRelationship
        /// <summary>
        /// Used when we need to ensure the order of Relationships in local data structures matches the order found in the database
        /// </summary>
        private class OrderedRelationship : Dtos.Base.Relationship
        {
            // integer to indicate the original retrieved order
            public int Seq;

            // static method to generate a Relationship from an OrderedRelationship
            public static Dtos.Base.Relationship RelFromOrdRel(OrderedRelationship ord)
            {
                return new Dtos.Base.Relationship()
                {
                    PrimaryEntity = ord.PrimaryEntity,
                    OtherEntity = ord.OtherEntity,
                    RelationshipType = ord.RelationshipType,
                    IsPrimaryRelationship = ord.IsPrimaryRelationship,
                    IsActive = ord.IsActive,
                    StartDate = (ord.StartDate == DateTime.MinValue) ? null : ord.StartDate,
                    EndDate = (ord.EndDate == DateTime.MaxValue) ? null : ord.EndDate,
                };
            }
        }
        #endregion

        private IRelationshipRepository _relationshipRepository;
        private IReferenceDataRepository _referenceDataRepository;
        private IPersonBaseRepository _personBaseRepository;

        private int _seqId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">Interface to the adapter registry.</param>
        /// <param name="currentUserFactory">Interface to the current user factory.</param>
        /// <param name="roleRepository">Interface to the role repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="referenceDataRepository">Interface to the reference data repository.</param>
        /// <param name="relationshipRepository">Interface to the relationship repository.</param>
        /// <param name="personBaseRepository">Interface to the person base repository.</param>
        public RelationshipService(IAdapterRegistry adapterRegistry,
                                   ICurrentUserFactory currentUserFactory,
                                   IRoleRepository roleRepository,
                                   ILogger logger,
                                   IReferenceDataRepository referenceDataRepository,
                                   IRelationshipRepository relationshipRepository,
                                   IPersonBaseRepository personBaseRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _relationshipRepository = relationshipRepository;
            _personBaseRepository = personBaseRepository;
        }

        /// <summary>
        /// Returns the list of primary relationships that this person has with other persons or organizations
        /// </summary>
        /// <param name="id">the identifier of the person of interest</param>
        /// <returns>an enumeration of <see cref="Relationship"/> primary relationships</returns>
        public async Task<IEnumerable<Dtos.Base.Relationship>> GetPersonPrimaryRelationshipsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // A user can only see their own relationships
            if (!CurrentUser.IsPerson(id))
            {
                string message = CurrentUser + " cannot view relationship information for person " + id;
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // instantiate the output list
            List<Dtos.Base.Relationship> relsToReturn = new List<Dtos.Base.Relationship>();

            // get all relationships; return if none
            var allRels = await GetPersonRelationshipsAsync(id);
            if (!allRels.Any()) return relsToReturn;

            // convert to OrderedRelationship to ensure that order is preserved
            IEnumerable<OrderedRelationship> allOrdRels = (IEnumerable<OrderedRelationship>)allRels.Select(rel => new OrderedRelationship()
            {
                Seq = _seqId++,
                PrimaryEntity = rel.PrimaryEntity,
                OtherEntity = rel.OtherEntity,
                RelationshipType = rel.RelationshipType,
                IsPrimaryRelationship = rel.IsPrimaryRelationship,
                IsActive = rel.IsActive,
                StartDate = (rel.StartDate == null) ? DateTime.MinValue : rel.StartDate,
                EndDate = (rel.EndDate == null) ? DateTime.MaxValue : rel.EndDate,

            });

            // get all active relationships; return if none
            var allActiveOrdRels = allOrdRels.Where(x => x.IsActive).ToList();
            if (allActiveOrdRels == null || !allActiveOrdRels.Any()) return relsToReturn;

            // collect the set of explicit primary relationships, if any.  If nonesuch, do not return but keep processing.
            var primeRels = allActiveOrdRels.Where(x => x.IsPrimaryRelationship).Select(y => OrderedRelationship.RelFromOrdRel(y)).ToList();
            if (primeRels != null && primeRels.Any())
            {
                relsToReturn.AddRange(primeRels);
            }

            // Get rid of all of the relationships of all of the people who have an explicit primary relationship.
            // If there are still people remaining, determine their implicit primary relationships
            var primaryPeople = relsToReturn.Select(x => x.OtherEntity).Distinct().ToList();
            var nonPrimaryOrdRels = allActiveOrdRels.Where(x => !primaryPeople.Contains(x.OtherEntity)).ToList();
            if (nonPrimaryOrdRels != null && nonPrimaryOrdRels.Any())
            {
                // Group by OtherEntity.
                var relGroups = nonPrimaryOrdRels.GroupBy(o => o.OtherEntity).ToList();
                //  Order each group by StartDate then by sequence number, and take the first of each group
                var implicitPrimaryOrdRels = relGroups.Select(g => g.OrderBy(s => s.StartDate).ThenBy(s => s.Seq).First()).ToList();
                // Convert to Relationships and add to the return list.  implicitPrimaryOrdRels *must* contain rows because nonPrimaryOrdRels contains rows.
                relsToReturn.AddRange(implicitPrimaryOrdRels.Select(x => OrderedRelationship.RelFromOrdRel(x)));
            }

            return relsToReturn;
        }

        /// <summary>
        /// Creates the given relationship type between the two given entities
        /// </summary>
        /// <param name="isTheId">P1, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="relationshipType">the relationship type in the the phrase 'P1 is the "relationship type" of P2'</param>
        /// <param name="ofTheId">P2, in the phrase 'P1 is the "relationship type" of P2'</param>
        /// <returns>the created <see cref="Relationship"/></returns>
        public async Task<Dtos.Base.Relationship> PostRelationshipAsync(string isTheId, string relationshipType, string ofTheId)
        {
            if (string.IsNullOrEmpty(isTheId))
            {
                throw new ArgumentNullException("isTheId");
            }
            if (string.IsNullOrEmpty(relationshipType))
            {
                throw new ArgumentNullException("relationshipType");
            }
            if (string.IsNullOrEmpty(ofTheId))
            {
                throw new ArgumentNullException("ofTheId");
            }

            // Can only create a relationship involving oneself
            if (!(CurrentUser.IsPerson(isTheId) || CurrentUser.IsPerson(ofTheId)))
            {
                string message = "Cannot a create a relationship of which you are not a part.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Can't create a relationship with oneself
            if (isTheId.Equals(ofTheId))
            {
                string message = "Cannot create a relationship with oneself.";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var types = await _referenceDataRepository.GetRelationshipTypesAsync();
            if (!types.Where(t => t.Code.Equals(relationshipType)).Any())
            {
                string message = relationshipType+ " is not a valid relationship type.";
                logger.Error(message);
                throw new ArgumentOutOfRangeException(message);
            }
            var newRelationship = await _relationshipRepository.PostRelationshipAsync(isTheId, relationshipType, ofTheId);
            var adapter = new AutoMapperAdapter<Domain.Base.Entities.Relationship, Dtos.Base.Relationship>(_adapterRegistry, logger);
            return adapter.MapToType(newRelationship);
        }

        /// <summary>
        /// Retrieves all relationships for a person
        /// </summary>
        /// <param name="id">the identifier of the person of interest</param>
        /// <returns>All relationships for the person</returns>
        private async Task<IEnumerable<Dtos.Base.Relationship>> GetPersonRelationshipsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // A user can only see their own relationships
            if (!CurrentUser.IsPerson(id))
            {
                string message = CurrentUser + " cannot view relationship information for person " + id;
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var outRelations = await GetNormalizedRelationships(id);

            var adapter = new AutoMapperAdapter<Domain.Base.Entities.Relationship, Dtos.Base.Relationship>(_adapterRegistry, logger);
            return outRelations.Select(x => adapter.MapToType(x));
        }

        /// <summary>
        /// Returns a list of normalized relationships
        /// </summary>
        /// <param name="id">Unique identifier of the person with relationships to be normalized</param>
        /// <returns>List of normalized relationships</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Relationship>> GetNormalizedRelationships(string id)
        {
            // instantiate the output list
            List<Domain.Base.Entities.Relationship> normalizedRelationships = new List<Domain.Base.Entities.Relationship>();

            var relationshipTypes = await _referenceDataRepository.GetRelationshipTypesAsync();
            var rtvRelations = await _relationshipRepository.GetPersonRelationshipsAsync(id);
            if (!rtvRelations.Any())
            {
                return normalizedRelationships;
            }

            normalizedRelationships.AddRange(rtvRelations.Where(r => r.PrimaryEntity == id).Select(rel => Domain.Base.Services.RelationshipService.CreateInverseRelationship(rel, relationshipTypes)));
            normalizedRelationships.AddRange(rtvRelations.Where(r => r.OtherEntity == id).Select(rel => Domain.Base.Services.RelationshipService.CreateRelationship(rel)));

            return normalizedRelationships;
        }
    }
}
