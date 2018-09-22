// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A refund transaction
    /// </summary>
    public class ActivityPaymentMethodItem : ActivityDateTermItem
    {
        /// <summary>
        /// Method of payment for the refund
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
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
