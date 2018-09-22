//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The person who has the relationship with the subject person.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonRelationshipsRelatedPerson
    {
        /// <summary>
        /// The person who has the relationship with the subject person.
        /// </summary>

        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 person { get; set; }
    }
}