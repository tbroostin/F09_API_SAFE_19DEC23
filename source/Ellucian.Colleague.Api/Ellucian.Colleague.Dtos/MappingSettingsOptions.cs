﻿//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details for configuration values used for Ethos integration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MappingSettingsOptions : BaseModel2
    {
        /// <summary>
        /// The full name of the mapping setting.
        /// </summary>
        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// The Ethos resource(s) to which the mapping applies.
        /// </summary>
        [JsonProperty("ethos", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MappingSettingsOptionsEthos Ethos { get; set; }

    }
}
