//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The titles and values for the configuration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CompoundConfigurationSettingsOptionsSecondaryValue
    {
        /// <summary>
        /// The origin of the primary value
        /// </summary>

        [JsonProperty("origin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Origin { get; set; }

        /// <summary>
        /// The source options of the primary value.
        /// </summary>

        [JsonProperty("sourceOptions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CompoundConfigurationSettingsSourceOptions> SourceOptions { get; set; }

    }
}
