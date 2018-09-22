// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A deposit on an account
    /// </summary>
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
