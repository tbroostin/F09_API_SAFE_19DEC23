// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A scheduled plan payment
    /// </summary>
    public class ActivityPaymentPlanScheduleItem : ActivityDateTermItem
    {
        /// <summary>
        /// Setup charge included in payment
        /// </summary>
        public decimal? SetupCharge { get; set; }

        /// <summary>
        /// Late charge included in payment
        /// </summary>
        public decimal? LateCharge { get; set; }

        /// <summary>
        /// Amount paid toward payment
        /// </summary>
        public decimal? AmountPaid { get; set; }

        /// <summary>
        /// Amount currently due on payment
        /// </summary>
        public decimal? NetAmountDue { get; set; }

        /// <summary>
        /// Date the scheduled payment was completely paid
        /// </summary>
        public DateTime? DatePaid { get; set; }
    }
}
