// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A registration approval
    /// </summary>
    public class RegistrationTermsApproval2
    {
        /// <summary>
        /// Internal ID of the registration approval
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of student for whom approval is needed
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Date and Time of the registration approval
        /// </summary>
        public DateTimeOffset AcknowledgementTimestamp { get; set; }

        /// <summary>
        /// ID of the IPC registration to which the registration approval is tied
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// List of sections associated with the registration approval
        /// </summary>
        public List<string> SectionIds { get; set; }

        /// <summary>
        /// List of invoices associated with the registration approval
        /// </summary>
        public List<string> InvoiceIds { get; set; }

        /// <summary>
        /// ID of approval response for Terms and Conditions Approval
        /// </summary>
        public string TermsResponseId { get; set; }

        /// <summary>
        /// ID of approval document containing acknowledgement text
        /// </summary>
        public string AcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Class constructor - initializes lists
        /// </summary>
        public RegistrationTermsApproval2()
        {
            SectionIds = new List<string>();
            InvoiceIds = new List<string>();
        }
    }
}
