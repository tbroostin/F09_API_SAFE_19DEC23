//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The applicant's desired compensation.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DesiredCompensationProperty
    {
        /// <summary>
        /// The time period of the salary.
        /// </summary>

        [JsonProperty("period")]
        public SalaryPeriod Period { get; set; }

        /// <summary>
        /// The ISO 4217 currency code.
        /// </summary>
        [JsonProperty("rate")]
        public JobApplicationsRateDtoProperty Rate { get; set; }
    }
}
