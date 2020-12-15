// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Enumeration of possible types of an health check
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HealthCheckStatusType
    {
        /// <summary>
        /// Available
        /// </summary>
        [EnumMember(Value = "available")]
        Available,

        /// <summary>
        /// Unavailable
        /// </summary>
        [EnumMember(Value = "unavailable")]
        Unavailable
    }
}