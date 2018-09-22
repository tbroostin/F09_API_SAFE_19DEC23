/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmployeeService : IBaseService
    {
        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status types.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee"/> and count for paging.</returns>
        Task<Tuple<IEnumerable<Dtos.Employee>, int>> GetEmployeesAsync(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "");
        
        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="guid">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee"./></returns>
        Task<Employee> GetEmployeeByGuidAsync(string guid);



        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status types.</param>
        /// <param name="contractType">Contract type filter.</param>
        /// <param name="contractDetail">Contract detail filter.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee2"/> and count for paging.</returns>
        Task<Tuple<IEnumerable<Dtos.Employee2>, int>> GetEmployees2Async(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
            string contractType = "", string contractDetail = "");

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="id">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee2"./></returns>
        Task<Employee2> GetEmployee2ByIdAsync(string id);

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status types filter.</param>
        /// <param name="contractType">Contract type filter.</param>
        /// <param name="contarctDetail">Contract detail filter.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee2"/> and count for paging.</returns>
        Task<Tuple<IEnumerable<Dtos.Employee2>, int>> GetEmployees3Async(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "", 
            string contractType = "", string contractDetail = "");

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="id">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee2"./></returns>
        Task<Employee2> GetEmployee3ByIdAsync(string id);

        /// <summary>
        /// Update an existing employee v12
        /// </summary>
        /// <param name="guid">Employee GUID for update.</param>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>A employeeDto object <see cref="Dtos.Employee2"/> in EEDM format</returns>
        Task<Dtos.Employee2> PutEmployee2Async(string guid, Dtos.Employee2 employeeDto, Dtos.Employee2 origEmployeeDto);

        /// <summary>
        /// Create a new employee record v12
        /// </summary>
        /// <param name="employee2Dto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        Task<Dtos.Employee2> PostEmployee2Async(Dtos.Employee2 employeeDto);

        /// <summary>
        /// Gets a list of employees that match person query criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>list of employees</returns>
        Task<IEnumerable<Dtos.Base.Person>> QueryEmployeeNamesByPostAsync(Dtos.Base.EmployeeNameQueryCriteria criteria);
        
    }
}
