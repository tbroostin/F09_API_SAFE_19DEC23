//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of valid veteran statuses. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn) ]
    public class VeteranStatuses : CodeItem2
    {
        /// <summary>
        /// The name of a higher level veteran status category to which the status belongs.
        /// </summary>

        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VeteranStatusesCategory? Category { get; set; }

    }
}
