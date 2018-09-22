// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class RegistrationPaymentControl
    {
        // Private fields
        private readonly string _id;
        private readonly string _studentId;
        private readonly string _termId;
        private RegistrationPaymentStatus _paymentStatus;

        // Private lists
        private List<string> _registeredSectionIds = new List<string>();
        private List<string> _invoiceIds = new List<string>();
        private List<string> _academicCredits = new List<string>();
        private List<string> _payments = new List<string>();

        /// <summary>
        /// ID of registration payment control
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Student ID
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Term ID
        /// </summary>
        public string TermId { get { return _termId; } }

        /// <summary>
        /// Payment status for registration
        /// </summary>
        public RegistrationPaymentStatus PaymentStatus 
        { 
            get { return _paymentStatus; }
            set { _paymentStatus = value; }
        }

        /// <summary>
        /// List of section IDs for which the student is actively registered
        /// </summary>
        public ReadOnlyCollection<string> RegisteredSectionIds { get; private set; }

        /// <summary>
        /// List of registration invoice IDs
        /// </summary>
        public ReadOnlyCollection<string> InvoiceIds { get; private set; }

        /// <summary>
        /// List of the academic credits corresponding to the registered sections
        /// </summary>
        public ReadOnlyCollection<string> AcademicCredits { get; private set; }

        /// <summary>
        /// List of payments made toward this registration through Pay for Registration
        /// </summary>
        public ReadOnlyCollection<string> Payments { get; private set; }

        /// <summary>
        /// ID of the last registration terms approval record
        /// </summary>
        public string LastTermsApprovalId { get; set; }

        /// <summary>
        /// ID of the last payment plan approval record
        /// </summary>
        public string LastPlanApprovalId { get; set; }

        /// <summary>
        /// ID of the associated payment plan
        /// </summary>
        public string PaymentPlanId { get; set; }

        /// <summary>
        /// Constructor for registration payment control
        /// </summary>
        /// <param name="id">Record ID</param>
        /// <param name="studentId">Student ID</param>
        /// <param name="termId">Term ID</param>
        /// <param name="paymentStatus">Registration payment status</param>
        public RegistrationPaymentControl(string id, string studentId, string termId, RegistrationPaymentStatus paymentStatus)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            // For now, term is required. This may change later to support non-term registration.
            if (String.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }

            _id = id;
            _studentId = studentId;
            _termId = termId;
            _paymentStatus = paymentStatus;

            RegisteredSectionIds = _registeredSectionIds.AsReadOnly();
            InvoiceIds = _invoiceIds.AsReadOnly();
            AcademicCredits = _academicCredits.AsReadOnly();
            Payments = _payments.AsReadOnly();
        }

        /// <summary>
        /// Add a registered section
        /// </summary>
        /// <param name="section">Section ID</param>
        public void AddRegisteredSection(string section)
        {
            if (String.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }
            // Don't add a duplicate value
            if (!_registeredSectionIds.Contains(section))
            {
                _registeredSectionIds.Add(section);
            }
        }

        /// <summary>
        /// Add an invoice
        /// </summary>
        /// <param name="invoice">Invoice ID</param>
        public void AddInvoice(string invoice)
        {
            if (String.IsNullOrEmpty(invoice))
            {
                throw new ArgumentNullException("invoice");
            }
            // Don't add a duplicate value
            if (!_invoiceIds.Contains(invoice))
            {
                _invoiceIds.Add(invoice);
            }
        }

        /// <summary>
        /// Add an academic credit
        /// </summary>
        /// <param name="acadCred">Academic credit ID</param>
        public void AddAcademicCredit(string acadCred)
        {
            if (String.IsNullOrEmpty(acadCred))
            {
                throw new ArgumentNullException("acadCred");
            }
            // Don't add a duplicate value
            if (!_academicCredits.Contains(acadCred))
            {
                _academicCredits.Add(acadCred);
            }
        }

        /// <summary>
        /// Add a registration payment
        /// </summary>
        /// <param name="payment">Payment ID</param>
        public void AddPayment(string payment)
        {
            if (String.IsNullOrEmpty(payment))
            {
                throw new ArgumentNullException("payment");
            }
            // Don't add a duplicate value
            if (!_payments.Contains(payment))
            {
                _payments.Add(payment);
            }
        }
    }
}
