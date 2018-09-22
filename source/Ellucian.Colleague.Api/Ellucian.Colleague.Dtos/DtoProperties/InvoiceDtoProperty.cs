//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The invoices or refunds for which the payment occurred. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InvoiceDtoProperty
    {

        /// <summary>
        /// The invoice for which the payment occurred.
        /// </summary>
        [JsonProperty("invoice", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Invoice { get; set; }

        /// <summary>
        /// The refund for which the payment occurred.
        /// </summary>
        [JsonProperty("refund", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Refund { get; set; }
    }
}