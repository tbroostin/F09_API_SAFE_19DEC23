﻿// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// AcademicPeriod DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AcademicPeriodDtoProperty3 : BaseCodeTitleDetailDtoProperty
    {
        /// <summary>
        /// Start date in UTC
        /// </summary>
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? Start { get; set; }


        /// <summary>
        /// End date in UTC
        /// </summary>
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// cenus date list
        /// </summary>
        [JsonProperty("censusDates", ItemConverterType = typeof(DateOnlyConverter), DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<DateTime?> CensusDates { get; set; }

        /// <summary>
        /// The <see cref="AcademicPeriodCategory">Academic Period category</see>
        /// </summary>
        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AcademicPeriodCategory3 Category { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public AcademicPeriodDtoProperty3() : base() { }
    }
}

