// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a Requisition
    /// </summary>
    [Serializable]
    public class Requisition : AccountsPayablePurchasingDocument
    {
        /// <summary>
        /// Private number for public getter
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The requisition number
        /// </summary>
        public string Number { get { return number; } }

        /// <summary>
        /// The requistion type (procurement, eprocurement, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The requisition desired date
        /// </summary>
        public DateTime? DesiredDate { get; set; }

        /// <summary>
        /// Private current status for public getter
        /// </summary>
        private readonly RequisitionStatus status;

        /// <summary>
        /// The requisition current status
        /// </summary>
        public RequisitionStatus Status { get { return status; } }

        /// <summary>
        /// Private current status date for public getter
        /// </summary>
        private readonly DateTime statusDate;

        /// <summary>
        /// The requisition current status date
        /// </summary>
        public DateTime StatusDate { get { return statusDate; } }

        /// <summary>
        /// The requisition initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The requisition requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// The requisition ship to code
        /// </summary>
        public string ShipToCode { get; set; }

        /// <summary>
        /// Requisition commodity code
        /// </summary>
        public string CommodityCode { get; set; }

        /// <summary>
        /// The blanket purchase order associated with the requisition
        /// </summary>
        public string BlanketPurchaseOrder { get;  set; }

        /// <summary>
        /// The requisition internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// The private list of purchase orders associated with the requisition
        /// </summary>
        private readonly List<string> purchaseOrders = new List<string>();

        /// <summary>
        /// The public getter for the private list of purchase orders associated with the requisition
        /// </summary>
        public ReadOnlyCollection<string> PurchaseOrders { get; private set; }

        /// <summary>
        /// ID indicating the F.O.B. (free on board) code for this requisition.
        /// Indicates the arrangement for shipping cost liability made between vendor and customer
        /// </summary>
        public string Fob { get; set; }

        /// <summary>
        /// Vendor cash discount terms
        /// </summary	
        public string VendorTerms { get; set; }

        /// <summary>
        /// Host Country
        /// </summary	
        public string HostCountry { get; set; }

        /// <summary>
        /// The suggested or desired buyer for the items on the requisition
        /// </summary>
        public string Buyer { get; set; }

        /// <summary>
        /// Alternate name where the ordered goods should be shipped
        /// </summary>
        public string AltShippingName { get; set; }

        /// <summary>
        /// Alternate address where the ordered goods should be shipped
        /// </summary>
        public List<string> AltShippingAddress { get; set; }

        /// <summary>
        /// Alternate city where the ordered goods should be shipped
        /// </summary>
        public string AltShippingCity { get; set; }

        /// <summary>
        /// Alternate state where the ordered goods should be shipped
        /// </summary>
        public string AltShippingState { get; set; }

        /// <summary>
        /// Alternate postal code where the ordered goods should be shipped
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
        /// The vendor country code associated with the miscellaneous vendor 
        /// </summary>
        public string MiscCountry { get; set; }

        /// <summary>
        /// The vendor name associated with the miscellaneous vendor
        /// </summary>		
        public List<string> MiscName { get; set; }

        /// <summary>
        /// The vendor address for a miscellaneous vendor
        /// </summary>
        public List<string> MiscAddress { get; set; }

        /// <summary>
        /// The vendor city for a miscellaneous vendor
        /// </summary>
        public string MiscCity { get; set; }

        /// <summary>
        /// The vendor state for a miscellaneous vendor
        /// </summary>
        public string MiscState { get; set; }

        /// <summary>
        /// The vendor postal code for a miscellaneous vendor
        /// </summary>
        public string MiscZip { get; set; }

        /// <summary>
        /// Used as the default on the initiator field of the purchase order line items.
        /// </summary>
        public string DefaultInitiator { get; set; }

        /// <summary>
        /// The date the requisition originator would like to receive the items.
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Integration Corporation or Person Indicator
        /// </summary>
        public string IntgCorpPerIndicator { get; set; }

        /// <summary>
        /// Integration Alternative Shipping Country
        /// </summary>
        public string IntgAltShipCountry { get; set; }

        /// <summary>
        /// Integration Submitted By operator for funds availability checking.
        /// </summary>
        public string IntgSubmittedBy { get; set; }

        /// <summary>
        /// flag to bypass tax forms.
        /// </summary>
        public bool bypassTaxForms { get; set; }

        /// <summary>
        /// flag to bypass approvals
        /// </summary>
        public bool bypassApprovals { get; set; }

        /// <summary>
        /// Vendor PreferredAddressId
        /// </summary>
        public string VendorPreferredAddressId { get; set; }
        public string VendorAlternativeAddressId { get; set; }
        public bool UseAltAddress { get; set; }

        /// <summary>
        /// List of email addresses - confirmation email notifications would be sent to these email addresses on create / update .
        /// </summary>
        public List<string> ConfirmationEmailAddresses { get; set; }

        /// <summary>
        /// This constructor initializes the requisition domain entity
        /// </summary>
        /// <param name="id">Requisition ID</param>
        /// <param name="number">Requisition number</param>
        /// <param name="vendorName">Requisition vendor name</param>
        /// <param name="status">Requisition status</param>
        /// <param name="statusDate">Requisition status date</param>
        /// <param name="date">Requisition date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public Requisition(string id, string number, string vendorName, RequisitionStatus status, DateTime statusDate, DateTime date)
            : base(id, vendorName, date)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Number is a required field.");
            }

            this.number = number;
            this.status = status;
            this.statusDate = statusDate;
            PurchaseOrders = this.purchaseOrders.AsReadOnly();
        }

        /// <summary>
        /// This constructor initializes the requisition domain entity
        /// </summary>
        /// <param name="id">Requisition ID</param>
        /// <param name="guid">GUID</param>
        /// <param name="number">Requisition number</param>
        /// <param name="vendorName">Requisition vendor name</param>
        /// <param name="status">Requisition status</param>
        /// <param name="statusDate">Requisition status date</param>
        /// <param name="date">Requisition date</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null</exception>
        public Requisition(string id, string guid, string number, string vendorName, RequisitionStatus status, DateTime statusDate, DateTime date)
            : base(id, guid, vendorName, date)
        {
           
            this.number = number;
            this.status = status;
            this.statusDate = statusDate;
            PurchaseOrders = this.purchaseOrders.AsReadOnly();
        }


        /// <summary>
        /// This method adds a purchase order to the list of purchase orders for the requisition
        /// </summary>
        /// <param name="purchaseOrder">The associated purchase order</param>
        public void AddPurchaseOrder(string purchaseOrder)
        {
            if (string.IsNullOrEmpty(purchaseOrder))
            {
                throw new ArgumentNullException("purchaseOrder", "Purchase Order cannot be null");
            }
            if (purchaseOrders != null && !this.purchaseOrders.Contains(purchaseOrder))
            {
                this.purchaseOrders.Add(purchaseOrder);
            }
        }
    }
}
