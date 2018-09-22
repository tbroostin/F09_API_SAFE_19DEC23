// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class Invoice
    {
        // Private fields
        private readonly string _id;
        private readonly string _personId;
        private readonly string _receivableTypeCode;
        private readonly string _term;      // Term is not required but cannot be changed
        private readonly string _referenceNumber;
        private readonly DateTime _date;
        private readonly DateTime _dueDate;
        private readonly DateTime _billingStart;
        private readonly DateTime _billingEnd;
        private readonly string _description;
        private readonly List<Charge> _charges = new List<Charge>();
        private List<string> _adjustedByInvoices = new List<string>();

        /// <summary>
        /// Invoice ID
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Person ID of accountholder
        /// </summary>
        public string PersonId { get { return _personId; } }

        /// <summary>
        /// Code for receivable type on account
        /// </summary>
        public string ReceivableTypeCode { get { return _receivableTypeCode; } }

        /// <summary>
        /// Term ID
        /// </summary>
        public string TermId { get { return _term; } }

        /// <summary>
        /// Invoice reference number
        /// </summary>
        public string ReferenceNumber { get { return _referenceNumber; } }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime Date { get { return _date; } }

        /// <summary>
        /// Invoice due date
        /// </summary>
        public DateTime DueDate { get { return _dueDate; } }

        /// <summary>
        /// Billing start date
        /// </summary>
        public DateTime BillingStart { get { return _billingStart; } }

        /// <summary>
        /// Billing end date
        /// </summary>
        public DateTime BillingEnd { get { return _billingEnd; } }

        /// <summary>
        /// Invoice description
        /// </summary>
        public string Description { get { return _description; } }

        /// <summary>
        /// List of charges on this invoice
        /// </summary>
        public ReadOnlyCollection<Charge> Charges { get; private set; }

        /// <summary>
        /// Archived indicator
        /// </summary>
        public bool Archived { get; set; }

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
        /// Constructor for invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="personId">Accountholder ID</param>
        /// <param name="receivableType">Receivable type code</param>
        /// <param name="term">Term ID (optional)</param>
        /// <param name="referenceNumber">Invoice number</param>
        /// <param name="date">Invoice date</param>
        /// <param name="dueDate">Invoice due date</param>
        /// <param name="billingStart">Billing start date</param>
        /// <param name="billingEnd">Billing end date</param>
        /// <param name="description">Description</param>
        /// <param name="charges">List of charges</param>
        public Invoice(string id, string personId, string receivableType, string term, string referenceNumber, DateTime date, DateTime dueDate, DateTime billingStart, DateTime billingEnd,
            string description, IEnumerable<Charge> charges)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id","Invoice ID cannot be null");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId","Person ID cannot be null");
            }
            if (string.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable type cannot be null");
            }
            if (String.IsNullOrEmpty(referenceNumber))
            {
                throw new ArgumentNullException("referenceNumber", "Reference number must be specified");
            }
            if (billingStart > billingEnd)
            {
                throw new ArgumentException("Billing start date cannot be after billing end date.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description","Description cannot be null");
            }
            if (charges == null || charges.Count() == 0)
            {
                throw new ArgumentNullException("charges", "An invoice must have at least one charge.");
            }

            _id = id;
            _personId = personId;
            _receivableTypeCode = receivableType;
            _term = term;
            _referenceNumber = referenceNumber;
            _date = date;
            _dueDate = dueDate;
            _billingStart = billingStart;
            _billingEnd = billingEnd;
            _description = description;
            _charges.AddRange(charges);
            Charges = _charges.AsReadOnly();
            AdjustedByInvoices = _adjustedByInvoices.AsReadOnly();
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
