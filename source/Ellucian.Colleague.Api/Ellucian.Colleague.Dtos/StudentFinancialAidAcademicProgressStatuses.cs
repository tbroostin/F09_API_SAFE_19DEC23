//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The financial aid satisfactory academic progress status of the person. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentFinancialAidAcademicProgressStatuses : BaseModel2
    {
        /// <summary>
        /// The person associated with the financial aid satisfactory academic progress status.
        /// </summary>
        [JsonProperty("person")]
        [FilterProperty("criteria")]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// The financial aid satisfactory academic progress status of the person.
        /// </summary>
        [JsonProperty("status")]
        [FilterProperty("criteria")]
        public GuidObject2 Status { get; set; }

        /// <summary>
        /// The date the financial aid satisfactory academic progress status is effective.
        /// </summary>
        [JsonProperty("effectiveOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(DateOnlyConverter))]
        public DateTime? EffectiveOn { get; set; }

        /// <summary>
        /// The date the financial aid satisfactory academic progress status is effective.
        /// </summary>
        [JsonProperty("assignedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(DateOnlyConverter))]
        public DateTime? AssignedOn { get; set; }

        /// <summary>
        /// The categorization of the financial aid satisfactory academic progress (SAP) status.
        /// </summary>
        [JsonProperty("type")]
        [FilterProperty("criteria")]
        public GuidObject2 ProgressType { get; set; }
    }
}
