// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IOrganizationalPersonPositionService
    {

        /// <summary>
        /// Returns OrganizationalPersonPosition for the given ID
        /// </summary>
        /// <param name="id">Organizational person position ID</param>
        /// <returns>OrganizationalPersonPosition dto for the given ID</returns>
        Task<OrganizationalPersonPosition> GetOrganizationalPersonPositionByIdAsync(string id);

        Task<IEnumerable<OrganizationalPersonPosition>> QueryOrganizationalPersonPositionAsync(OrganizationalPersonPositionQueryCriteria criteria);
    }
}
