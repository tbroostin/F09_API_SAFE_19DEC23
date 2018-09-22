﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// AcademicPeriod DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GradeSchemeDtoProperty : BaseCodeTitleDetailDtoProperty
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
        /// End date in UTC
        /// </summary>
        [JsonProperty("academicLevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AcademicLevelDtoProperty AcademicLevel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public GradeSchemeDtoProperty() : base()
        {
            
        }
    }
}