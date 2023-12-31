﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The status of the line item (open, closed).
    /// </summary>
    [JsonConverter(typeof (StringEnumConverter))]
    public enum AccountsPayableInvoicesStatus
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// open
        /// </summary>
        [EnumMember(Value = "open")] 
        Open,

        /// <summary>
        /// closed
        /// </summary>
        [EnumMember(Value = "closed")] 
        Closed
    }
}