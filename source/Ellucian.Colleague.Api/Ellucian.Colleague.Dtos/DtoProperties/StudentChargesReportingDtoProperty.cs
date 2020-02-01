// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The employement status type and detail
    /// </summary>
    [DataContract]
    public class StudentChargesReportingDtoProperty
    {

        /// <summary>
        /// The usage associated with the charge (i.e. tax reporting only).
        /// </summary>
        [JsonProperty("usage", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public EnumProperties.StudentChargeUsageTypes? Usage { get; set; }

        /// <summary>
        /// The date the charge originated for sonsideration in tax report generation.
        /// </summary>
        [JsonProperty("originatedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? OriginatedOn { get; set; }
    }
}