﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The privacy status of person's information.
    /// </summary>
    [DataContract]
    public class PersonPrivacyDtoProperty
    {
        /// <summary>
        /// The global category of information privacy protection.
        /// </summary>
        [JsonProperty("privacyCategory")]
        public PrivacyStatusType? PrivacyCategory { get; set; }

        /// <summary>
        /// Globally unique identifier for privacy code
        /// </summary>
        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Detail { get; set; }
    }
}