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
    /// MetaData for HEDM objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MetaDataDtoProperty
    {
        /// <summary>
        /// Hedm objects created on date
        /// </summary>
        [JsonProperty("createdOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Title of section
        /// </summary>
        [JsonProperty("createdBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Hedm objects modified on date
        /// </summary>
        [JsonProperty("modifiedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Title of section
        /// </summary>
        [JsonProperty("modifiedBy", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ModifiedBy { get; set; }
    }
}
