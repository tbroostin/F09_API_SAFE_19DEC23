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
    public class AccountsPayableInvoices : AccountsPayablePurchasingDocument
    {
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
        /// Private variable for the voucher status.
        /// </summary>
        private readonly VoucherStatus status;

        /// <summary>
        /// This is the public getter for the private voucher status.
        /// </summary>
        public VoucherStatus Status { get { return status; } }


        /// <summary>
        /// This is the voucher due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Private variable for the voucher invoice number.
        /// </summary>
        private readonly string invoiceNumber;

        /// <summary>
        /// Public getter for the private voucher invoice number.
        /// </summary>
        public string InvoiceNumber { get { return invoiceNumber; } }

        /// <summary>
        /// Private variable for the voucher invoice date.
        /// </summary>
        private readonly DateTime? invoiceDate;

        /// <summary>
        /// Public getter for the private voucher invoice date.
        /// </summary>
        public DateTime? InvoiceDate { get { return invoiceDate; } }

        /// <summary>
        /// This is the voucher check number. It contains just the check number,
        /// and not the bank code.
        /// </summary>
        public string CheckNumber { get; set; }

        /// <summary>
        /// This is the voucher check date.
        /// </summary>
        public DateTime? CheckDate { get; set; }
       
        /// <summary>
        /// The effective date of the associated voucher status.   
        /// </summary>
        public DateTime? VoucherStatusDate { get; set; }

        /// <summary>
        /// Used by the voucher payment program to decide whether or not to allow a 
        /// voucher otherwise eligible for payment to be paid.
        /// </summary>
        public string VoucherPayFlag { get; set; }
                
        /// <summary>
        /// Line Item Tazes
        /// </summary>
        public List<LineItemTax> VoucherTaxes { get; set; }

        /// <summary>
        /// This amount is the total of all line items for the voucher without the 
        /// cash discount
        /// </summary>
        public Decimal? VoucherInvoiceAmt { get; set; }

        /// <summary>
        /// Used to determine what to book to GL discount accounts based on discount method 
        /// </summary>
        public Decimal? VoucherDiscAmt { get; set; }

        /// <summary>
        /// This is used to calculate cash discounts
        /// </summary>
        public string VoucherVendorTerms { get; set; }

        /// <summary>
        ///  Address information for a vendor
        /// </summary>
        public string VendorAddressId { get; set; }

        /// <summary>
        /// The net posted to the ledger
        /// </summary>
        public Decimal? VoucherNet { get; set; }

        /// <summary>
        /// The reference number of a document that is associated with the voucher
        /// </summary>
        public List<string> VoucherReferenceNo { get; set; }

        /// <summary>
        ///  Core address ID associated with the 'AP.CHECK' 
        ///  address information for a Core vendor
        /// </summary>
        public string VoucherAddressId { get; set; }

        /// <summary>
        /// Miscellaneous Vendor Name for vouchers with
        /// no vendor record in Colleague.
        /// </summary>
        public List<string> VoucherMiscName { get; set; }

        /// <summary>
        /// Miscellaneous Vendor address should be used if set.
        /// </summary>
        public bool VoucherUseAltAddress { get; set; }

        /// <summary>
        /// Miscellaneous Vendor type "Person" or "Corporation"
        /// for a voucher with no vendor record in Colleague.
        /// </summary>
        public string VoucherMiscType { get; set; }

        /// <summary>
        /// Miscellaneous Vendor Address lines for a voucher
        /// with no vendor record in Colleague.
        /// </summary>
        public List<string> VoucherMiscAddress { get; set; }

        /// <summary>
        /// Miscellaneous Vendor City for a voucher with no
        /// vendor record in Colleague.
        /// </summary>
        public string VoucherMiscCity { get; set; }

        /// <summary>
        /// Miscellaneous Vendor state for a voucher with no
        /// vendor record in Colleague.
        /// </summary>
        public string VoucherMiscState { get; set; }

        /// <summary>
        /// Miscellaneous Vendor postal code for a voucher with
        /// no vendor record in Colleague.
        /// </summary>
        public string VoucherMiscZip { get; set; }

        /// <summary>
        /// Miscellaneous Vendor country for a voucher with no
        /// vendor record in Colleague.
        /// </summary>
        public string VoucherMiscCountry { get; set; }

        /// <summary>
        /// used as the date on the GL transactions for backing 
        /// out expenses and reinstating encumbrances.
        /// </summary>
        public DateTime? VoucherVoidGlTranDate { get; set; }

        /// <summary>
        /// Host Country
        /// </summary>
        public string HostCountry { get; set; }

        /// <summary>
        ///  The PERSON ID of the person creating a voucher     
        /// </summary>
        public string VoucherRequestor { get; set; }


        /// <summary>
        /// The submitted by operator for funds availability checking.
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// Causes the API to assign tax forms, box codes, and locations to line items based solely on the VENDORS record in Colleague
        /// </summary>
        public bool ByPassTaxForms { get; set; }

        /// <summary>
        /// Allows this partner system to create purchase orders directly in an "outstanding" status even if Colleague approvals are turned on.
        /// </summary>
        public bool ByPassVoucherApproval { get; set; }

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
        /// <param name="voucherId">This is the voucher ID.</param>
        /// <param name="date">This is the voucher date.</param>
        /// <param name="status">This is the voucher status.</param>
        /// <param name="vendorName">This is the voucher vendor name.</param>
        /// <param name="invoiceNumber">This is the voucher invoice number.</param>
        /// <param name="invoiceDate">This is the voucher invoice date.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public AccountsPayableInvoices(string voucherId, DateTime date, VoucherStatus status, string vendorName, string invoiceNumber, DateTime? invoiceDate)
            : base(voucherId, vendorName, date)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
            {
                throw new ArgumentNullException("vendorInvoiceNumber", string.Concat("Vendor Invoice Number is a required field. Entity: 'VOUCHERS', Record ID: '", voucherId, "'"));
            }

            this.status = status;
            this.invoiceNumber = invoiceNumber;
            this.invoiceDate = invoiceDate;
            LineItems = this.lineItems.AsReadOnly();
        }

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
        public AccountsPayableInvoices(string guid, string voucherId, DateTime date, VoucherStatus status, string vendorName, string invoiceNumber, DateTime? invoiceDate)
            : base(voucherId, vendorName, date)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Concat( "Guid is a required field. Voucher id: ", voucherId));
            }
            if (string.IsNullOrEmpty(invoiceNumber))
            {
                throw new ArgumentNullException("vendorInvoiceNumber", string.Concat("Vendor Invoice Number is a required field. Entity: 'VOUCHERS', Record ID: '", voucherId, "'"));
         
            }

            this.guid = guid;
            this.status = status;
            this.invoiceNumber = invoiceNumber;
            this.invoiceDate = invoiceDate;
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