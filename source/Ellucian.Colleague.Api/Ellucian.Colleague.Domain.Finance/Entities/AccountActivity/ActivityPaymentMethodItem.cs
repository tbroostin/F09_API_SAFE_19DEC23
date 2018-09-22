// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityPaymentMethodItem : ActivityDateTermItem
    {
        /// <summary>
        /// Payment method for the original payment
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public RefundVoucherStatus Status { get; set; }

        /// <summary>
        /// Date associated with this item's status
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// E-Commerce transaction number 
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Last 4 digits of the credit card for the original payment
        /// </summary>
        public string CreditCardLastFourDigits { get; set; }

        /// <summary>
        /// Number of the check for the original payment
        /// </summary>
        public string CheckNumber { get; set; }

        /// <summary>
        /// Date of the check for the original payment
        /// </summary>
        public DateTime? CheckDate { get; set; }
    }
}
