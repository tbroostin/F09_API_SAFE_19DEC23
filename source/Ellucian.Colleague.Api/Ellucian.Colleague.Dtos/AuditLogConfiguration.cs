// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// An Extended Data representation.
    /// </summary>
    [DataContract]
    public class AuditLogConfiguration
    {
        /// <summary>
        /// Event ID key to Valcode associated to Audit Category
        /// </summary>
        [JsonProperty("eventid", NullValueHandling = NullValueHandling.Ignore)]
        public string EventId { get; set; }

        /// <summary>
        /// Description from Valcode table
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// Description of the Log type (not available in Colleague at this time)
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Flag to indicate if the audit category is enabled.
        /// </summary>
        [JsonProperty("isenabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// AuditLogConfiguration constructor
        /// </summary>
        public AuditLogConfiguration() : base() { }
    }
}
