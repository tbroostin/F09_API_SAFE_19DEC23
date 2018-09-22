// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An invoice against a receivable account
    /// </summary>
    [Serializable]
    public class ReceivableInvoice : ReceivableTransaction
    {
        // Private fields
        private readonly DateTime _dueDate;
        private readonly DateTime? _billingStart;
        private readonly DateTime? _billingEnd;
        private readonly string _description;
        private readonly List<ReceivableCharge> _charges = new List<ReceivableCharge>();
        private readonly List<string> _adjustedByInvoices = new List<string>();
        private string _invoiceType;

        /// <summary>
        /// Invoice due date
        /// </summary>
        public DateTime DueDate { get { return _dueDate; } }

        /// <summary>
        /// Billing start date
        /// </summary>
        public DateTime? BillingStart { get { return _billingStart; } }

        /// <summary>
        /// Billing end date
        /// </summary>
        public DateTime? BillingEnd { get { return _billingEnd; } }

        /// <summary>
        /// Invoice description
        /// </summary>
        public string Description { get { return _description; } }

        /// <summary>
        /// List of charges on this invoice
        /// </summary>
        public ReadOnlyCollection<ReceivableCharge> Charges { get; private set; }

        /// <summary>
        /// ID of invoice adjusted by this invoice
        /// </summary>
        public string AdjustmentToInvoice { get; set; }

        /// <summary>
        /// List of IDs that adjust this invoice
        /// </summary>
        public ReadOnlyCollection<string> AdjustedByInvoices { get; private set; }

        /// <summary>
        /// Base amount of invoice w/o taxes
        /// </summary>
        public decimal BaseAmount { get { return Charges.Sum(x => x.BaseAmount); } }

        /// <summary>
        /// Amount of taxes on invoice
        /// </summary>
        public decimal TaxAmount { get { return Charges.Sum(x => x.TaxAmount); } }

        /// <summary>
        /// Total amount of invoice
        /// </summary>
        public decimal Amount { get { return BaseAmount + TaxAmount; } }

        /// <summary>
        /// Invoice Type
        /// </summary>
        public string InvoiceType
        {
            get
            {
                return _invoiceType;
            }
            set
            {
                if (!string.IsNullOrEmpty(_invoiceType))
                {
                    throw new InvalidOperationException("InvoiceType already defined for receivable invoice.");
                }
                _invoiceType = value;
            }
        }

        /// <summary>
        /// Constructor for invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="referenceNumber">Invoice number</param>
        /// <param name="personId">Accountholder ID</param>
        /// <param name="receivableType">Receivable type code</param>
        /// <param name="termId">Term ID (optional)</param>
        /// <param name="date">Invoice date</param>
        /// <param name="dueDate">Invoice due date</param>
        /// <param name="billingStart">Billing start date</param>
        /// <param name="billingEnd">Billing end date</param>
        /// <param name="description">Description</param>
        /// <param name="charges">List of charges</param>
        public ReceivableInvoice(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, 
            DateTime dueDate, DateTime? billingStart, DateTime? billingEnd, string description, IEnumerable<ReceivableCharge> charges)
            : base(id, referenceNumber, personId, receivableType, termId, date)
        {
            if (dueDate == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("dueDate");
            }
            if ((billingStart == null) != (billingEnd == null))
            {
                throw new ArgumentException("billingStart and billingEnd must either both be null or both have a value.");
            }
            if (billingEnd != null)
            {
                if (billingStart == default(DateTime))
                {
                    throw new ArgumentOutOfRangeException("billingStart");
                }
                if (billingEnd == default(DateTime))
                {
                    throw new ArgumentOutOfRangeException("billingEnd");
                }
                if (billingStart > billingEnd)
                {
                    throw new ArgumentOutOfRangeException("billingStart", "Billing start date cannot be after billing end date.");
                }
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "Description cannot be null.");
            }
            if (charges == null || !charges.Any())
            {
                throw new ArgumentNullException("charges", "An invoice must have at least one charge.");
            }

            _dueDate = dueDate;
            _billingStart = billingStart;
            _billingEnd = billingEnd;
            _description = description;
            foreach (var charge in charges)
            {
                AddInvoiceCharge(charge);
            }

            Charges = _charges.AsReadOnly();
            AdjustedByInvoices = _adjustedByInvoices.AsReadOnly();
        }

        /// <summary>
        /// Add a charge to an invoice
        /// </summary>
        /// <param name="charge">the charge to add</param>
        private void AddInvoiceCharge(ReceivableCharge charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge", "A charge must be specified.");
            }
            if (!string.IsNullOrEmpty(charge.Id) && !string.IsNullOrEmpty(Id) &&
                !string.IsNullOrEmpty(charge.InvoiceId) && charge.InvoiceId != Id && !_charges.Contains(charge))
            {
                throw new InvalidOperationException("InvoiceId already defined for receivable charge.");
            }
            if (!_charges.Contains(charge))
            {
                _charges.Add(charge);
            }
        }

        /// <summary>
        /// Add an adjusting invoice to this invoice
        /// </summary>
        /// <param name="invoiceId">ID of adjusting invoice</param>
        public void AddAdjustingInvoice(string invoiceId)
        {
            if (String.IsNullOrEmpty(invoiceId))
            {
                throw new ArgumentNullException("invoiceId", "An adjusting invoice ID must be specified.");
            }

            // Prevent duplicates
            if (!AdjustedByInvoices.Contains(invoiceId))
            {
                _adjustedByInvoices.Add(invoiceId);
            }
        }
    }
}
