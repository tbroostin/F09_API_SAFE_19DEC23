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
    public class InstructionalEventInstancesFilter
    {
        /// <summary>
        /// fiscalYear
        /// </summary>        
        [JsonProperty("instructionalEventInstances")]
        [FilterProperty("instructionalEventInstances")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 InstructionalEventInstances { get; set; }
    }
}
