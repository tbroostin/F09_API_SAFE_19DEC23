// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Request for an instant enrollment payment cash receipt acknowledgement
    /// </summary>
    public class InstantEnrollmentCashReceiptAcknowledgementRequest
    {
        /// <summary>
        /// Unique identifier for an e-commerce payment transaction id that is tied to the instant enrollment registration.
        /// </summary>
        /// <remarks>Must be blank when a cash receipt id is given</remarks>
        public string TransactionId { get; set; }

        /// <summary>
        /// Unique identifier for a cash receipt id that is tied to the instant enrollment registration.
        /// </summary>
        /// <remarks>Must be blank when an e-commerce transaction id is given</remarks>
        public string CashReceiptId { get; set; }

        /// <summary>
        /// Unique identifier for the person for whom the instant enrollment registration was made.
        /// </summary>
        /// <remarks>Required</remarks>
        public string PersonId { get; set; }

    }
}
