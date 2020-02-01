/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.HumanResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmployeeSummaryService
    {
        /// <summary>
        /// Returns EmployeeSummary objects for requested criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <exception cref="ArgumentNullException">Critera must be provided</exception>
        /// <exception cref="PermissionsException">Authenticated user must be a supervisor or have access to requsted summary objects</exception>
        /// <returns>A collection of EmployeeSummary dtos</returns>
        Task<IEnumerable<EmployeeSummary>> QueryEmployeeSummaryAsync(EmployeeSummaryQueryCriteria criteria);
    }
}
