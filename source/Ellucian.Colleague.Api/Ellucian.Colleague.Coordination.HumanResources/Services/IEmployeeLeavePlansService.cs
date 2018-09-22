//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmployeeLeavePlans services
    /// </summary>
    public interface IEmployeeLeavePlansService : IBaseService
    {
        /// <summary>
        /// Gets all employee-leave-plans
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="EmployeeLeavePlans">employeeLeavePlans</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EmployeeLeavePlans>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a employeeLeavePlans by guid.
        /// </summary>
        /// <param name="guid">Guid of the employeeLeavePlans in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="EmployeeLeavePlans">employeeLeavePlans</see></returns>
        Task<Ellucian.Colleague.Dtos.EmployeeLeavePlans> GetEmployeeLeavePlansByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets current user leave plans
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansV2Async(string effectivePersonId = null, bool bypassCache = false);

    }
}
