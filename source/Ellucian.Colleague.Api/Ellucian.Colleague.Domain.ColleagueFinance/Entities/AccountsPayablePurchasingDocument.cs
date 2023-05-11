// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This class represents an Accounts Payable/Purchasing document and is intended to be
    /// extended by the various AP/Purchasing documents (e.g. Voucher, PO, BPO, etc.).
    /// </summary>
    [Serializable]
    public abstract class AccountsPayablePurchasingDocument : BaseFinanceDocument
    {
        /// <summary>
        /// Document vendor ID.
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Private variable for the vendor name.
        /// </summary>
        private readonly string vendorName;

        /// <summary>
        /// This is the public getter for the private vendor name.
        /// </summary>
        public string VendorName { get { return vendorName; } }

        /// <summary>
        /// Vendor address.
        /// </summary>
        public string VendorAddress { get; set; }

        /// <summary>
        /// Address type code
        /// </summary>
        public string VendorAddressTypeCode { get; set; }

        /// <summary>
        /// Address type description
        /// </summary>
        public string VendorAddressTypeDesc { get; set; }

        /// <summary>
        /// Document amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Document maintenance date.
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// Document currency code.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Document AP type.
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// Flag to indicate if document has attachment/s associated
        /// </summary>
        public bool AttachmentsIndicator { get; set; }

        /// <summary>
        /// Flag to indicate if document has returned from approval
        /// </summary>
        public bool ApprovalReturnedIndicator { get; set; }

        /// <summary>
        /// This is the private list of line items associated with the document.
        /// </summary>
        private readonly List<LineItem> lineItems = new List<LineItem>();

        /// <summary>
        /// This is the public getter for the private list of line items associated with the document.
        /// </summary>
        public ReadOnlyCollection<LineItem> LineItems { get; private set; }

        /// <summary>
        /// Create a new AP/Purchasing document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="date">Document date</param>
        public AccountsPayablePurchasingDocument(string documentId, string vendorName, DateTime date)
            : base(documentId, date)
        {
            this.vendorName = vendorName;
            LineItems = this.lineItems.AsReadOnly();
        }

        /// <summary>
        /// Create a new AP/Purchasing document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="guid">GUID</param>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="date">Document date</param>
        public AccountsPayablePurchasingDocument(string documentId, string guid, string vendorName, DateTime date)
            : base(documentId, guid, date)
        {
            this.vendorName = vendorName;
            LineItems = this.lineItems.AsReadOnly();
        }

        /// <summary>
        /// This method adds a line item to the document.
        /// </summary>
        /// <param name="lineItem">Line item object.</param>
        public void AddLineItem(LineItem lineItem)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException("lineItem", "Line item cannot be null");
            }

            if (this.lineItems != null && (this.lineItems.Where(x => x.Id == lineItem.Id).Count() == 0 || lineItem.Id == "NEW"))
            {
                this.lineItems.Add(lineItem);
            }
        }
    }
}
