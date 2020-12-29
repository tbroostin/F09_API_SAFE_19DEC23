// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains information for a budget reporting unit.
    /// </summary>
    public class BudgetReportingUnit
    {
        /// <summary>
        /// The reporting unit ID.
        /// </summary>
        public string ReportingUnitId { get; set; }

        /// <summary>
        /// The reporting unit description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The reporting unit authorization date.
        /// </summary>
        public DateTime? AuthorizationDate { get; set; }

        /// <summary>
        /// Boolean indicator of whether the authorization date has passed.
        /// </summary>
        public bool HasAuthorizationDatePassed { get; set; }
    }
}
