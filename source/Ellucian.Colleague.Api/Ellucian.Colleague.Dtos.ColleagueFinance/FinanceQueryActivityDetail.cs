// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The Finance Query activity detail DTO.
    /// </summary>
    public class FinanceQueryActivityDetail
    {
        /// <summary>
        /// The GL account.
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// The GL account formatted for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// The GL account description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Budget pool indicator; Umbrella, Poolee, or null
        /// </summary>
        public string BudgetPoolIndicator { get; set; }

        /// <summary>
        /// Set of budget activity detail for the GL account.
        /// </summary>
        public List<FinanceQueryGlTransaction> Transactions { get; set; }
    }
}
