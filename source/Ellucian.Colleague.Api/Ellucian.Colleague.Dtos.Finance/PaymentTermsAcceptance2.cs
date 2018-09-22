// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Data transfer object for accepting the terms of payment
    /// </summary>
    public class PaymentTermsAcceptance2
    {
        /// <summary>
        /// ID of student accepting the payment terms
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Registration payment control ID
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// Date and time of the registration acknowledgement
        /// </summary>
        public DateTimeOffset AcknowledgementDateTime { get; set; }

        /// <summary>
        /// Acknowledgement text displayed to student
        /// </summary>
        public IEnumerable<string> AcknowledgementText { get; set; }

        /// <summary>
        /// Terms and Conditions text displayed to student
        /// </summary>
        public IEnumerable<string> TermsText { get; set; }

        /// <summary>
        /// List of IDs of the sections displayed on the acknowledgement
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }

        /// <summary>
        /// List of invoice IDs summarized on the acknowledgement
        /// </summary>
        public IEnumerable<string> InvoiceIds { get; set; }

        /// <summary>
        /// Login ID of the user giving the approval
        /// </summary>
        public string ApprovalUserId { get; set; }

        /// <summary>
        /// Date and time at which the approval was given
        /// </summary>
        public DateTimeOffset ApprovalReceived { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PaymentTermsAcceptance2()
        {
            AcknowledgementText = new List<string>();
            TermsText = new List<string>();
            SectionIds = new List<string>();
            InvoiceIds = new List<string>();
        }
    }
}
