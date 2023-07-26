/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IOrganizationalChartService : IBaseService
    {
        /// <summary>
        /// Returns all OrgChartEmployee objects for for one level of requested criteria
        /// </summary>
        /// <param name="rootEmployeeId">The ID of the root employee for the org chart</param>
        /// <exception cref="PermissionsException">Authenticated user must have access to requsted summary objects</exception>
        /// <returns>A collection of OrgChartEmployee dtos</returns>
        Task<IEnumerable<OrgChartEmployee>> GetOrganizationalChartAsync(string rootEmployeeId);

        /// <summary>
        /// Returns single OrgChartEmployee object for requested criteria
        /// </summary>
        /// <param name="rootEmployeeId">The ID of the root employee for the org chart</param>
        /// <exception cref="PermissionsException">Authenticated user must have access to requsted summary objects</exception>
        /// <returns>A single OrgChartEmployee dtos</returns>
        Task<OrgChartEmployee> GetOrganizationalChartEmployeeAsync(string rootEmployeeId);

        /// <summary>
        /// Gets a list of employees matching the given search criteria.
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria</param>
        /// <returns>A list of <see cref="EmployeeSearchResult"> objects.</see></returns>
        Task<IEnumerable<EmployeeSearchResult>> QueryEmployeesByPostAsync(EmployeeNameQueryCriteria criteria);
    }
}
