// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Academic Programs named query
    /// </summary>
    public class AcademicProgramsFilter
    {
        /// <summary>
        /// Filter to return student academic credentials by academic program.
        /// </summary>        
        [JsonProperty("academicPrograms")]
        [FilterProperty("academicPrograms")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 AcademicPrograms { get; set; }        
    }
}