// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a person credential
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonCredential3
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
        public IEnumerable<Dtos.DtoProperties.Credential3DtoProperty> Credentials { get; set; }

        /// <summary>
        /// <see cref="AlternativeCredentials">Credentials</see> of the person that uniquely identifies a user.
        /// </summary>
        [JsonProperty("alternativeCredentials", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<AlternativeCredentials> AlternativeCredentials { get; set; }

        /// <summary>
        /// Person credential constructor
        /// </summary>
        public PersonCredential3()
        {
            Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>();         
        }
    }
}