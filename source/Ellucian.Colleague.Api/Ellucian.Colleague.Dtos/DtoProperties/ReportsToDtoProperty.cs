// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The positions to which this position reports.
    /// </summary>
    [DataContract]
    public class ReportsToDtoProperty
    {
        /// <summary>
         /// The position to which this position reports.
        /// </summary>
        [JsonProperty("position", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Postition { get; set; }

        /// <summary>
        ///The type of reporting position (e.g. primary, alternate, etc.).
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositionReportsToType? Type { get; set; }
    }
}