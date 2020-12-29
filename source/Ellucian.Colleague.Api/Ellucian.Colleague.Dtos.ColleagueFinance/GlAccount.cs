// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A General Ledger account.
    /// </summary>
    public class GlAccount
    {
        /// <summary>
        /// GL account number.
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// GL account description.
        /// </summary>
        public string GlAccountDescription { get; set; }

        /// <summary>
        /// Formatted GL account number for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }
    }
}
