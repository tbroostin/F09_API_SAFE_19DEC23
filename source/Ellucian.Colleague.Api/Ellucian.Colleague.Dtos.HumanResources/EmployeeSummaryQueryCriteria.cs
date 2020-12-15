/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Criteria for getting an EmployeeSummary object
    /// </summary>
    public class EmployeeSummaryQueryCriteria
    {
        /// <summary>
        /// The supervisor Id for an employee; when provided, all employees that are supervised by this supervisor will be returned
        /// </summary>
        public string EmployeeSupervisorId { get; set; }

        /// <summary>
        /// A list of employee Ids to get EmployeeSummary objects for; if provided with a supervisor Id, all EmployeeIds must be supervised
        /// by the provided EmployeeSupervisorId.
        /// </summary>
        public List<string> EmployeeIds { get; set; }

        /// <summary>
        /// The lookup start date, all records with end date before this date will not be retrieved.
        /// If none specified, the endpoint will retrieve all available information
        /// </summary>
        public DateTime? LookupStartDate { get; set; }

    }
}
