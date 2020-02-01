// Copyright 2015 Ellucian Company L.P. and its affiliates.

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
    /// The requisition information exposed from the domain entity
    /// </summary>
    public class Requisition
    {
        /// <summary>
        /// Requisition ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The requisition number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The requisition current status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RequisitionStatus Status { get; set; }

        /// <summary>
        /// The requisition current status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The requisition total amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The requisition currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The requisition date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The requisition desired date
        /// </summary>
        public DateTime? DesiredDate { get; set; }

        /// <summary>
        /// The requisition maintenance date
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// The requisition vendor id
        /// </summary>
        /// 
        public string VendorId { get; set; }

        /// <summary>
        /// The requisition vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The requisition initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// The requisition requestor name
        /// </summary>
        public string RequestorName { get; set; }

        /// <summary>
        /// The requisition AP Type
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// The requisition ship to code
        /// </summary>
        public string ShipToCode { get; set; }

        /// <summary>
        /// Requisition commodity code
        /// </summary>
        public string CommodityCode { get; set; }

        /// <summary>
        /// The blanket purchase order ID associated to this requisition
        /// </summary>
        public string BlanketPurchaseOrder { get; set; }

        /// <summary>
        /// The requisition comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// The requisition internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// List of approvers and next approvers for this requisition
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// List of purchase order IDs associated to this requisition
        /// </summary>
        public List<string> PurchaseOrders { get; set; }

        /// <summary>
        /// List of items associated to this requisition
        /// </summary>
        public List<LineItem> LineItems { get; set; }

    }
}
