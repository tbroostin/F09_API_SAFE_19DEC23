// Copyright 2017 Ellucian Company L.P. and its affiliates.
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
    /// <summary>
    /// Service for Organizational Positions
    /// </summary>
    [RegisterType]
    public class OrganizationalPositionService: BaseCoordinationService, IOrganizationalPositionService
    {
        private readonly IOrganizationalPositionRepository _organizationalPositionRepository;

        /// <summary>
        /// Constructor for OrganizationalPositionService
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="organizationalPositionRepository">Organizational position repository</param>
        /// <param name="logger">logger</param>
        /// <param name="configurationRepository">Configuratoin repository</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="currentUserFactory">Current user factory</param>
        public OrganizationalPositionService(IAdapterRegistry adapterRegistry, IOrganizationalPositionRepository organizationalPositionRepository, ILogger logger,
            IConfigurationRepository configurationRepository, IRoleRepository roleRepository, ICurrentUserFactory currentUserFactory)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _organizationalPositionRepository = organizationalPositionRepository;
        }

        /// <summary>
        /// Gets a single organizational position for the given id
        /// </summary>
        /// <param name="id">Organizational position id</param>
        /// <returns>Organizational position dto</returns>
        public async Task<Dtos.Base.OrganizationalPosition> GetOrganizationalPositionByIdAsync(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Cannot have a null or empty id");
            }
            var ids = new List<string> { id };
            var organizationalPositionEntities = await _organizationalPositionRepository.GetOrganizationalPositionsByIdsAsync(ids);
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalPosition, Dtos.Base.OrganizationalPosition>();
            if (organizationalPositionEntities.Count() == 1)
            {
                return entityToDtoAdapter.MapToType(organizationalPositionEntities.First());
            }
            return null;
        }

        /// <summary>
        /// Retrieves organizational position matching the given criteria
        /// </summary>
        /// <param name="criteria">Organizational position query criteria</param>
        /// <returns>Matching organizational position dtos</returns>
        public async Task<IEnumerable<Dtos.Base.OrganizationalPosition>> QueryOrganizationalPositionsAsync(Dtos.Base.OrganizationalPositionQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Cannot have a null criteria");
            }
            if ((criteria.Ids == null || !criteria.Ids.Any()) && string.IsNullOrEmpty(criteria.SearchString))
            {
                throw new ArgumentException("You must provide IDs or a search string.", "criteria");
            }
            var organizationalPositionEntities = await _organizationalPositionRepository.GetOrganizationalPositionsAsync(criteria.SearchString, criteria.Ids);
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalPosition, Dtos.Base.OrganizationalPosition>();
            var dtoList = new List<Dtos.Base.OrganizationalPosition>();
            foreach (var organizationalPositionEntity in organizationalPositionEntities)
            {
                dtoList.Add(entityToDtoAdapter.MapToType(organizationalPositionEntity));
            }
            return dtoList;
        }
    }
}
