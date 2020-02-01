// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains the working budget information for the user.
    /// </summary>
    public class WorkingBudget2
    {
        /// <summary>
        /// Total number of budget line items for working budget.
        /// </summary>
        public int TotalLineItems { get; set; }

        /// <summary>
        /// List of line items for the working budget.
        /// It contains as many budget line items as requested per page plus
        /// as many subtotals are present in that page.
        /// </summary>
        public List<LineItem> LineItems { get; set; }

    }
}
