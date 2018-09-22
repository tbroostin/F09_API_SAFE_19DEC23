//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;


namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The list of valid statuses for admission application supporting items. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AdmissionApplicationSupportingItemStatus : CodeItem2
    {
        /// <summary>
        /// The type of the admission application supporting item status.
        /// </summary>

        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AdmissionApplicationSupportingItemStatusType Type { get; set; }

    }
}
