// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The purchase order information exposed from the domain entity
    /// </summary>
    public class PurchaseOrder
    {
        /// <summary>
        /// Purchase Order ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Purchase Order number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The purchase order current status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PurchaseOrderStatus Status { get; set; }

        /// <summary>
        /// The purchase order status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The purchase order total amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The purchase order currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The purchase order date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The purchase order delivery date
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// The purchase order maintenance date
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// The purchase order vendor id
        /// </summary>
        /// 
        public string VendorId { get; set; }

        /// <summary>
        /// The purchase order vendor name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The purchase order initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The purchase order requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// The purchase order AP Type
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// The purchase order ship to code
        /// </summary>
        public string ShipToCode { get; set; }

        /// <summary>
        /// The purchase order ship to code and description
        /// </summary>
        public string ShipToCodeName { get; set; }

        /// <summary>
        /// Used as the default on the commodity code field of the purchase order line items.
        /// </summary>
        public string DefaultCommodityCode { get; set; }

        /// <summary>
        /// The purchase order comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// The purchase order internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// The purchase order default commodity code.
        /// </summary>
        public string CommodityCode { get; set; }
        
        /// <summary>
        /// The prepay voucher id
        /// </summary>
        public string PrepayVoucherId { get; set; }

        /// <summary>
        /// Returns the list of approvers and next approvers for this purchase order
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// This is the associated requisition IDs
        /// </summary>
        public List<string> Requisitions { get; set; }

        /// <summary>
        /// This is the associated vouchers numbers
        /// </summary>
        public List<string> Vouchers { get; set; }

        /// <summary>
        /// List of items associated to this purchase order
        /// </summary>
        public List<LineItem> LineItems { get; set; }
        
        /// <summary>
        /// List of accepted items id
        /// </summary>
        public List<string> AcceptedItems { get; set; }

        /// <summary>
        /// List of email addresses - confirmation email notifications would be sent to these email addresses on create / update .
        /// </summary>
        public List<string> ConfirmationEmailAddresses { get; set; }

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
    }
}
