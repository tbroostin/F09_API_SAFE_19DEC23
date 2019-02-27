//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The most recent student classification for their academic levels.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentLevelClassificationsDtoProperty
    {
        /// <summary>
        /// The academic level for a student
        /// </summary>
        [JsonProperty("level", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Level { get; set; }

        /// <summary>
        /// The most recent classification for a student's academic level.
        /// </summary>
        [JsonProperty("latestClassification", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 LatestClassification { get; set; }
    }
}