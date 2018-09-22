﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Detailed information concerning financial aid applicant income. 
    /// </summary>
    [DataContract]
    public class FinancialAidApplicationApplicant
    {
        /// <summary>
        /// The person associated with the financial aid application.
        /// </summary>
        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Person { get; set; }

        /// <summary>
        /// Financial aid applicant not known to the student information system.
        /// </summary>
        [JsonProperty("financialAidApplicant", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 FinancialAidApplicant { get; set; }
    }
}
