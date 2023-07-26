// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Attributes;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// Information needed to complete processing a payment
    /// </summary>
    public class PaymentConfirmation
    {
        /// <summary>
        /// PaymentConfirmation constructor
        /// </summary>
        public PaymentConfirmation()
        {
            ConfirmationText = new List<string>();
        }

        /// <summary>
        /// ID of payment provider account
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string ProviderAccount { get; set; }

        /// <summary>
        /// Code of convenience fee required on payment
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string ConvenienceFeeCode { get; set; }

        /// <summary>
        /// Description of convenience fee required on payment
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string ConvenienceFeeDescription { get; set; }

        /// <summary>
        /// Amount of convenience fee required on payment
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public Nullable<Decimal> ConvenienceFeeAmount { get; set; }

        /// <summary>
        /// GL number of convenience fee required on payment
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string ConvenienceFeeGeneralLedgerNumber { get; set; }

        /// <summary>
        /// Text to display with payment confirmation/review page
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public List<string> ConfirmationText { get; set; }

        /// <summary>
        /// Error message if payment cannot be processed
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string ErrorMessage { get; set; }
    }
}
