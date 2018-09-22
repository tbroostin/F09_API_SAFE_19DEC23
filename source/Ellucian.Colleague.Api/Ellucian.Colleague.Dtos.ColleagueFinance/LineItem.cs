// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The line item for an Accounts Payable or Purchasing document
    /// exposed from the domain entity.
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// Line item description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Line item quantity.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Line item price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Line item unit of issue.
        /// </summary>
        public string UnitOfIssue { get; set; }

        /// <summary>
        /// Line item vendor part.
        /// </summary>
        public string VendorPart { get; set; }

        /// <summary>
        /// Line item extended price.
        /// </summary>
        public decimal ExtendedPrice { get; set; }

        /// <summary>
        /// Line item expected delivery date.
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Line item desired date.
        /// </summary>
        public DateTime? DesiredDate { get; set; }

        /// <summary>
        /// Line item invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Line item tax form.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Line item tax form code.
        /// </summary>
        public string TaxFormCode { get; set; }

        /// <summary>
        /// Line item tax form location.
        /// </summary>
        public string TaxFormLocation { get; set; }

        /// <summary>
        /// Line Item comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// List of line item GL distributions.
        /// </summary>
        public List<LineItemGlDistribution> GlDistributions { get; set; }

        /// <summary>
        /// List of line item tax information.
        /// </summary>
        public List<LineItemTax> LineItemTaxes { get; set; }
    }
}
