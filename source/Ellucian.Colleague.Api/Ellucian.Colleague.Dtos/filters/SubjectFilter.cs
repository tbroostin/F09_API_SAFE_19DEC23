﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Subject named query
    /// </summary>
    public class SubjectFilter
    {
        /// <summary>
        /// subject
        /// </summary>        
        [DataMember(Name = "subject", EmitDefaultValue = false)]
        [FilterProperty("subject")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 SubjectName { get; set; }
    }
}
