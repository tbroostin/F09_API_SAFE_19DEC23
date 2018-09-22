// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The adjustment type for the encumbrance.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AdjustmentType
    {
        /// <summary>
        /// partial
        /// </summary>
        [EnumMember(Value = "partial")]
        Partial,

        /// <summary>
        /// total
        /// </summary>
        [EnumMember(Value = "total")]
        Total,

        /// <summary>
        /// adjustment
        /// </summary>
        [EnumMember(Value = "adjustment")]
        Adjustment,
    }
}
