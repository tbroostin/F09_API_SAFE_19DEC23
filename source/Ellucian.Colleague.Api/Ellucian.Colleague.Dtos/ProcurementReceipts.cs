﻿//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about receipts of goods or services received against approved purchase orders. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ProcurementReceipts : BaseModel2
    {
        /// <summary>
        /// The originating purchase order associated with the procurement receipt.
        /// </summary>
        [JsonProperty("purchaseOrder", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 PurchaseOrder { get; set; }

        /// <summary>
        /// The packing slip number or receiving reference document number associated with the procurement receipt.
        /// </summary>
        [JsonProperty("packingSlipNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PackingSlipNumber { get; set; }

        /// <summary>
        /// The individual line items associated with the procurement receipt.
        /// </summary>
        [JsonProperty("lineItems", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ProcurementReceiptsLineItems> LineItems { get; set; }

        /// <summary>
        /// The shipping method used for the received goods and services (e.g. ground, air, UPS, Purolator).
        /// </summary>
        [JsonProperty("shippingMethod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 ShippingMethod { get; set; }

        /// <summary>
        /// The date on which the goods or services were received.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("receivedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? ReceivedOn { get; set; }

        /// <summary>
        /// The person who received the goods or services.
        /// </summary>
        [JsonProperty("receivedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 ReceivedBy { get; set; }

        /// <summary>
        /// The comment associated with the procurement receipt.
        /// </summary>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }
    }
}