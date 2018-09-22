// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// A type of phone for a person.  Includes a standard type (e.g. 'mobile', 'home') and an optional, detail id.
    /// </summary>
    [DataContract]
    public class PersonPhoneTypeDtoProperty
    {
        /// <summary>
        /// A type of phone contact for a person.
        /// </summary>
        [JsonProperty("phoneType")]
        public PersonPhoneTypeCategory? PhoneType { get; set; }

        /// <summary>
        /// Globally unique identifier for phone number
        /// </summary>
        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Detail { get; set; }
    }
}