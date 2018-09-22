// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// A payment made on a receipt
    /// </summary>
    public class PaymentMethod
    {
        /// <summary>
        /// Payment method code
        /// </summary>
        public string PayMethodCode { get; set; }

        /// <summary>
        /// Payment method description
        /// </summary>
        public string PayMethodDescription { get; set; }

        /// <summary>
        /// Payment control number
        /// </summary>
        public string ControlNumber { get; set; }

        /// <summary>
        /// Payment confirmation number
        /// </summary>
        public string ConfirmationNumber { get; set; }

        /// <summary>
        /// Payment transaction number
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Payment description
        /// </summary>
        public string TransactionDescription { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public Nullable<Decimal> TransactionAmount { get; set; }
    }
}
