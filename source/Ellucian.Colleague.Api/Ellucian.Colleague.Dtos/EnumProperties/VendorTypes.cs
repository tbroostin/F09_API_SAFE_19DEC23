// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// The type of vendor.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VendorTypes
    {
        /// <summary>
        /// procurement
        /// </summary>
        [EnumMember(Value = "procurement")]
        Procurement,

        /// <summary>
        /// eProcurement
        /// </summary>
        [EnumMember(Value = "eProcurement")]
        EProcurement,

        /// <summary>
        /// travel
        /// </summary>
        [EnumMember(Value = "travel")]
        Travel
    }
}
