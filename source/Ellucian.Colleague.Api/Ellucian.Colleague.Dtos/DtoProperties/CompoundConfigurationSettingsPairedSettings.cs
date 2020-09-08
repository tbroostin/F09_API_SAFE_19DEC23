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
    public class CompoundConfigurationSettingsPairedSettings
    {


        /// <summary>
        /// The primary title for the configuration.
        /// </summary>

        [JsonProperty("primaryTitle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PrimaryTitle { get; set; }

        /// <summary>
        /// The primary value for the configuration.
        /// </summary>

        [JsonProperty("primaryValue", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PrimaryValue { get; set; }

        /// <summary>
        /// The secondary title for the configuration.
        /// </summary>

        [JsonProperty("secondaryTitle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SecondaryTitle { get; set; }

        /// <summary>
        /// The secondary value for the configuration.
        /// </summary>

        [JsonProperty("secondaryValue", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SecondaryValue { get; set; }


    }
}
