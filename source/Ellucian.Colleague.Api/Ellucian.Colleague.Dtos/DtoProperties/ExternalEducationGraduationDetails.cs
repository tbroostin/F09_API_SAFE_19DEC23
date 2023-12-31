﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details of the educational institution where the person graduated.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ExternalEducationGraduationDetails
    {

        /// <summary>
        /// The source of educational institution where the person graduated.
        /// </summary>
        [JsonProperty("source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ExternalEducationGraduationDetailsSource? Source { get; set; }

        /// <summary>
        /// The date the student graduated from the source institution.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("graduatedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? GraduatedOn { get; set; }
   
    }
}