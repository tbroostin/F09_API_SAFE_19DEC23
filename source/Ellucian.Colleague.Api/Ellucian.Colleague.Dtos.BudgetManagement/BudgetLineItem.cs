// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains the budget line item information for a working budget.
    /// </summary>
    public class BudgetLineItem
    {
        /// <summary>
        /// The budget line item GL account ID.
        /// </summary>
        public string BudgetAccountId { get; set; }

        /// <summary>
        /// The formatted budget line item GL account.
        /// </summary>
        public string FormattedBudgetAccountId { get; set; }

        /// <summary>
        /// The General Ledger class for the budget account.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))] 
        public GlClass GlClass { get; set; }

        /// <summary>
        /// The budget line item GL account description.
        /// </summary>
        public string BudgetAccountDescription { get; set; }

        /// <summary>
        /// Information for the budget officer assigned to this budget account.
        /// </summary>
        public BudgetOfficer BudgetOfficer { get; set; }

        /// <summary>
        /// Information for the budget reporting unit this budget account belongs to.
        /// </summary>
        public BudgetReportingUnit BudgetReportingUnit { get; set; }

        /// <summary>
        /// Budget amount from the base budget.
        /// </summary>
        public long BaseBudgetAmount { get; set; }

        /// <summary>
        /// Working budget amount.
        /// </summary>
        public long WorkingAmount { get; set; }

        /// <summary>
        /// Justification notes.
        /// </summary>
        public string JustificationNotes { get; set; }

        /// <summary>
        /// List of budget comparables containing comparable amounts.
        /// </summary>
        public List<BudgetComparable> BudgetComparables { get; set; }

    }
}