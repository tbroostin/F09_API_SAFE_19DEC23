// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Credential DTO Property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PersonMatchRequestCredentialDtoProperty
    {
        /// <summary>
        /// The credential type of the credential
        /// </summary>
        [JsonProperty("type")]
        public Dtos.EnumProperties.PersonMatchRequestCredentialType? Type { get; set; }

        /// <summary>
        /// The value of the credential
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
