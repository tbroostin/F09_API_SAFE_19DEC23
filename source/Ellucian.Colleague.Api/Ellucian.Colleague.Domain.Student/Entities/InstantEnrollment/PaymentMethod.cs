// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// A payment made on a receipt
    /// </summary>
    [Serializable]
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
