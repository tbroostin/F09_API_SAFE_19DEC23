// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Student Unverified Grades filters
    /// </summary>
    [DataContract]
    public class StudentUnverifiedGradesFilter
    {
        /// <summary>
        /// The section associated with the grade.
        /// </summary>        
        //[JsonProperty("section")]
        [DataMember(Name = "section", EmitDefaultValue = false)]
        [FilterProperty("section")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Section { get; set; }
    }
}
