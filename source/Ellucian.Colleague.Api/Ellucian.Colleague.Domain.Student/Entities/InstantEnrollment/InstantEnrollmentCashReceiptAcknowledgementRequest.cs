// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Request for an instant enrollment payment cash receipt acknowledgement
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="transactionId">Unique identifier for an e-commerce transcation payment.</param>
        /// <param name="cashReceiptId">Unique identifier for a cash receipt.</param>
        /// <param name="personId">Unique identifier for the person for who made the instant enrollment payment.</param>
        public InstantEnrollmentCashReceiptAcknowledgementRequest(string transactionId, string cashReceiptId, string personId)
        {
            if (string.IsNullOrEmpty(transactionId) && string.IsNullOrEmpty(cashReceiptId))
            {
                throw new ArgumentNullException("transactionId", "Either a transaction ID or a cash receipt ID is required when retrieving an instant enrollment cash receipt acknowledgement.");
            }

            //person id is required when retreiving acknowledgement by cash receipt id
            //if (!string.IsNullOrEmpty(cashReceiptId) && string.IsNullOrEmpty(personId))
            //{
            //    throw new ArgumentNullException("personId", "A person ID is required when retrieving an instant enrollment payment acknowledgement paragraph.");
            //}

            TransactionId = transactionId;
            CashReceiptId = cashReceiptId;
            PersonId = personId;
        }

    }
}
