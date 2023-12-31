// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The indication if the section is CrossListed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CrossListed
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// crossListed
        /// </summary>
        [EnumMember(Value = "crossListed")]
        CrossListed,

        /// <summary>
        /// notCrossListed
        /// </summary>
        [EnumMember(Value = "notCrossListed")]
        NotCrossListed
    }
}