/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Criteria for getting an EmployeeLeavePlan object
    /// </summary>
    public class EmployeeLeavePlanQueryCriteria
    {
        /// <summary>
        /// Id of supervisor to get EmployeeLeavePlan objects for
        /// </summary>
        public string SupervisorId { get; set; }

        /// <summary>
        /// Id(s) of supervisees to get EmployeeLeavePlan objects for
        /// </summary>
        public IEnumerable<string> SuperviseeIds { get; set; }

        /// <summary>
        /// The lookup start date, all records with end date before this date will not be retrieved.
        /// If none specified, the endpoint will retrieve all available information
        /// </summary>
        public DateTime? LookupStartDate { get; set; }

    }
}
