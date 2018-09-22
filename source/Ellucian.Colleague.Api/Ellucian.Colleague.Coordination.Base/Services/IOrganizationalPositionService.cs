// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service for working with Organizational Positions
    /// </summary>
    public interface IOrganizationalPositionService
    {
        /// <summary>
        /// Constructor for OrganizationalPositionService
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="organizationalPositionRepository">Organizational position repository</param>
        /// <param name="logger">logger</param>
        /// <param name="configurationRepository">Configuratoin repository</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="currentUserFactory">Current user factory</param>
        Task<Dtos.Base.OrganizationalPosition> GetOrganizationalPositionByIdAsync(string id);

        /// <summary>
        /// Gets a single organizational position for the given id
        /// </summary>
        /// <param name="id">Organizational position id</param>
        /// <returns>Organizational position dto</returns>
        Task<IEnumerable<Dtos.Base.OrganizationalPosition>> QueryOrganizationalPositionsAsync(Dtos.Base.OrganizationalPositionQueryCriteria criteria);
    }
}
