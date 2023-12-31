﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.Interfaces;
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
    public class LocationDtoProperty
    {
        /// <summary>
        /// The location information for this instructional event
        /// </summary>
        [JsonConverter(typeof(InstructionalLocationDtoPropertyConverter))]
        [JsonProperty("location")]
        public InstructionalLocationDtoProperty Location { get; set; }
    }
}