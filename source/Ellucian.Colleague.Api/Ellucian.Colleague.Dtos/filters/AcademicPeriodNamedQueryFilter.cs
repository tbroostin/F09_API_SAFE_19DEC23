// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// academicPeriod named query
    /// </summary>
    public class AcademicPeriodNamedQueryFilter
    {
        /// <summary>
        /// fiscalYear
        /// </summary>        
        [JsonProperty("academicPeriod")]
        [FilterProperty("academicPeriod")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 AcademicPeriod { get; set; }
    }
}
