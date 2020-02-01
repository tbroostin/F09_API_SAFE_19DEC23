// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Filter criteria for the working budget.
    /// </summary>
    public class WorkingBudgetQueryCriteria
    {
        /// <summary>
        /// List of IDs. Only 0 or 1 IDs will be accepted.
        /// </summary>
        public List<string> Ids { get; set; }

        /// <summary>
        /// Start position of the line items to return.
        /// </summary>
        public int StartLineItem { get; set; }

        /// <summary>
        /// Total number of budget line items in the working budget.
        /// </summary>
        public int LineItemCount { get; set; }

        /// <summary>
        /// List of budget officer IDs to filter the results.
        /// </summary>
        public List<string> BudgetOfficerIds { get; set; }

        /// <summary>
        /// List of budget reporting unit IDs to filter the results.
        /// </summary>
        public List<string> BudgetReportingUnitIds { get; set; }

        /// <summary>
        /// Whether to also include any descendants for the selected reporting units.
        /// </summary>
        public bool IncludeBudgetReportingUnitsChildren { get; set; }

        /// <summary>
        /// A list of component filter criteria.
        /// </summary>
        public IEnumerable<ComponentQueryCriteria> ComponentCriteria { get; set; }

        /// <summary>
        /// A list of sort/subtotal component filter criteria.
        /// </summary>
        public IEnumerable<SortSubtotalComponentQueryCriteria> SortSubtotalComponentQueryCriteria { get; set; }
    }
}
