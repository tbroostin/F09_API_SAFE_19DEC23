// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Student named query
    /// </summary>
    [DataContract]
    public class StudentFilter
    {
        /// <summary>
        /// Student filter
        /// </summary>        
        //[JsonProperty("section")]
        [DataMember(Name = "student", EmitDefaultValue = false)]
        [FilterProperty("student")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Student { get; set; }
    }
}
