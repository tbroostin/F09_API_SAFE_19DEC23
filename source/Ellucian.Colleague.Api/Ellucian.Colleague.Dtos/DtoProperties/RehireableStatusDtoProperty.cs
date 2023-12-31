﻿// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The eligibility status for being rehired. 
    /// </summary>
    [DataContract]
    public class RehireableStatusDtoProperty
    {
        /// <summary>
        /// Enumeration of the eligibility status for being rehired.
        /// </summary>
        [JsonProperty("eligibility", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public RehireEligibility? Eligibility { get; set; }

        /// <summary>
        /// The global identifier for the Type.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Type { get; set; }
    }
}