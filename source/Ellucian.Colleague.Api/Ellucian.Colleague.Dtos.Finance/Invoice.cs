// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An AR Invoice
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Invoice ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the person for whom the invoice was created
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Code of the type of receivable account for which the invoice was created
        /// </summary>
        public string ReceivableTypeCode { get; set; }

        /// <summary>
        /// ID of the Term in which the invoice resides
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Invoice reference number
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Billing Period Start Date for the invoice
        /// </summary>
        public DateTime BillingStart { get; set; }

        /// <summary>
        /// Billing Period End Date for the invoice
        /// </summary>
        public DateTime BillingEnd { get; set; }

        /// <summary>
        /// Description of the invoice
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date on which payment for this invoice is due
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// The due date offset by CTZS
        /// </summary>
        public DateTimeOffset? DueDateOffset { get; set; }

        /// <summary>
        /// List of Charges associated with the invoice
        /// </summary>
        public List<Charge> Charges { get; set; }
        
        /// <summary>
        /// Sum of all of the charges on the invoice, prior to any tax calculations
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// The amount of taxes assessed to the invoice
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount of the invoice
        /// </summary>
        public decimal Amount { get; set; }
    }
}
