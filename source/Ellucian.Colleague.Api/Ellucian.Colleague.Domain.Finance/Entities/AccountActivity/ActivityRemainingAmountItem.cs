// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityRemainingAmountItem : ActivityDateTermItem
    {
        /// <summary>
        /// Amount of deposit originally paid
        /// </summary>
        public decimal? PaidAmount { get; set; }

        /// <summary>
        /// Amount of deposit remaining to be paid on the account
        /// </summary>
        public decimal? RemainingAmount { get; set; }

        /// <summary>
        /// Amount of deposit refunded
        /// </summary>
        public decimal? RefundAmount { get; set; }

        /// <summary>
        /// Amount of deposit currently unavailable to be used
        /// </summary>
        public decimal? OtherAmount { get; set; }

        /// <summary>
        /// Deposit's associated cash receipt ID
        /// </summary>
        public string ReceiptId { get; set; }
    }
}
