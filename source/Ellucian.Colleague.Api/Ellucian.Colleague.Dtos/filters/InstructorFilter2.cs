// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor URL criteria, supporting both styles of json input
    /// </summary>
    public class InstructorFilter2
    {
        /// <summary>
        /// instructor
        /// </summary>        
        [DataMember(Name = "instructor", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Instructor { get; set; }


        /// <summary>
        /// The location where the instructor typically performs his/her duties.
        /// </summary>

        [DataMember(Name = "primaryLocation", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 PrimaryLocation { get; set; }
    }
}
