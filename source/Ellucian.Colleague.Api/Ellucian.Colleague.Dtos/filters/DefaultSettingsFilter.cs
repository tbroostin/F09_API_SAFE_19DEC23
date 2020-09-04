// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// advancedSearch named query
    /// </summary>
    public class DefaultSettingsFilter
    {
        /// <summary>
        /// Advanced Search filter by keyword
        /// </summary>        
        [JsonProperty("keyword")]
        [FilterProperty("advancedSearch")]
        public string Keyword { get; set; }

        /// <summary>
        /// Advanced Search filter by Default Settings Guid
        /// </summary>        
        [JsonProperty("defaultSettings")]
        [FilterProperty("advancedSearch")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 DefaultSettings { get; set; }
    }
}
