// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// Email Preference
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PersonEmailPreference
    {
        /// <summary>
        /// Primary within this email type.
        /// </summary>
        [EnumMember(Value = "primary")]
        Primary
        
    }
}
