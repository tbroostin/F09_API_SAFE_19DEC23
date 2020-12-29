// Copyright 2015 - 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a voucher.
    /// </summary>
    [Serializable]
    public class Voucher : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// GUID for the voucher; not required, but cannot be changed once assigned.
        /// </summary>
        private string guid;
        
        public string Guid
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
        /// This is the voucher invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// This is the voucher invoice date.
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

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
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
                }
                else if (!string.IsNullOrEmpty(recurringVoucherId))
                {
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
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
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
                }
                else if (!string.IsNullOrEmpty(recurringVoucherId))
                {
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
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
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
                }
                else if (!string.IsNullOrEmpty(blanketPurchaseOrderId))
                {
                    throw new ApplicationException("A voucher can only have either a PO, BPO, or recurring voucher.");
                }
                else
                {
                    this.recurringVoucherId = value;
                }
            }
        }

        /// <summary>
        /// Vendor Address lines for a voucher.        
        /// </summary>
        public List<string> VendorAddressLines { get; set; }

        /// <summary>
        /// Vendor City for a voucher.
        /// </summary>
        public string VendorCity { get; set; }

        /// <summary>
        /// Vendor state for a voucher.
        /// </summary>
        public string VendorState { get; set; }

        /// <summary>
        /// Vendor postal code for a voucher.
        /// </summary>
        public string VendorZip { get; set; }

        /// <summary>
        /// Vendor country for a voucher.
        /// </summary>
        public string VendorCountry { get; set; }

        /// <summary>
        /// Voucher status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// List of email addresses - confirmation email notifications would be sent to these email addresses on create / update .
        /// </summary>
        public List<string> ConfirmationEmailAddresses { get; set; }

        /// <summary>
        /// This constructor initializes the voucher domain entity.
        /// </summary>
        /// <param name="voucherId">This is the voucher ID.</param>
        /// <param name="date">This is the voucher date.</param>
        /// <param name="status">This is the voucher status.</param>
        /// <param name="vendorName">This is the voucher vendor name.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public Voucher(string voucherId, DateTime date, VoucherStatus status, string vendorName)
            : base(voucherId, vendorName, date)
        {
            this.status = status;
        }

        /// <summary>
        /// This constructor initializes the voucher domain entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="voucherId">This is the voucher ID.</param>
        /// <param name="date">This is the voucher date.</param>
        /// <param name="status">This is the voucher status.</param>
        /// <param name="vendorName">This is the voucher vendor name.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the applicable parameters are null.</exception>
        public Voucher(string guid, string voucherId, DateTime date, VoucherStatus status, string vendorName)
            : base(voucherId, vendorName, date)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is a required field. Voucher id: " + voucherId);
            } 

            this.guid = guid;
            this.status = status;
        }
    }
}