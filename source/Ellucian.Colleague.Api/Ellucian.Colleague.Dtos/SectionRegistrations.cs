//Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A record of a student's interaction with a specific section such as registration, grades, involvement. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionRegistrations : BaseModel2
    {
        /// <summary>
        /// A person registering for a section.
        /// </summary>
        [JsonProperty("registrant", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Registrant { get; set; }

        /// <summary>
        /// The requested sections for registration activity
        /// </summary>
        [JsonProperty("sections", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<SectionRegistration5> Sections { get; set; }
    }
}