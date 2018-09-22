// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IOrganizationalPersonPositionRepository
    {

        /// <summary>
        /// Get the position assignments and relevant relationships for the given IDs
        /// </summary>
        /// <param name="ids">Organizational Person Position IDs</param>
        /// <returns>The Collection of Organizational Person Positions with the given IDs</returns>
        Task<IEnumerable<Domain.Base.Entities.OrganizationalPersonPosition>> GetOrganizationalPersonPositionsByIdsAsync(IEnumerable<string> ids);

        /// <summary>
        /// Gets all the slots and relationships in the organizational structure
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Domain.Base.Entities.OrganizationalPersonPosition>> GetOrganizationalPersonPositionAsync(IEnumerable<string> personIds, IEnumerable<string> ids = null);
    }
}
