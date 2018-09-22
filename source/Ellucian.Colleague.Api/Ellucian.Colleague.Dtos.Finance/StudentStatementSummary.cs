// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Summary portion of a student statement
    /// </summary>
    public class StudentStatementSummary
    {
        /// <summary>
        /// Collection of accounts receivable charge group descriptions and their overall group type balances
        /// </summary>
        public IEnumerable<ActivityTermItem> ChargeInformation { get; set; }

        /// <summary>
        /// Collection of accounts receivable non-charge activity descriptions, consolidated by activity type, 
        /// and their overall activity type balances
        /// </summary>
        public IEnumerable<ActivityTermItem> NonChargeInformation { get; set; }

        /// <summary>
        /// Total amount of any payment plan adjustments, equal to the amounts of all scheduled payments not currently due 
        /// in the statement term or period. Overdue scheduled payments and the next unpaid scheduled payment on each plan are currently due.
        /// </summary>
        public decimal PaymentPlanAdjustmentsAmount { get; set; }

        /// <summary>
        /// Total amount due for all deposits due in the statement term or period
        /// </summary>
        public decimal CurrentDepositsDueAmount { get; set; }

        /// <summary>
        /// Range of dates for the summary portion of the statement
        /// </summary>
        public string SummaryDateRange { get; set; }

        /// <summary>
        /// Description of the term or period for a statement
        /// </summary>
        public string TimeframeDescription { get; set; }
    }
}
