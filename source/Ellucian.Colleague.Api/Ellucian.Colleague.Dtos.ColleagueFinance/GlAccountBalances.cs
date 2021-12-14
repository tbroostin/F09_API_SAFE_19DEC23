// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Contains the balances of GL account.
    /// </summary>
    public class GlAccountBalances
    {
        /// <summary>
        /// GL account (internal format).
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// Formatted GL account.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// The GL account description
        /// </summary>
        public string GlAccountDescription { get; set; }

        /// <summary>
        /// The GL account budget amount.
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// The GL account encumbrance amount. 
        /// </summary>
        public decimal EncumbranceAmount { get; set; }

        /// <summary>
        /// The GL account requisition amount. 
        /// </summary>
        public decimal RequisitionAmount { get; set; }

        /// <summary>
        /// The GL account actual amount.
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// umbrella gl account.
        /// </summary>
        public string UmbrellaGlAccount { get; set; }

        /// <summary>
        /// whether gl account is poolee or non-pooled account
        /// given their GL account security setup.
        /// </summary>
        public bool IsPooleeAccount { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}