// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains the working budget information for a budget officer.
    /// </summary>
    public class WorkingBudget
    {
        /// <summary>
        /// The working budget ID.
        /// </summary>
        public List<BudgetLineItem> BudgetLineItems { get; set; }

        /// <summary>
        /// Total number of budget line items for working budget.
        /// </summary>
        public int TotalLineItems { get; set; }
    }
}
