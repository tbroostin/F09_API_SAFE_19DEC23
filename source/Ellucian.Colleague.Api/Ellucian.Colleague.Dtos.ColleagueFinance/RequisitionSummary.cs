// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is a Requisition Summary
    /// </summary>
    public class RequisitionSummary
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
        /// The requisition date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The requisition total amount
        /// </summary>
        public decimal Amount { get; set; }

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
        /// The blanket purchase order Id associated with the requisition
        /// </summary>
        public string BlanketPurchaseOrderId { get; set; }

        /// <summary>
        /// The blanket purchase order number associated with the requisition
        /// </summary>
        public string BlanketPurchaseOrderNumber { get; set; }

        /// <summary>
        /// List of purchase orders associated to this requisition
        /// </summary>
        public List<PurchaseOrderLinkSummary> PurchaseOrders { get; set; }

        /// <summary>
        /// List of Approvers associated to this requisition
        /// </summary>
        public List<Approver> Approvers { get; set; }

    }
}
