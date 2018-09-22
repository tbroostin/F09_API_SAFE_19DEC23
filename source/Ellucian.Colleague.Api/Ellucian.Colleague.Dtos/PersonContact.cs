// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The subject person's contacts
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonContact
    {
        /// <summary>
        /// The name of the contact
        /// </summary>
        [JsonProperty("name")]
        public PersonContactName ContactName { get; set; }

        /// <summary>
        /// The contact types
        /// </summary>
        [JsonProperty("types")]
        public IEnumerable<Ellucian.Colleague.Dtos.EnumProperties.PersonContactType> Types { get; set; }

        /// <summary>
        /// The contact address
        /// </summary>
        [JsonProperty("contactAddress", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public PersonContactAddress ContactAddress { get; set; }

        /// <summary>
        /// The contact phone
        /// </summary>
        [JsonProperty("phones", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<PersonContactPhoneDtoProperty> PersonContactPhone { get; set; }

        /// <summary>
        /// The contact relationship
        /// </summary>
        [JsonProperty("relationship", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string ContactRelationship { get; set; }

        /// <summary>
        /// The contact priority
        /// </summary>
        [JsonProperty("priority", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int ContactPriority { get; set; }
    }
}
