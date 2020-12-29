// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
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
    /// <summary>
    /// Service to retrieve the current organizational structure
    /// </summary>
    [RegisterType]
    public class OrganizationalPersonPositionService : BaseCoordinationService, IOrganizationalPersonPositionService
    {
        private readonly IOrganizationalPersonPositionRepository _orgPersonPositionRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalPersonPositionService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="orgPersonPositionRepository"></param>
        /// <param name="personBaseRepository"></param>
        /// <param name="logger"></param>
        public OrganizationalPersonPositionService(IAdapterRegistry adapterRegistry, IOrganizationalPersonPositionRepository orgPersonPositionRepository, IPersonBaseRepository personBaseRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _orgPersonPositionRepository = orgPersonPositionRepository;
            _personBaseRepository = personBaseRepository;
            _logger = logger;
        }

        /// <summary>
        /// Requests organizational person positions that meet query criteria
        /// </summary>
        /// <param name="criteria">Criteria to search for person positions</param>
        /// <returns>OrganizationalPersonPosition dtos for the given criteria</returns>
        public async Task<IEnumerable<OrganizationalPersonPosition>> QueryOrganizationalPersonPositionAsync(OrganizationalPersonPositionQueryCriteria criteria)
        {
            IEnumerable<Domain.Base.Entities.OrganizationalPersonPosition> organizationalPersonPositions = new List<Domain.Base.Entities.OrganizationalPersonPosition>();
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Organizational Person Position Query Criteria cannot be null.");
            }

            CheckViewOrUpdateOrganizationalRelationshipsPermission();

            if (string.IsNullOrEmpty(criteria.SearchString))
            {
                if (criteria.Ids == null || criteria.Ids.Count() == 0)
                {
                    throw new ArgumentNullException("criteria", "Some organizational person position criteria must be specified.");
                }
                else
                {
                    // If ONLY OrganizationalPersonPosition primary IDs are coming in, use that repository method
                    organizationalPersonPositions = await _orgPersonPositionRepository.GetOrganizationalPersonPositionsByIdsAsync(criteria.Ids);
                }
            }
            else
            {
                var personIds = new List<string>();
                if (!string.IsNullOrEmpty(criteria.SearchString))
                {
                    var personSearchResults = await _personBaseRepository.SearchByIdsOrNamesAsync(new List<string>(), criteria.SearchString);
                    if (personSearchResults != null && personSearchResults.Any())
                    {
                        personIds.AddRange(personSearchResults.Select(p => p.Id));
                    }
                }

                organizationalPersonPositions = await _orgPersonPositionRepository.GetOrganizationalPersonPositionAsync(personIds, criteria.Ids);
            }

            return await BuildOrganizationalPersonPositionDtos(organizationalPersonPositions);
        }

        /// <summary>
        /// Returns OrganizationalPersonPosition for the given single ID
        /// </summary>
        /// <param name="id">Organizational person position ID</param>
        /// <returns>OrganizationalPersonPosition dto for the given ID</returns>
        public async Task<OrganizationalPersonPosition> GetOrganizationalPersonPositionByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "id is required to get organizational person position.");
            }

            CheckViewOrUpdateOrganizationalRelationshipsPermission();

            var organizationalPersonPositions = await _orgPersonPositionRepository.GetOrganizationalPersonPositionsByIdsAsync(new List<string> { id });
            if (organizationalPersonPositions.Count() < 1)
            {
                throw new KeyNotFoundException("Unable to find organizational person position for ID: " + id);
            }

            IEnumerable<Dtos.Base.OrganizationalPersonPosition> orgPersonPositionDtos = new List<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>();
            orgPersonPositionDtos = await BuildOrganizationalPersonPositionDtos(organizationalPersonPositions);

            return orgPersonPositionDtos.First();
        }

        private async Task<IEnumerable<OrganizationalPersonPosition>> BuildOrganizationalPersonPositionDtos(IEnumerable<Domain.Base.Entities.OrganizationalPersonPosition> organizationalPersonPositions)
        {
            var orgPersonPositionDtos = new List<Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>();
            if (organizationalPersonPositions != null)
            {
                var personBaseIds = organizationalPersonPositions.Select(opp => opp.PersonId).ToList();
                foreach (var item in organizationalPersonPositions)
                {
                    personBaseIds.AddRange(item.Relationships.Select(r => r.RelatedPersonId));
                    personBaseIds.AddRange(item.Relationships.Select(r => r.PersonId));
                    personBaseIds.AddRange(item.PositionRelationships.SelectMany(pr => pr.OrganizationalPersonPositions.Select(opp => opp.PersonId)));
                    personBaseIds.AddRange(item.PositionRelationships.SelectMany(pr => pr.RelatedOrganizationalPersonPositions.Select(ropp => ropp.PersonId)));
                }
                var personBaseObjects = await _personBaseRepository.GetPersonsBaseAsync(personBaseIds.Distinct());
                var orgPositionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.OrganizationalPersonPosition, Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition>(_adapterRegistry, _logger);
                foreach (var perPosition in organizationalPersonPositions)
                {
                    var orgPersonPositionDto = orgPositionAdapter.MapToType(perPosition);
                    orgPersonPositionDto.PersonName = personBaseObjects.Where(pb => pb.Id == orgPersonPositionDto.PersonId).ElementAt(0).PreferredName;
                    foreach (var relationship in orgPersonPositionDto.Relationships)
                    {
                        relationship.PersonName = personBaseObjects.Where(pb => pb.Id == relationship.PersonId).ElementAt(0).PreferredName;
                        relationship.RelatedPersonName = personBaseObjects.Where(pb => pb.Id == relationship.RelatedPersonId).ElementAt(0).PreferredName;
                    }
                    foreach (var positionRelationship in orgPersonPositionDto.PositionRelationships)
                    {
                        foreach (var posOrgPerPos in positionRelationship.OrganizationalPersonPositions)
                        {
                            posOrgPerPos.PersonName = personBaseObjects.Where(pb => pb.Id == posOrgPerPos.PersonId).ElementAt(0).PreferredName;
                        }
                        foreach (var posRelatedOrgPerPos in positionRelationship.RelatedOrganizationalPersonPositions)
                        {
                            posRelatedOrgPerPos.PersonName = personBaseObjects.Where(pb => pb.Id == posRelatedOrgPerPos.PersonId).ElementAt(0).PreferredName;
                        }
                    }
                    orgPersonPositionDtos.Add(orgPersonPositionDto);
                }
            }

            return orgPersonPositionDtos;
        }

        /// <summary>
        /// Throws an exception if the current user does not have the <see cref="BasePermissionCodes.ViewOrganizationalRelationships"/> 
        /// or <see cref="BasePermissionCodes.UpdateOrganizationalRelationships"/> permission.
        /// </summary>
        private void CheckViewOrUpdateOrganizationalRelationshipsPermission()
        {
            if (!HasPermission(BasePermissionCodes.ViewOrganizationalRelationships)
                && !HasPermission(BasePermissionCodes.UpdateOrganizationalRelationships))
            {
                throw new PermissionsException("User does not have permission to view organizational relationships.");
            }
        }
    }
}
