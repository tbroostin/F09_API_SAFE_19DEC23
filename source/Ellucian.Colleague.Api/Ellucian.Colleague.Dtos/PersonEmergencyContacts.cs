//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// People who will be contacted in the event of an emergency situation related to the subject person. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonEmergencyContacts : BaseModel2
    {
        /// <summary>
        /// The subject person.
        /// </summary>

        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The details of the person who is the emergency contact.
        /// </summary>

        [JsonProperty("contact", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PersonEmergencyContactsContact Contact { get; set; }

    }
}