// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class RegistrationTermsApproval
    {
        private string _id;
        private readonly string _studentId;
        private readonly DateTimeOffset _acknowledgementTimestamp;
        private readonly string _paymentControlId;
        private readonly List<string> _sectionIds = new List<String>();
        private readonly List<string> _invoiceIds = new List<String>();
        private readonly string _termsResponseId;

        /// <summary>
        /// ID of the registration approval
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("ID Registration approval ID cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Student ID number
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Date and time the approval was made
        /// </summary>
        public DateTimeOffset AcknowledgementTimestamp { get { return _acknowledgementTimestamp; } }

        /// <summary>
        /// Registration payment control ID
        /// </summary>
        public string PaymentControlId { get { return _paymentControlId; } }

        /// <summary>
        /// List of section IDs linked to the approval
        /// </summary>
        public List<string> SectionIds { get { return _sectionIds; } }

        /// <summary>
        /// List of invoice IDs linked to the approval
        /// </summary>
        public List<string> InvoiceIds { get { return _invoiceIds; } }

        /// <summary>
        /// Terms and conditions response
        /// </summary>
        public string TermsResponseId { get { return _termsResponseId; } }
        
        /// <summary>
        /// Acknowledgement document
        /// </summary>
        public string AcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <param name="timestamp"></param>
        /// <param name="paymentControlId"></param>
        /// <param name="sectionIds"></param>
        /// <param name="invoiceIds"></param>
        /// <param name="termsResponseId"></param>
        public RegistrationTermsApproval(string id, string studentId, DateTimeOffset timestamp, string paymentControlId, IEnumerable<string> sectionIds, 
            IEnumerable<string> invoiceIds, string termsResponseId)
        {
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID must be specified.");
            }
            if (string.IsNullOrEmpty(paymentControlId))
            {
                throw new ArgumentNullException("paymentControlId", "Payment control ID must be specified.");
            }
            if (sectionIds == null || sectionIds.Count() == 0)
            {
                throw new ArgumentNullException("sectionIds", "Registration approval must have at least one associated course section.");
            }
            if (invoiceIds == null || invoiceIds.Count() == 0)
            {
                throw new ArgumentNullException("invoiceIds", "Registration approval must have at least one associated invoice.");
            }
            if (termsResponseId == null)
            {
                throw new ArgumentNullException("termsResponseId", "Terms and conditions response ID cannot be null.");
            }

            _id = id;
            _studentId = studentId;
            _acknowledgementTimestamp = timestamp;
            _paymentControlId = paymentControlId;
            _sectionIds.AddRange(sectionIds);
            _invoiceIds.AddRange(invoiceIds);
            _termsResponseId = termsResponseId;
        }
    }
}
