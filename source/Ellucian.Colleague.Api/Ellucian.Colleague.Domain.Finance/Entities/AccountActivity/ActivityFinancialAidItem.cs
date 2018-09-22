// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// Financial Aid item for Account Activity
    /// </summary>
    [Serializable]
    public class ActivityFinancialAidItem
    {
        /// <summary>
        /// Constructor for ActivityFinancialAidItem
        /// </summary>
        public ActivityFinancialAidItem()
        {
            AwardTerms = new List<ActivityFinancialAidTerm>();
        }

        /// <summary>
        /// Award amount
        /// </summary>
        public decimal? AwardAmount { get; set; }

        /// <summary>
        /// Award description
        /// </summary>
        public string AwardDescription { get; set; }

        /// <summary>
        /// Award comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Amount of the award for which the student is not currently eligible
        /// </summary>
        public decimal? IneligibleAmount { get; set; }

        /// <summary>
        /// Amount of any loan fee associated with the award
        /// </summary>
        public decimal? LoanFee { get; set; }

        /// <summary>
        /// Amount of the award associated with other award terms
        /// </summary>
        public decimal? OtherTermAmount { get; set; }

        /// <summary>
        /// Period award
        /// </summary>
        public string PeriodAward { get; set; }

        /// <summary>
        /// Financial aid terms to which the award applies
        /// </summary>
        public List<ActivityFinancialAidTerm> AwardTerms { get; set; }

        /// <summary>
        /// Newline-delimited concatenation of the award terms for the award, used for display on student statements
        /// </summary>
        public string StudentStatementAwardTerms
        {
            get
            {
                if (AwardTerms.Count > 0)
                {
                    return String.Join(Environment.NewLine, AwardTerms.Select(at => at.AwardTerm));
                }
                return null;
            }
        }

        /// <summary>
        /// Newline-delimited concatenation of the disbursed amounts for the award, used for display on student statements
        /// </summary>
        public string StudentStatementDisbursedAmounts
        {
            get
            {
                if (AwardTerms.Count > 0)
                {
                    NumberFormatInfo noParens = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                    noParens.CurrencyNegativePattern = 1;
                    return String.Join(Environment.NewLine, AwardTerms.Select(at => (at.DisbursedAmount != null) ? (((decimal)at.DisbursedAmount).ToString("C", noParens)) : null));
                }
                return null;
            }
        }

        /// <summary>
        /// Newline-delimited concatenation of the anticipated amounts for the award, used for display on student statements
        /// </summary>
        public string StudentStatementAnticipatedAmounts
        {
            get
            {
                if (AwardTerms.Count > 0)
                {
                    NumberFormatInfo noParens = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                    noParens.CurrencyNegativePattern = 1;
                    return String.Join(Environment.NewLine, AwardTerms.Select(at => (at.AnticipatedAmount != null) ? (((decimal)at.AnticipatedAmount).ToString("C", noParens)) : null));
                }
                return null;
            }
        }
    }
}
