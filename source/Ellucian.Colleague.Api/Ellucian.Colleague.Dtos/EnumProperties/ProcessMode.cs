﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Enumeration to specify process mode in a General Ledger Transaciton
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessMode
    {
        /// <summary>
        /// current
        /// </summary>
        [EnumMember(Value = "update")]
        Update,

        /// <summary>
        /// expired
        /// </summary>
        [EnumMember(Value = "validate")]
        Validate
    }
}
