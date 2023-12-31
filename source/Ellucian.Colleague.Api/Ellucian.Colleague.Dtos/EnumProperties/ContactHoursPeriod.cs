﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Enumeration of possible time periods for which contact hours are considered.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactHoursPeriod
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        /// 
        NotSet = 0,

        /// <summary>
        /// Day
        /// </summary>
        [EnumMember(Value = "day")]
        Day,

        /// <summary>
        /// Week
        /// </summary>
        [EnumMember(Value = "week")]
        Week,

        /// <summary>
        /// Month
        /// </summary>
        [EnumMember(Value = "month")]
        Month,

        /// <summary>
        /// Pay Period
        /// </summary>
        [EnumMember(Value = "term")]
        Term,
    }
}