// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A financial aid award for a term
    /// </summary>
    public class ActivityFinancialAidTerm
    {
        /// <summary>
        /// Award term ID
        /// </summary>
        public string AwardTerm { get; set; }

        /// <summary>
        /// Amount paid to the student
        /// </summary>
        public decimal? DisbursedAmount { get; set; }

        /// <summary>
        /// Amount anticipated to be paid
        /// </summary>
        public decimal? AnticipatedAmount { get; set; }
    }
}
