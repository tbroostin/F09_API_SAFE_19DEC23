/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
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

    }
}
