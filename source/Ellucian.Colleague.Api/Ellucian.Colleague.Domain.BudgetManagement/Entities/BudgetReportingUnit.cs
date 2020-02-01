// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains information fo a budget reporting unit.
    /// </summary>
    [Serializable]
    public class BudgetReportingUnit
    {
        /// <summary>
        /// The reporting unit ID.
        /// </summary>
        public string ReportingUnitId { get { return reportingUnitId; } }
        private string reportingUnitId { get; set;}

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

        /// <summary>
        /// Constructor that initializes a reporting unit entity.
        /// </summary>
        public BudgetReportingUnit(string reportingUnitId)
        {
            this.reportingUnitId = reportingUnitId;
        }
    }
}
