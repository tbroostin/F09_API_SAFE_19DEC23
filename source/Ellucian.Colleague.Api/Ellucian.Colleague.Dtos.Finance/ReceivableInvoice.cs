// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A receivable invoice
    /// </summary>
    public class ReceivableInvoice : ReceivableTransaction
    {
        /// <summary>
        /// Invoice due date
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Billing start date
        /// </summary>
        public DateTime? BillingStart { get; set; }

        /// <summary>
        /// Billing end date
        /// </summary>
        public DateTime? BillingEnd { get; set; }

        /// <summary>
        /// Invoice description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of charges on this invoice
        /// </summary>
        public List<ReceivableCharge> Charges { get; set; }

        /// <summary>
        /// ID of invoice adjusted by this invoice
        /// </summary>
        public string AdjustmentToInvoice { get; set; }

        /// <summary>
        /// List of IDs that adjust this invoice
        /// </summary>
        public List<string> AdjustedByInvoices { get; set; }

        /// <summary>
        /// Base amount of invoice w/o taxes
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Amount of taxes on invoice
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount of invoice
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The type of the invoice
        /// </summary>
        public string InvoiceType { get; set; }
    }
}
