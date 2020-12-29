// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Contains a budget line item. 
    /// </summary>
    [Serializable]
    public class BudgetLineItem
    {
        /// <summary>
        /// Budget line item GL account.
        /// </summary>
        public string BudgetAccountId { get { return budgetAccountId; } }
        private string budgetAccountId { get; set; }

        /// <summary>
        /// Budget line item formatted GL account.
        /// </summary>
        public string FormattedBudgetAccountId { get; set; }

        /// <summary>
        /// The General Ledger class for the budget account.
        /// </summary>
        public GlClass GlClass { get; set; }

        /// <summary>
        /// Budget line item GL account description.
        /// </summary>
        public string BudgetAccountDescription { get; set; }

        /// <summary>
        /// Information for the budget officer information assigned to this budget account.
        /// </summary>
        public BudgetOfficer BudgetOfficer { get; set; }

        /// <summary>
        /// Information for the budget reporting unit this budget account belongs to.
        /// </summary>
        public BudgetReportingUnit BudgetReportingUnit { get; set; }

        /// <summary>
        /// Base budget amount for the budget line item.
        /// </summary>
        public long BaseBudgetAmount { get; set; }

        /// <summary>
        /// Budget line item working amount.
        /// </summary>
        public long WorkingAmount { get; set; }

        /// <summary>
        /// Justification notes.
        /// </summary>
        public string JustificationNotes { get; set; }

        /// <summary>
        /// List of budget comparables.
        /// </summary>
        public List<BudgetComparable> BudgetComparables { get; set; }

        /// <summary>
        /// Constructor that initializes a Budget Line Item entity.
        /// </summary>
        /// <param name="budgetAccountId">Budget line item GL account id.</param>
        public BudgetLineItem(string budgetAccountId)
        {
            if (string.IsNullOrEmpty(budgetAccountId))
            {
                throw new ArgumentNullException("budgetAccountId", "budgetAccountId is a required field.");
            }

            this.budgetAccountId = budgetAccountId;
            this.BudgetComparables = new List<BudgetComparable>();
        }

        /// <summary>
        /// Method to add a budget comparable to a budget line item.
        /// </summary>
        /// <param name="budgetComparable">A budget comparable.</param>
        public void AddBudgetComparable(BudgetComparable budgetComparable)
        {
            if (budgetComparable == null)
            {
                throw new ArgumentNullException("budgetComparable", "The budget comparable cannot be null.");
            }

            // Budget comparables will have unique comparable numbers.
            if (!BudgetComparables.Where(x => x.ComparableNumber == budgetComparable.ComparableNumber).Any())
            {
                BudgetComparables.Add(budgetComparable);
            }
        }

    }
}
