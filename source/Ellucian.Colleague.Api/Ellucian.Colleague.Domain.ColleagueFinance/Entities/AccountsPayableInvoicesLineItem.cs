// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a line item for an Accounts Payable Invoices
    /// </summary>
    [Serializable]
    public class AccountsPayableInvoicesLineItem : LineItem
    {

        /// <summary>
        /// Line Item Commodity Code
        /// </summary>
        public string CommodityCode { get; set; }

        /// <summary>
        /// Store the Document Line item ID to pass to CTX.
        /// </summary>
        public string DocLineItemId { get; set; }


        /// <summary>
        /// Line Item Unit of Measure
        /// </summary>
        public string UnitOfMeasure { get; set; }

      
        /// <summary>
        /// Line Item Taxes
        /// </summary>
        public List<LineItemTax> AccountsPayableLineItemTaxes { get; set; }

        /// <summary>
        /// Used in determining the extended price   and the trade discount percentage
        /// </summary>
        public Decimal? TradeDiscountAmount { get; set; }

        /// <summary>
        /// Determine the extended price and   the trade discount amount
        /// </summary>
        public Decimal? TradeDiscountPercent { get; set; }

        /// <summary>
        /// Used in determining extended price for   discount lost accounting method
        /// </summary>
        public Decimal? CashDiscountAmount { get; set; }
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// "An indicator specifying if the encumbrance for the line item should be liquidated in full (final payment).
        /// </summary>
        public bool FinalPaymentFlag { get; set; }


        public AccountsPayableInvoicesLineItem(string id, string description, decimal quantity, decimal price, decimal extendedPrice) 
            : base(id, description, quantity, price, extendedPrice)
        {
            AccountsPayableLineItemTaxes = new List<LineItemTax>();
        }
    }
}