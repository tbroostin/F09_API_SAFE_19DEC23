/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IEmployeeLeavePlansRepository
    {
        /// <summary>
        /// Get EmployeeLeavePlans objects for all EmployeeLeavePlans bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        
        /// <returns>Tuple of Perleave Entity objects <see cref="Perleave"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<Perleave>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get EmployeeLeavePlans objects for a specific guid.
        /// </summary>   
        /// <param name="guid">guid of the EmployeeLeavePlans record.</param>
        /// <returns>Perleave Entity <see cref="Perleave"./></returns>
        Task<Perleave> GetEmployeeLeavePlansByGuidAsync(string guid);

        /// <summary>
        /// Get EmployeeLeavePlans objects for a specific Id.
        /// </summary>   
        /// <param name="id">guid of the EmployeeLeavePlans record.</param>
        /// <returns>Perleave Entity <see cref="Perleave"./></returns>
        Task<Perleave> GetEmployeeLeavePlansByIdAsync(string id);


        /// <summary>
        /// Gets all EmployeeLeavePlan entities for the current given employee Id. Entities are created using a combination of data from Perleave
        /// and the reference entities passed into the repository method.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="leavePlans"></param>
        /// <param name="leaveTypes"></param>
        /// <param name="earnTypes"></param>
        /// <param name="includeLeavePlansWithNoEarningsTypes"></param>
        /// <returns>EmployeeLeavePlan Entities for Persion Id <see cref="EmployeeLeavePlan"./></returns>
        Task<IEnumerable<EmployeeLeavePlan>> GetEmployeeLeavePlansByEmployeeIdsAsync(IEnumerable<string> employeeIds, 
            IEnumerable<LeavePlan> leavePlans, 
            IEnumerable<LeaveType> leaveTypes, 
            IEnumerable<EarningType2> earnTypes,
            bool includeLeavePlansWithNoEarningsTypes = false);

        /// <summary>
        /// Get Collection of PerLeave GUIDs and IDs
        /// </summary>
        /// <param name="perleaveIds">collection of PerLeave ids</param>
        /// <returns>Dictionary consisting of a perLeaveId (key) and guid (value)</returns>
        Task<Dictionary<string, string>> GetPerleaveGuidsCollectionAsync(IEnumerable<string> perleaveIds);



    }
}
