// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{   
   /// <summary>
    /// Requiredness of the supplementary item.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AdmissionApplicationSupportingItemsRequired
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
                
        /// <summary>
        /// mandatory
        /// </summary>
        [EnumMember(Value = "mandatory")]
        Mandatory,
     }
} 


