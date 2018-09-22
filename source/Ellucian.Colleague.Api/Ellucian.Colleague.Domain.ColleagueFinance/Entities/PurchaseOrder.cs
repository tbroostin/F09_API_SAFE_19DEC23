// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Purchase Order
    /// </summary>
    [Serializable]
    public class PurchaseOrder : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The purchase order number
        /// </summary>
        public string Number { get { return number; } }

        /// <summary>
        /// The purchase order type (procurement, travel, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The purchase order submitted by operator for funds availability checking.
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// The purchase order delivery date
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Private current status for public getter
        /// </summary>
        private readonly PurchaseOrderStatus? status;

        /// <summary>
        /// The purchase order current status
        /// </summary>
        public PurchaseOrderStatus? Status { get { return status; } }

        /// <summary>
        /// Private current status date for public getter
        /// </summary>
        private readonly DateTime statusDate;

        /// <summary>
        /// The purchase order current status date
        /// </summary>
        public DateTime StatusDate { get { return statusDate; } }

        /// <summary>
        /// The purchase order initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The purchase order requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// The purchase order ship to code 
        /// </summary>
        public string ShipToCode { get; set; }

        /// <summary>
        /// The purchase order ship to code and description
        /// </summary>
        public string ShipToCodeName { get; set; }

        /// <summary>
        /// The purchase order internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// The private list of requisitions associated with the purchase order
        /// </summary>
        private readonly List<string> requisitions = new List<string>();

        /// <summary>
        /// The public getter for the private list of requisitions associated with the purchase order
        /// </summary>
        public ReadOnlyCollection<string> Requisitions { get; private set; }

        /// <summary>
        /// The private list of vouchers associated with the purchase order
        /// </summary>
        private readonly List<string> vouchers = new List<string>();

        /// <summary>
        /// The public getter for the private list of vouchers associated with the purchase order
        /// </summary>
        public ReadOnlyCollection<string> Vouchers { get; private set; }

        /// <summary>
        /// The reference number
        /// </summary>		
        public List<string> ReferenceNo { get; set; }

        /// <summary>
        /// Free On Board
        /// </summary>
        public string Fob { get; set; }

        /// <summary>
        /// Vendor Terms
        /// </summary	
        public string VendorTerms { get; set; }

        /// <summary>
        ///  Address information for a vendor
        /// </summary>
        public string VendorAddressId { get; set; }

        /// <summary>
        /// Host Country
        /// </summary	
        public string HostCountry { get; set; }

        /// <summary>
        /// Buyer
        /// </summary>
        public string Buyer { get; set; }

        /// <summary>
        /// Alternate Shipping Name
        /// </summary>
        public string AltShippingName { get; set; }

        /// <summary>
        /// Alternate Shipping Address
        /// </summary>
        public List<string> AltShippingAddress { get; set; }

        /// <summary>
        /// Alternate Shipping City
        /// </summary>
        public string AltShippingCity { get; set; }

        /// <summary>
        /// Alternate Shipping State
        /// </summary>
        public string AltShippingState { get; set; }

        /// <summary>
        /// Alternate Shipping Zip
        /// </summary>
        public string AltShippingZip { get; set; }

        /// <summary>
        /// Alternate Shipping Country
        /// </summary>
        public string AltShippingCountry { get; set; }

        /// <summary>
        /// Alternate Shipping Phone
        /// </summary>
        public string AltShippingPhone { get; set; }

        /// <summary>
        /// Alternate Shipping Phone Extension
        /// </summary>
        public string AltShippingPhoneExt { get; set; }

        /// <summary>
        /// Vendor alternitive address flag
        /// </summary>
        public bool AltAddressFlag { get; set; }

        /// <summary>
        /// Misc Country
        /// </summary>
        public string MiscCountry { get; set; }

        /// <summary>
        /// Misc Name
        /// </summary>		
        public List<string> MiscName { get; set; }

        /// <summary>
        /// Misc Address
        /// </summary>
        public List<string> MiscAddress { get; set; }

        /// <summary>
        /// Misc City
        /// </summary>
        public string MiscCity { get; set; }

        /// <summary>
        /// Misc State
        /// </summary>
        public string MiscState { get; set; }

        /// <summary>
        /// Misc Zip
        /// </summary>
        public string MiscZip { get; set; }

        /// <summary>
        /// Misc Zip
        /// </summary>
        public string MiscIntgCorpPersonFlag { get; set; }

        /// <summary>
        /// Used as the default on the initiator field of the purchase order line items.
        /// </summary>
        public string DefaultInitiator { get; set; }

        /// <summary>
        /// Void GL Transaction Date
        /// </summary>
        public DateTime? VoidGlTranDate { get; set; }
     
        public List<string> OutstandingItemsId { get; set; }

        public List<string> AcceptedItemsId { get; set; }

        public bool bypassTaxForms { get; set; }

        public bool bypassApprovals { get; set; }

        /// <summary>
        /// This constructor initializes the purchase order domain entity
        /// </summary>
        /// <param name="id">Purchase order ID</param>
        /// <param name="number">Purchase order number</param>
        /// <param name="vendor name">Purchase order vendor name</param>
        /// <param name="status">Purchase order status</param>
        /// <param name="statusDate">Purchase order status date</param>
        /// <param name="date">Purchase order date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public PurchaseOrder(string id, string number, string vendorName, PurchaseOrderStatus? status, DateTime statusDate, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;
            this.status = status;
            this.statusDate = statusDate;
            Requisitions = this.requisitions.AsReadOnly();
            Vouchers = this.vouchers.AsReadOnly();
        }


        /// <summary>
        /// This constructor initializes the purchase order domain entity
        /// </summary>
        /// <param name="id">Purchase order ID</param>
        /// <param name="guid">Purchase order GUID</param>
        /// <param name="number">Purchase order number</param>
        /// <param name="vendor name">Purchase order vendor name</param>
        /// <param name="status">Purchase order status</param>
        /// <param name="statusDate">Purchase order status date</param>
        /// <param name="date">Purchase order date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public PurchaseOrder(string id, string guid, string number, string vendorName, PurchaseOrderStatus? status, DateTime statusDate, DateTime date)
            : base(id, guid, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;
            this.status = status;
            this.statusDate = statusDate;
            Requisitions = this.requisitions.AsReadOnly();
            Vouchers = this.vouchers.AsReadOnly();
        }
        /// <summary>
        /// This method adds a requisition to the list of requisitions for the purchase order
        /// </summary>
        /// <param name="requisition">The associated requisition</param>
        public void AddRequisition(string requisition)
        {
            if (string.IsNullOrEmpty(requisition))
            {
                throw new ArgumentNullException("requisition", "Requisition cannot be null");
            }

            if (requisitions != null && !this.requisitions.Contains(requisition))
            {
                this.requisitions.Add(requisition);
            }
        }

        /// <summary>
        /// This method adds a voucher to the list of vouchers for the purchase order
        /// </summary>
        /// <param name="voucher">The associated voucher</param>
        public void AddVoucher(string voucher)
        {
            if (string.IsNullOrEmpty(voucher))
            {
                throw new ArgumentNullException("voucher", "Voucher cannot be null");
            }

            if (vouchers != null && !this.vouchers.Contains(voucher))
            {
                this.vouchers.Add(voucher);
            }
        }
    }
}
