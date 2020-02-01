//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The outcome of the person matching request. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonMatchingRequestsOutcomesDtoProperty
    {
        /// <summary>
        /// The type of outcome (initial or final).
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public PersonMatchingRequestsType Type { get; set; }

        /// <summary>
        /// The outcome status of the person matching request.
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public PersonMatchingRequestsStatus Status { get; set; }

        /// <summary>
        /// The date and time associated with the status of the person matching request.
        /// </summary>
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date { get; set; }
    }
}