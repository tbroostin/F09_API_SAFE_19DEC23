// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Repository for organizational positions
    /// </summary>
    public interface IOrganizationalPositionRepository
    {
        /// <summary>
        /// Retrieves organizational positions for the given ids
        /// </summary>
        /// <param name="ids">Organizational position ids</param>
        /// <returns>Organizational position entities</returns>
        Task<IEnumerable<OrganizationalPosition>> GetOrganizationalPositionsByIdsAsync(IEnumerable<string> ids);

        /// <summary>
        /// Retrieves organizational positions for the given search criteria or ids
        /// </summary>
        /// <param name="searchCriteria">Id or Partial position title</param>
        /// <param name="ids">Organizational position ids</param>
        /// <returns>Organizational position entities</returns>
        Task<IEnumerable<OrganizationalPosition>> GetOrganizationalPositionsAsync(string searchCriteria, IEnumerable<string> ids);
    }
}
