//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class RelationshipTypesService : BaseCoordinationService, IRelationshipTypesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public RelationshipTypesService(

            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all relationship-types
        /// </summary>
        /// <returns>Collection of RelationshipTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipTypes>> GetRelationshipTypesAsync(bool bypassCache = false)
        {
            var relationshipTypesCollection = new List<Ellucian.Colleague.Dtos.RelationshipTypes>();

            var relationshipTypesEntities = await _referenceDataRepository.GetRelationTypes2Async(bypassCache);
            if (relationshipTypesEntities != null && relationshipTypesEntities.Any())
            {
                foreach (var relationshipType in relationshipTypesEntities)
                {
                    Ellucian.Colleague.Dtos.RelationshipTypes relTyp = ConvertRelationshipTypesEntityToDto(relationshipType);
                    var inverse = !string.IsNullOrEmpty(relationshipType.InverseRelType) ? relationshipTypesEntities.FirstOrDefault(i => i.Code.ToUpper() == relationshipType.InverseRelType.ToUpper()) : null;
                    relationshipTypesCollection.Add(ConvertInverseRelationshipTypeEntityToValidRelationshipTypeDtoAndAddToCollection(relTyp, inverse));
                }
            }
            return relationshipTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a RelationshipTypes from its GUID
        /// </summary>
        /// <returns>RelationshipTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.RelationshipTypes> GetRelationshipTypesByGuidAsync(string guid)
        {
            var relationTypeEntities = (await _referenceDataRepository.GetRelationTypes2Async(true));

            try
            {
                Ellucian.Colleague.Domain.Base.Entities.RelationType relTypEntity = (relationTypeEntities).Where(rt => rt.Guid == guid).FirstOrDefault();
                if (relTypEntity == null)
                {
                    throw new KeyNotFoundException("relationship-types not found for GUID " + guid);
                }
                Ellucian.Colleague.Dtos.RelationshipTypes relTypDto = ConvertRelationshipTypesEntityToDto(relTypEntity);
                var inverse = !string.IsNullOrEmpty(relTypEntity.InverseRelType) ? relationTypeEntities.FirstOrDefault(i => i.Code.ToUpper() == relTypEntity.InverseRelType.ToUpper()) : null;
                return ConvertInverseRelationshipTypeEntityToValidRelationshipTypeDtoAndAddToCollection(relTypDto, inverse);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("relationship-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("relationship-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RelationTypes domain entity to its corresponding RelationshipTypes DTO
        /// </summary>
        /// <param name="source">RelationTypes domain entity</param>
        /// <returns>RelationshipTypes DTO</returns>
        private Ellucian.Colleague.Dtos.RelationshipTypes ConvertRelationshipTypesEntityToDto(Ellucian.Colleague.Domain.Base.Entities.RelationType source)
        {
            var relationshipTypes = new Ellucian.Colleague.Dtos.RelationshipTypes();

            relationshipTypes.Id = source.Guid;
            relationshipTypes.Code = source.Code;
            relationshipTypes.Title = source.Description;
            relationshipTypes.Description = null;
            if (!string.IsNullOrEmpty(source.OrgIndicator))
            {
                if (source.OrgIndicator.Equals("Y"))
                {
                    relationshipTypes.RestrictedUsage = RelationshipTypesRestrictedUsage.Nonpersonsonly;
                }
                else if (source.OrgIndicator.Equals("N"))
                {
                    relationshipTypes.RestrictedUsage = RelationshipTypesRestrictedUsage.Personsonly;
                }
            }
                                                                                                
            return relationshipTypes;
        }

        private Dtos.RelationshipTypes ConvertInverseRelationshipTypeEntityToValidRelationshipTypeDtoAndAddToCollection(Ellucian.Colleague.Dtos.RelationshipTypes source, Ellucian.Colleague.Domain.Base.Entities.RelationType inverse)
        {
            var validReciprocalRelationship = new List<Dtos.RelationshipTypesValidReciprocalRelationships>();

            if (inverse != null)
            {
                var element = new Dtos.RelationshipTypesValidReciprocalRelationships()
                {
                    Detail = new Dtos.GuidObject2(inverse.Guid)
                };
                validReciprocalRelationship.Add(element);
            }

            if ((validReciprocalRelationship != null) && (validReciprocalRelationship.Any()))
            {
                source.ValidReciprocalRelationships = validReciprocalRelationship;
            }

            return source;
        }


    }
   
}