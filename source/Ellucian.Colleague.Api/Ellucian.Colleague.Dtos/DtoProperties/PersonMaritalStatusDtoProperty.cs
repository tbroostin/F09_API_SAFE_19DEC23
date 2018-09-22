// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The current marital state (for example, single, married, or divorced) of the person.
    /// </summary>
    [DataContract]
    public class PersonMaritalStatusDtoProperty
    {
        /// <summary>
        /// The higher-level marital category of the person.
        /// </summary>
        [JsonProperty("maritalCategory")]
        public PersonMaritalStatusCategory? MaritalCategory { get; set; }

        /// <summary>
        /// Globally unique identifier for marital status code
        /// </summary>
        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Detail { get; set; }
    }
}