//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details for compound configuration values used for Ethos integration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CompoundConfigurationSettings : BaseModel2
    {
        /// <summary>
        /// The full name of the compound configuration setting.
        /// </summary>

        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// The description of the compound configuration setting.
        /// </summary>

        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// The Ethos resources to which the compound configuration applies.
        /// </summary>

        [JsonProperty("ethos", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CompoundConfigurationSettingsEthos> Ethos { get; set; }

        /// <summary>
        /// The source values for the associated configuration.
        /// </summary>

        [JsonProperty("source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CompoundConfigurationSettingsSource Source { get; set; }       

    }
}