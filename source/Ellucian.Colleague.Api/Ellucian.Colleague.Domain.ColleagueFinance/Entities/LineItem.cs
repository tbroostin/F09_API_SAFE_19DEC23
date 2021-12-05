// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a line item for an Accounts Payable or Purchasing document.
    /// </summary>
    [Serializable]
    public class LineItem
    {
        /// <summary>
        /// Private system-generated line item id.
        /// </summary>
        private readonly string id;

        /// <summary>
        /// Public getter for the private id.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private line item description.
        /// </summary>
        private readonly string description;

        /// <summary>
        /// Public getter for the line item description.
        /// </summary>
        public string Description { get { return description; } }

        /// <summary>
        /// Private variable for the line item quantity.
        /// </summary>
        private readonly decimal quantity;

        /// <summary>
        /// Public getter for the private line item quantity.
        /// </summary>
        public decimal Quantity { get { return quantity; } }

        /// <summary>
        /// Private variable for the line item price.
        /// </summary>
        private readonly decimal price;

        /// <summary>
        /// Public getter for the private line item price.
        /// </summary>
        public decimal Price { get { return price; } }

        /// <summary>
        /// Private variable for the line item extended price.
        /// </summary>
        private readonly decimal extendedPrice;

        /// <summary>
        /// Public getter for the private line item extended price.
        /// </summary>
        public decimal ExtendedPrice { get { return extendedPrice; } }

        /// <summary>
        /// Line item unit of issue.
        /// </summary>
        public string UnitOfIssue { get; set; }

        /// <summary>
        /// Line item vendor part.
        /// </summary>
        public string VendorPart { get; set; }

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
        /// Line Item commodity code
        /// </summary>
        public string CommodityCode { get; set; }

        /// <summary>
        /// List of Line Item Tax codes
        /// </summary>
        public List<LineItemReqTax> LineItemReqTaxCodes = new List<LineItemReqTax>();

        /// <summary>
        /// Line Item Tax code
        /// </summary>
        public List<LineItemReqTax> ReqLineItemTaxCodes { get; set; }

        /// <summary>
        ///Line Item Trade Discount Amount
        /// </summary>
        public Decimal? TradeDiscountAmount { get; set; }

        /// <summary>
        /// Line Item Trade Discount Percentage
        /// </summary>
        public Decimal? TradeDiscountPercentage { get; set; }
        
        /// <summary>
        /// Fixed asset flag
        /// </summary>
        public string FixedAssetsFlag { get; set; }

        /// <summary>
        /// Status Date
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
		/// LineItemStatus
		/// </summary>
		public LineItemStatus? LineItemStatus { get; set; }

        /// <summary>
        /// Requisition ID for supporting documents.
        /// </summary>
        public string RequisitionId { get; set; }
        

        /// <summary>
        /// This is the private list of GL distributions associated with the line item.
        /// </summary>
        private readonly List<LineItemGlDistribution> glDistributions = new List<LineItemGlDistribution>();

        /// <summary>
        /// This is the public getter for the private list of GL distributions.
        /// </summary>
        public ReadOnlyCollection<LineItemGlDistribution> GlDistributions { get; private set; }

        /// <summary>
        /// This is the private list of tax information associated with the line item.
        /// </summary>
        private readonly List<LineItemTax> lineItemTaxes = new List<LineItemTax>();

        /// <summary>
        /// This is the private list of tax code information associated with the line item.
        /// </summary>
        private readonly List<LineItemReqTax> lineItemReqTaxes = new List<LineItemReqTax>();

        /// <summary>
        /// This is the public getter for the private list of tax information.
        /// </summary>
        public ReadOnlyCollection<LineItemTax> LineItemTaxes { get; private set; }

        /// <summary>
        /// Indicates whether the line item has an overbudget general ledger number.
        /// </summary>
        public bool OverBudget { get; set; }

        /// <summary>
        /// This constructor initializes the line item object.
        /// </summary>
        /// <param name="id">This is the line item ID.</param>
        /// <param name="description">This is the line item description.</param>
        /// <param name="quantity">This is the line item quantity.</param>
        /// <param name="price">This is the line item price.</param>
        /// <param name="extendedPrice">This is the line item extended price.</param>
        /// /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public LineItem(string id, string description, decimal quantity, decimal price, decimal extendedPrice)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is a required field.");
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "Description is a required field.");
            }

            this.id = id;
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.extendedPrice = extendedPrice;
            this.ReqLineItemTaxCodes = lineItemReqTaxes;
            GlDistributions = glDistributions.AsReadOnly();
            LineItemTaxes = lineItemTaxes.AsReadOnly();
        }

        /// <summary>
        /// This method adds a line item GL distribution to the list
        /// of GL distributions that belong to the line item.
        /// </summary>
        /// <param name="LineItemGlDistribution">This is the line item GL distribution.</param>
        public void AddGlDistribution(LineItemGlDistribution lineItemGlDistribution)
        {
            if (lineItemGlDistribution == null)
            {
                throw new ArgumentNullException("lineItemGlDistribution", "GL distribution cannot be null");
            }

            bool isInList = false;
            if (glDistributions != null)
            {
                foreach (var glDistr in glDistributions)
                {
                    if ((glDistr.GlAccountNumber == lineItemGlDistribution.GlAccountNumber) & (glDistr.ProjectLineItemId == lineItemGlDistribution.ProjectLineItemId))
                    {
                        isInList = true;
                    }
                }
            }

            if (!isInList)
            {
                glDistributions.Add(lineItemGlDistribution);
            }
        }

        /// <summary>
        /// This method adds a line item GL distribution to the list
        /// of GL distributions that belong to the line item during save from SS.
        /// </summary>
        /// <param name="LineItemGlDistribution">This is the line item GL distribution.</param>
        public void AddGlDistributionForSave(LineItemGlDistribution lineItemGlDistribution)
        {
            if (lineItemGlDistribution == null)
            {
                throw new ArgumentNullException("lineItemGlDistribution", "GL distribution cannot be null");
            }

            bool isInList = false;
            if (glDistributions != null)
            {
                foreach (var glDistr in glDistributions)
                {
                    if ((glDistr.GlAccountNumber == lineItemGlDistribution.GlAccountNumber) && (glDistr.ProjectNumber == lineItemGlDistribution.ProjectNumber))
                    {
                        isInList = true;
                    }
                }
            }
            if (!isInList)
            {
                glDistributions.Add(lineItemGlDistribution);
            }
        }


        /// <summary>
        /// This method adds tax information to the list of
        /// taxes for a line item.
        /// </summary>
        /// <param name="LineItemTaxGlDistribution">This is the line item tax information.</param>
        public void AddTax(LineItemTax lineItemTax)
        {
            if (lineItemTax == null)
            {
                throw new ArgumentNullException("tax", "Line item tax cannot be null");
            }

            bool isInList = false;
            if (lineItemTaxes != null)
            {
                foreach (var tax in lineItemTaxes)
                {
                    if (tax.TaxCode == lineItemTax.TaxCode)
                    {
                        tax.TaxAmount += lineItemTax.TaxAmount;
                        isInList = true;
                    }
                }
            }

            if (!isInList)
            {
                lineItemTaxes.Add(lineItemTax);
            }
        }

        /// <summary>
        /// This method adds tax code information to the list of
        /// taxes for a line item.
        /// </summary>
        /// <param name="LineItemReqTax">This is the line item tax code information.</param>
        public void AddReqTax(LineItemReqTax lineItemTax)
        {
            if (lineItemTax == null)
            {
                throw new ArgumentNullException("tax", "Line item tax cannot be null");
            }

            bool isInList = false;
            if (lineItemReqTaxes != null)
            {
                foreach (var tax in lineItemReqTaxes)
                {
                    if (tax.TaxReqTaxCode == lineItemTax.TaxReqTaxCode)
                    {
                       
                        isInList = true;
                    }
                }
            }

            if (!isInList)
            {
                lineItemReqTaxes.Add(lineItemTax);
            }
        }


        /// <summary>
        /// This method adds tax information to the list by TaxCode and by GL Number
        /// for each line item.
        /// </summary>
        /// <param name="LineItemTaxGlDistribution">This is the line item tax information.</param>
        public void AddTaxByGL(LineItemTax lineItemTax)
        {
            if (lineItemTax == null)
            {
                throw new ArgumentNullException("tax", "Line item tax cannot be null");
            }

            bool isInList = false;
            if (lineItemTaxes != null)
            {
                foreach (var tax in lineItemTaxes)
                {
                    if (tax.TaxCode == lineItemTax.TaxCode && tax.LineGlNumber == lineItemTax.LineGlNumber)
                    {
                        tax.TaxAmount += lineItemTax.TaxAmount;
                        isInList = true;
                    }
                }
            }

            if (!isInList)
            {
                lineItemTaxes.Add(lineItemTax);
            }
        }
    }
}
