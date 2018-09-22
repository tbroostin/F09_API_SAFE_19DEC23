// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A physical location within the organization where the event will take place
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Location
    {
        /// <summary>
        /// The location information for this instructional event
        /// </summary>
        [JsonConverter(typeof(InstructionalLocation2Converter))]
        [JsonProperty("location")]
        public InstructionalLocation2 Locations { get; set; }
    }
}