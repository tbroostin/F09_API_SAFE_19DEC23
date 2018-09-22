// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The employement status type and detail
    /// </summary>
    [DataContract]
    public class ContractTypeDtoProperty
    {

        /// <summary>
        /// The type of employment (Eg. Full-time or Part-time).
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EnumProperties.ContractType? Type { get; set; }

        /// <summary>
        /// The Guid of the type of employment (Eg. Full-time or Part-time).
        /// </summary>
        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Detail { get; set; }
    }
}