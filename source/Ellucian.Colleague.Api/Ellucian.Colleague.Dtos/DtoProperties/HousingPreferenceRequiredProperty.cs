//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The person's housing preferences.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class HousingPreferenceRequiredProperty
    {
        /// <summary>
        /// The global identifier for the Preferred.
        /// </summary>
        [JsonProperty("preferred")]
        public GuidObject2 Preferred { get; set; }

        /// <summary>
        /// A indication if the preference is required or optional.
        /// </summary>
        [JsonProperty("required")]
        public RequiredPreference Required { get; set; }
    }
}
