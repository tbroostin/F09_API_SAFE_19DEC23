// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor named query
    /// </summary>
    public class InstructorFilter
    {
        /// <summary>
        /// instructor
        /// </summary>        
        [DataMember(Name = "instructor", EmitDefaultValue = false)]
        [FilterProperty("instructor")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 InstructorId { get; set; }
    }
}
