// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a person credential
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonCredential2
    {
        /// <summary>
        /// A global identifier of a person
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// <see cref="Credential">Credentials</see> of the person
        /// </summary>
        [JsonProperty("credentials", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<Dtos.DtoProperties.CredentialDtoProperty2> Credentials { get; set; }

        /// <summary>
        /// Person credential constructor
        /// </summary>
        public PersonCredential2()
        {
            Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>();
        }
    }
}