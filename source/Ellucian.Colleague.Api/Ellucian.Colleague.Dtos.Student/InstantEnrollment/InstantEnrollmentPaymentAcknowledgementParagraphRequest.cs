// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Request for an instant enrollment payment acknowledgement paragraph
    /// </summary>
    public class InstantEnrollmentPaymentAcknowledgementParagraphRequest
    {
        /// <summary>
        /// Unique identifier for the person for whom the instant enrollment payment acknowledgement paragraph will be retrieved.
        /// </summary>
        /// <remarks>Required</remarks>
        public string PersonId { get; set; }

        /// <summary>
        /// Unique identifier for a cash receipt whose payment is tied to the instant enrollment payment acknowledgement paragraph to be retrieved.
        /// </summary>
        /// <remarks>Optional. Included when there is a cash receipt associated with the registration. Not included when the registration had no cost and payment.</remarks>
        public string CashReceiptId { get; set; }

    }
}
