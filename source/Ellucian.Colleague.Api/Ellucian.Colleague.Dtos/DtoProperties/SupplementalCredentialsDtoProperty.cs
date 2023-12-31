﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The supplemental academic credential (degree, diploma, etc.) the person earned at the institution. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SupplementalCredentialsDtoProperty
    {

        /// <summary>
        /// The academic credential (degree, diploma, etc.) the person earned at the institution.
        /// </summary>
        [JsonProperty("credential", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Credential { get; set; }

        /// <summary>
        /// The date when the person earned the credential.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("earnedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? EarnedOn { get; set; }

    }
}