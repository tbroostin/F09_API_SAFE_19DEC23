// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class PaymentTermsAcceptance
    {
        private readonly string _studentId;
        private readonly string _paymentControlId;
        private readonly DateTimeOffset _acknowledgementDateTime;
        private readonly List<string> _invoiceIds = new List<string>();
        private readonly List<string> _sectionIds = new List<string>();
        private readonly List<string> _termsText = new List<string>();
        private readonly string _approvalUserId;
        private readonly DateTimeOffset _approvalReceived;

        /// <summary>
        /// Student ID
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// ID of registration payment control
        /// </summary>
        public string PaymentControlId { get { return _paymentControlId; } }

        /// <summary>
        /// Date time shown on acknowledgement form
        /// </summary>
        public DateTimeOffset AcknowledgementDateTime { get { return _acknowledgementDateTime; } }

        /// <summary>
        /// List of invoice IDs for this registration
        /// </summary>
        public ReadOnlyCollection<string> InvoiceIds { get; private set; }

        /// <summary>
        /// List of section IDs in which the student is registered
        /// </summary>
        public ReadOnlyCollection<string> SectionIds { get; private set; }

        /// <summary>
        /// Text of the Terms and Conditions
        /// </summary>
        public ReadOnlyCollection<string> TermsText { get; private set; }

        /// <summary>
        /// Login ID of the user granting approval
        /// </summary>
        public string ApprovalUserId { get { return _approvalUserId; } }

        /// <summary>
        /// Date and time approval was received from the approver
        /// </summary>
        public DateTimeOffset ApprovalReceived { get { return _approvalReceived; } }

        public List<string> AcknowledgementText { get; set; }

        public PaymentTermsAcceptance(string studentId, string payControlId, DateTimeOffset ackDateTime, IEnumerable<string> invoiceIds, IEnumerable<string> sectionIds,
            IEnumerable<string> termsText, string approvalUserId, DateTimeOffset approvalReceived)
        {
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required");
            }
            if (String.IsNullOrEmpty(payControlId))
            {
                throw new ArgumentNullException("payControlId", "Payment control ID is required");
            }
            if (invoiceIds == null || invoiceIds.Count() < 1)
            {
                throw new ArgumentNullException("invoiceIds", "One or more invoice IDs are required");
            }
            if (sectionIds == null || sectionIds.Count() < 1)
            {
                throw new ArgumentNullException("sectionIds", "One or more section IDs are required");
            }
            if (termsText == null || termsText.Count() < 1)
            {
                throw new ArgumentNullException("termsText", "Terms and conditions text is required");
            }
            if (String.IsNullOrEmpty(approvalUserId))
            {
                throw new ArgumentNullException("approvalUserId", "Approval user ID is required");
            }

            _studentId = studentId;
            _paymentControlId = payControlId;
            _acknowledgementDateTime = ackDateTime;
            _invoiceIds = invoiceIds.ToList();
            _sectionIds = sectionIds.ToList();
            _termsText = termsText.ToList();
            _approvalUserId = approvalUserId;
            _approvalReceived = approvalReceived;

            InvoiceIds = _invoiceIds.AsReadOnly();
            SectionIds = _sectionIds.AsReadOnly();
            TermsText = _termsText.AsReadOnly();
        }
    }
}
