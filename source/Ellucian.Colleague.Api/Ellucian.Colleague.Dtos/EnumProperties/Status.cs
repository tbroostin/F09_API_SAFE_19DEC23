// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{   
   /// <summary>
    /// The status (e.g. active or inactive).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
                                  
         /// <summary>
        /// active
        /// </summary>
        [EnumMember(Value = "active")]
        Active,
                     
         /// <summary>
        /// inactive
        /// </summary>
        [EnumMember(Value = "inactive")]
        Inactive,
    }
} 