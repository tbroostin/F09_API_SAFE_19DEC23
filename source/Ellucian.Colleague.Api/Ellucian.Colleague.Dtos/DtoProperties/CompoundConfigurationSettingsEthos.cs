﻿//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The Ethos resources to which the compound configuration applies. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CompoundConfigurationSettingsEthos
    {
        
        /// <summary>
        /// The Ethos resource to which the compound configuration applies.
        /// </summary>

        [JsonProperty("resource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public string Resource { get; set; }

        /// <summary>
        /// The Ethos property to which the compound configuration applies.
        /// </summary>

        [JsonProperty("propertyName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public string PropertyName { get; set; }


    }
}
