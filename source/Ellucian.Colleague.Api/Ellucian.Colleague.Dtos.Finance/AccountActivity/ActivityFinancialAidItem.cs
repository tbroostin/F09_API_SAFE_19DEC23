// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A financial aid award
    /// </summary>
    public class ActivityFinancialAidItem
    {
        /// <summary>
        /// ActivityFinancialAidItem constructor
        /// </summary>
        public ActivityFinancialAidItem()
        {
            AwardTerms = new List<ActivityFinancialAidTerm>();
        }

        /// <summary>
        /// Amount of the award
        /// </summary>
        public decimal? AwardAmount { get; set; }

        /// <summary>
        /// Award description
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// Comments on the award
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Amount of the award for which the student is not eligible
        /// </summary>
        public decimal? IneligibleAmount { get; set; }

        /// <summary>
        /// Amount of loan fees deducted from the award
        /// </summary>
        public decimal? LoanFee { get; set; }

        /// <summary>
        /// Amount paid on this award in other terms
        /// </summary>
        public decimal? OtherTermAmount { get; set; }

        /// <summary>
        /// Award code
        /// </summary>
        public string PeriodAward { get; set; }

        /// <summary>
        /// List of <see cref="ActivityFinancialAidTerm">financial aid payments</see>
        /// </summary>
        public List<ActivityFinancialAidTerm> AwardTerms { get; set; }

        /// <summary>
        /// Concatenation of newline-delimited award terms for the award for use with student statements
        /// </summary>
        public string StudentStatementAwardTerms { get; set; }

        /// <summary>
        /// Concatenation of newline-delimited disbursed amounts for the award for use with student statements
        /// </summary>
        public string StudentStatementDisbursedAmounts { get; set; }

        /// <summary>
        /// Concatenation of newline-delimited anticipated amounts for the award for use with student statements
        /// </summary>
        public string StudentStatementAnticipatedAmounts { get; set; }
    }
}
