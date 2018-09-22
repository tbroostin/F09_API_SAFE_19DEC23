// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// SectionStatus DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionStatusDtoProperty 
    {
        /// <summary>
        /// Section Status 
        /// </summary>
        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public SectionStatus2? Category { get; set; }

        /// <summary>
        /// Section Status detail
        /// </summary>
        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Detail { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public SectionStatusDtoProperty()
        {
        }
    }
}