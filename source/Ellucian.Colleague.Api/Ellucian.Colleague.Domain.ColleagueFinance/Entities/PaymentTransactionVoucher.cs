// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is an AccountsPayableInvoices.
    /// </summary>
    [Serializable]
    public class PaymentTransactionVoucher 
    {
        /// <summary>
        /// Private system-generated ID.
        /// </summary>
        private readonly string id;

        /// <summary>
        /// This is the public getter for the private ID.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// GUID for the voucher; required, but cannot be changed once assigned.
        /// </summary>
        private string guid;

        public new string Guid
        {
            get { return guid; }
            set
            {
                if (string.IsNullOrEmpty(guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }       

        /// <summary>
        /// This amount is the total of all line items for the voucher without the 
        /// cash discount
        /// </summary>
        public AmountAndCurrency VoucherInvoiceAmt { get; set; }

       

        /// <summary>
        /// Private variable for the voucher purchase order ID.
        /// </summary>
        private string purchaseOrderId;
      
        /// <summary>
        /// This is the purchase order ID associated with the voucher.
        /// </summary>
        public string PurchaseOrderId
        {
            get
            {
                return this.purchaseOrderId;
            }

            set
            {
                // A voucher can originate from one document, either a PO, BPO, or a recurring voucher.
                if (!string.IsNullOrEmpty(blanketPurchaseOrderId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id,"'"));
                }
                else if (!string.IsNullOrEmpty(recurringVoucherId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id, "'"));
                }
                else
                {
                    this.purchaseOrderId = value;
                }
            }
        }



        /// <summary>
        /// Private variable for the voucher blanket purchase order ID.
        /// </summary>
        private string blanketPurchaseOrderId;

        /// <summary>
        /// This is the blanket purchase order ID associated with the voucher.
        /// </summary>
        public string BlanketPurchaseOrderId
        {
            get
            {
                return this.blanketPurchaseOrderId;
            }

            set
            {
                // A voucher can originate from one document, either a PO, BPO, or a recurring voucher.
                if (!string.IsNullOrEmpty(purchaseOrderId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id, "'"));
                }
                else if (!string.IsNullOrEmpty(recurringVoucherId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id,"'"));
                }
                else
                {
                    this.blanketPurchaseOrderId = value;
                }
            }
        }

        /// <summary>
        /// Private variable for the voucher recurring voucher ID.
        /// </summary>
        private string recurringVoucherId;

        /// <summary>
        /// This is the recurring voucher ID associated with the voucher.
        /// </summary>
        public string RecurringVoucherId
        {
            get
            {
                return this.recurringVoucherId;
            }

            set
            {
                // A voucher can originate from one document, either a PO, BPO, or a recurring voucher.
                if (!string.IsNullOrEmpty(purchaseOrderId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id, "'"));
                }
                else if (!string.IsNullOrEmpty(blanketPurchaseOrderId))
                {
                    throw new ApplicationException(string.Concat("A voucher can only have either a PO, BPO, or recurring voucher. Entity: 'VOUCHERS', Record ID: '", this.Id, "'"));
                }
                else
                {
                    this.recurringVoucherId = value;
                }
            }
        }
        /// <summary>
        /// This is the private list of line items associated with the document.
        /// </summary>
        private readonly List<AccountsPayableInvoicesLineItem> lineItems = new List<AccountsPayableInvoicesLineItem>();

        /// <summary>
        /// This is the public getter for the private list of line items associated with the document.
        /// </summary>
        public new ReadOnlyCollection<AccountsPayableInvoicesLineItem> LineItems { get; private set; }

        

        /// <summary>
        /// This constructor initializes the voucher domain entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="voucherId">This is the voucher ID.</param>
        /// <param name="date">This is the voucher date.</param>
        /// <param name="status">This is the voucher status.</param>
        /// <param name="vendorName">This is the voucher vendor name.</param>
        /// <param name="invoiceNumber">This is the voucher invoice number.</param>
        /// <param name="invoiceDate">This is the voucher invoice date.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public PaymentTransactionVoucher(string guid, string voucherId)          
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Concat( "Guid is a required field. Voucher id: ", voucherId));
            }
           

            this.guid = guid;
            this.id = voucherId;            
            LineItems = this.lineItems.AsReadOnly();
        }

        /// <summary>
        /// This method adds a line item to the document.
        /// </summary>
        /// <param name="lineItem">Line item object.</param>
        public void AddAccountsPayableInvoicesLineItem(AccountsPayableInvoicesLineItem lineItem)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException("lineItem", string.Concat("Line item cannot be null. Entity: 'VOUCHERS', Record ID: '", this.Id, "'"));
            }

            if (this.lineItems != null && this.lineItems.Count(x => x.Id == lineItem.Id && x.Id != System.Guid.Empty.ToString()) > 1)
            {
                throw new ArgumentNullException("lineItem", string.Concat("Duplicate Line item found. Entity:'VOUCHERS', Record ID: '", this.Id, "'"));
            }
            this.lineItems.Add(lineItem);
        }
    }
}