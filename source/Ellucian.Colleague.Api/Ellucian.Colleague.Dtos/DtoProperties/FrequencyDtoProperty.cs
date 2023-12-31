﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The frequency at which the performance review occurs.
    /// </summary>
    [DataContract]
    public class FrequencyDtoProperty
    {
        /// <summary>
        /// The unit of measure for the frequency
        /// </summary>
        [JsonProperty("unit")]
        public FrequencyUnitType Unit { get; set; }

        /// <summary>
        /// The value of the frequency at which the performance review occurs
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Value { get; set; }
    }
}