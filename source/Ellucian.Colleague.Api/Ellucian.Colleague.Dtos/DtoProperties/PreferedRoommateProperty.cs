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
    /// The student's preferred roommate.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PreferedRoommateProperty
    {
        /// <summary>
        /// The student's preferred roommate.
        /// </summary>
        [JsonProperty("roommate")]
        public GuidObject2 Roommate { get; set; }

        /// <summary>
        /// The student's preferred roommate characteristic.
        /// </summary>
        [JsonProperty("roommateCharacteristic", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 RoommateCharacteristic { get; set; }

    }
}
