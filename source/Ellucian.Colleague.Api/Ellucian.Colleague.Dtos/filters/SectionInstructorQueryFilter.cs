// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Section instructor named query.
    /// </summary>
    public class SectionInstructorQueryFilter
    {
        /// <summary>
        /// section instructor.
        /// </summary>        
        [DataMember(Name = "sectionInstructor", EmitDefaultValue = false)]
        [FilterProperty("sectionInstructor")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 SectionInstructorId { get; set; }
    }
}