// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// purchase order summary
    /// </summary>
    public class PurchaseOrderSummary
    {
        /// <summary>
        /// purchase order ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The purchase order number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The purchase order submitted by operator for funds availability checking.
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// The purchase order current status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PurchaseOrderStatus Status { get; set; }

        /// <summary>
        /// The purchase order current status date
        /// </summary>
        public DateTime StatusDate { get; set; }
        
        /// <summary>
        /// The purchase order date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The purchase order total amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The purchase order vendor id
        /// </summary>        
        public string VendorId { get; set; }

        /// <summary>
        /// The purchase order vendor name
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
        /// List of requisitions associated to the purchase order
        /// </summary>
        public List<RequisitionLinkSummary> Requisitions { get; set; }

        /// <summary>
        /// List of voucher ids associated to the purchase order
        /// </summary>
        public List<string> VoucherIds { get; set; }

        /// <summary>
        /// List of Approvers associated to this purchase order
        /// </summary>
        public List<Approver> Approvers { get; set; }



    }
}

