﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Enumeration of the Cohort Type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CohortType
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Federal.
        /// </summary>
        [EnumMember(Value = "federal")]
        Federal,
    }
}
