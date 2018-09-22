// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the blanket purchase order DTO
    /// </summary>
    public class BlanketPurchaseOrder
    {
        /// <summary>
        /// Blanket Purchase Order ID
        /// </summary>
        public string Id {get; set;}

        /// <summary>
        /// Blanket Purchase Order number
        /// </summary>
        public string Number {get; set;}

        /// <summary>
        /// Blanket Purchase Order current status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BlanketPurchaseOrderStatus Status { get; set; }

        /// <summary>
        /// Blanket Purchase Order current status date
        /// </summary>
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// The purchase order total amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Blanket Purchase Order currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Blanket Purchase Order date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Blanket Purchase Order maintenance date
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// Blanket Purchase Order expiration date
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Blanket Purchase Order vendor id
        /// </summary>
        /// 
        public string VendorId { get; set; }

        /// <summary>
        /// Blanket Purchase Order vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Blanket Purchase Order initiator name
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// Blanket Purchase Order AP Type
        /// </summary>
        public string ApType { get; set; }

        /// <summary>
        /// Blanket Purchase Order Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Blanket Purchase Order comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Blanket Purchase Order internal comments
        /// </summary>
        public string InternalComments { get; set; }

        /// <summary>
        /// Returns the list of approvers and next approvers for this Blanket Purchase Order
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// One or more associated requisition IDs 
        /// </summary>
        public List<string> Requisitions { get; set; }

        /// <summary>
        /// One or more associated voucher numbers
        /// </summary>
        public List<string> Vouchers { get; set; }

        /// <summary>
        /// List of the Blanket Purchase Order GL distributions
        /// </summary>
        public List<BlanketPurchaseOrderGlDistribution> GlDistributions { get; set; }
    }
}
