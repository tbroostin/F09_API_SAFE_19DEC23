﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The methodology for which the application is applicable.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StudentFinancialAidNeedMethodology
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,


        /// <summary>
        /// institutional
        /// </summary>
        [EnumMember(Value = "institutional")]
        Institutional,

        /// <summary>
        /// federal
        /// </summary>
        [EnumMember(Value = "federal")]
        Federal,

    }
}
