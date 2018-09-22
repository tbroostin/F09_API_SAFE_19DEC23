/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IEmployeeRepository : IEthosExtended
    {
        /// <summary>
        /// Gets a list of employee keys
        /// </summary>
        /// <param name="ids">Optional: List of Ids to limit the query to</param>
        /// <param name="hasOnlineEarningsStatementConsentFilter">Optional: Online Earnings Statement Consent filter</param>
        /// <param name="includeNonEmployees">Boolean to include non-employees in query</param>
        /// <param name="activeOnly">Boolean to return only active users</param>
        /// <returns>List of keys for employees</returns>
        Task<IEnumerable<string>> GetEmployeeKeysAsync(IEnumerable<string> ids = null, bool? hasOnlineEarningsStatementConsentFilter = null, bool includeNonEmployees = false, bool activeOnly = false);

        /// <summary>
        /// Get Employees objects for all employees bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatus">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractTypeCodes">Contract types filter.</param>
        /// <param name="contractDetailTypeCode">Contract detail filter.</param>
        /// <returns>Tuple of Employee Entity objects <see cref="Employee"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<Employee>, int>> GetEmployeesAsync(int offset, int limit, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
            IEnumerable<string> contractTypeCodes = null, string contractDetailTypeCode = "");

        /// <summary>
        /// Get Employee object for the specified guid
        /// </summary>   
        /// <param name="guid">guid of the employees record.</param>
        /// <returns>Employee Entity <see cref="Employee"./></returns>
        Task<Employee> GetEmployeeByGuidAsync(string guid);

        /// <summary>
        /// Get Employees objects for all employees bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatus">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractTypeCodes">Contract types filter.</param>
        /// <param name="contractDetailTypeCode">Contract detail filter.</param>
        /// <returns>Tuple of Employee Entity objects <see cref="Employee"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<Employee>, int>> GetEmployees2Async(int offset, int limit, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "" ,
            IEnumerable<string> contractTypeCodes = null, string contractDetailTypeCode = "");

        /// <summary>
        /// Get Employee object for the specified guid
        /// </summary>   
        /// <param name="guid">guid of the employees record.</param>
        /// <returns>Employee Entity <see cref="Employee"./></returns>
        Task<Employee> GetEmployee2ByGuidAsync(string guid);


        /// <summary>
        /// Get Unidata formatted date for filters.
        /// </summary>   
        /// <param name="date">date </param>
        /// <returns>date in undiata format</returns>
        Task<string> GetUnidataFormattedDate(string date);
               
        /// <summary>
        /// Update an existing employee v12
        /// </summary>
        /// <param name="employeeEntity">Employee entity to be updated</param>
        /// <returns>A employee entity object <see cref="Employee"/> in EEDM format</returns>
        Task<Employee> UpdateEmployee2Async(Employee employeeEntity);

        /// <summary>
        /// Create a new employee record v12
        /// </summary>
        /// <param name="employeeEntity">Employee entity to be updated</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        Task<Employee> CreateEmployee2Async(Employee employeeEntity);

        /// <summary>
        /// Get the employee ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The employee ID</returns>
        Task<string> GetEmployeeIdFromGuidAsync(string guid);
    }
}
