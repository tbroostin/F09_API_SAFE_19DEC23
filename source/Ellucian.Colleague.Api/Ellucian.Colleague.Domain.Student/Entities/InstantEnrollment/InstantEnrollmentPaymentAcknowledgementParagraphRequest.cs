// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Request for an instant enrollment payment acknowledgement paragraph
    /// </summary>
    [Serializable]
    public class InstantEnrollmentPaymentAcknowledgementParagraphRequest
    {
        /// <summary>
        /// Unique identifier for the person for whom the instant enrollment payment acknowledgement paragraph will be retrieved.
        /// </summary>
        /// <remarks>Required</remarks>
        public string PersonId { get; private set; }

        /// <summary>
        /// Unique identifier for a cash receipt whose payment is tied to the instant enrollment payment acknowledgement paragraph to be retrieved.
        /// </summary>
        /// <remarks>Optional. Included when there is a cash receipt associated with the registration. Not included when the registration had no cost and payment.</remarks>
        public string CashReceiptId { get; private set; }

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="personId">Unique identifier for the person for whom the instant enrollment payment acknowledgement paragraph will be retrieved; required.</param>
        /// <param name="cashReceiptId">Unique identifier for a cash receipt whose payment is tied to the instant enrollment payment acknowledgement paragraph to be retrieved; optional.</param>
        public InstantEnrollmentPaymentAcknowledgementParagraphRequest(string personId, string cashReceiptId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person ID is required when retrieving an instant enrollment payment acknowledgement paragraph.");
            }
            PersonId = personId;
            CashReceiptId = cashReceiptId;
        }
    }
}
