// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Enumeration of possible instructor name types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstructorNameType2
    {
        /// <summary>
        /// Personal
        /// </summary>
        [EnumMember(Value = "personal")]
        Personal,

        /// <summary>
        /// Birth
        /// </summary>
        [EnumMember(Value = "birth")]
        Birth,

        /// <summary>
        /// Legal
        /// </summary>
        [EnumMember(Value = "legal")]
        Legal
    }
}