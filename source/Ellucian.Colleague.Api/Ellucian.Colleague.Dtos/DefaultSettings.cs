//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details for configuration values used for Ethos integration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DefaultSettings : BaseModel2
    {
        /// <summary>
        /// The full name of the configuration setting.
        /// </summary>
        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// The Ethos resource(s) to which the configuration applies.
        /// </summary>
        [JsonProperty("ethos", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DefaultSettingsEthos> Ethos { get; set; }

        /// <summary>
        /// The source values for the configuration.
        /// </summary>
        [JsonProperty("source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DefaultSettingsSource Source { get; set; }

        /// <summary>
        /// The full description of the source similar to field help.
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// The advanced search details for large datasets.
        /// </summary>
        [JsonProperty("advancedSearch", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DefaultAdvancedSearchProperty AdvancedSearch { get; set; }
    }
}
