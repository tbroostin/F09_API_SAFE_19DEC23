// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A registration approval
    /// </summary>
    public class RegistrationTermsApproval
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
        public DateTime Timestamp { get; set; }

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
        /// Associated Terms and Conditions document
        /// </summary>
        public ApprovalDocument TermsDocument { get; set; }

        /// <summary>
        /// Associated Terms and Conditions Approval
        /// </summary>
        public ApprovalResponse TermsResponse { get; set; }

        /// <summary>
        /// Associated approval document
        /// </summary>
        public ApprovalDocument AcknowledgementDocument { get; set; }

        /// <summary>
        /// Class constructor - initializes lists
        /// </summary>
        public RegistrationTermsApproval()
        {
            SectionIds = new List<string>();
            InvoiceIds = new List<string>();
        }
    }
}
