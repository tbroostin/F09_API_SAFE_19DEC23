// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains budget line items for a budget officer.
    /// </summary>
    [Serializable]
    public class WorkingBudget
    {
        /// <summary>
        /// List of budget line items for the working budget.
        /// </summary>
        public List<BudgetLineItem> BudgetLineItems { get; set; }

        /// <summary>
        /// Total number of budget line items for working budget.
        /// </summary>
        public int TotalLineItems { get; set; }

        /// <summary>
        /// Constructor that initializes a working budget entity.
        /// </summary>
        public WorkingBudget()
        {
            this.BudgetLineItems = new List<BudgetLineItem>();
        }

        /// <summary>
        /// Method to add a budget line item to a working budget.
        /// </summary>
        /// <param name="budgetLineItem">A budget line item.</param>
        public void AddBudgetLineItem(BudgetLineItem budgetLineItem)
        {
            if (budgetLineItem == null)
            {
                throw new ArgumentNullException("budgetLineItem", "The budget line item cannot be null.");
            }

            // Budget line items will have unique budget account IDs.
            if (!BudgetLineItems.Where(x => x.BudgetAccountId == budgetLineItem.BudgetAccountId).Any())
            {
                BudgetLineItems.Add(budgetLineItem);
            }
        }
    }
}
