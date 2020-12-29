// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor named query
    /// </summary>
    public class PersonFilterFilter2
    {
        /// <summary>
        /// person Filter - Guid for SAVE.LIST.PARMS which contains a savedlist of person IDs
        /// </summary>        
        [JsonProperty("personFilter")]
        [FilterProperty("personFilter")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 personFilter { get; set; }        
    }
}