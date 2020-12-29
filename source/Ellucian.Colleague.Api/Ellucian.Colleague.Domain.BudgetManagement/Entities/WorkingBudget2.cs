// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains the working budget information for the user.
    /// </summary>
    [Serializable]
    public class WorkingBudget2
    {
        /// <summary>
        /// Total number of budget line items for working budget.
        /// </summary>
        public int TotalLineItems { get; set; }

        /// <summary>
        /// List of line items for the working budget.
        /// It contains as many budget line items as requested per page plus 
        /// as many subtotals that are present in that page.
        /// </summary>
        public ReadOnlyCollection<LineItem> LineItems { get; private set; }
        private readonly List<LineItem> lineItems = new List<LineItem>();

        /// <summary>
        /// Constructor that initializes a working budget entity.
        /// </summary>
        public WorkingBudget2()
        {
            LineItems = lineItems.AsReadOnly();
        }

        /// <summary>
        /// Method to add a line item to a working budget.
        /// </summary>
        /// <param name="lLineItem">A line item for the working budget.</param>
        public void AddLineItem(LineItem lineItem)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException("lineItem", "The line item cannot be null.");
            }

            lineItems.Add(lineItem);
        }

        /// <summary>
        /// Remove all line items from the list in the working budget.
        /// </summary>
        public void RemoveLineItems()
        {
            lineItems.RemoveAll(x => true);
        }
    }
}
