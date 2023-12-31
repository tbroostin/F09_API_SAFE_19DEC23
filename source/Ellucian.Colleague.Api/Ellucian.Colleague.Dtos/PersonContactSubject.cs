﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// People who will be contacted in case of emergency or other situations related to the subject person
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonContactSubject : BaseModel2
    {
        /// <summary>
        /// A global identifier of a person
        /// </summary>
        [JsonProperty("person")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// <see cref="Contacts">Contacts</see> of the person
        /// </summary>
        [JsonProperty("contacts", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Dtos.PersonContact> Contacts { get; set; }
    }
}