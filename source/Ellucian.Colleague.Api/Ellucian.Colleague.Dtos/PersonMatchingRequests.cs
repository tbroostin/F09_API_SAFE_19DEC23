//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about new person requests that are processed through duplicate prevention matching rules. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonMatchingRequests : BaseModel2
    {
        /// <summary>
        /// The new or matched person record.
        /// </summary>
        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The outcome of the person matching request.
        /// </summary>
        [JsonProperty("outcomes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<PersonMatchingRequestsOutcomesDtoProperty> Outcomes { get; set; }

        /// <summary>
        /// The originator (user or system) of the person matching request.
        /// </summary>
        [JsonProperty("originator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public string Originator { get; set; }
    }
}
