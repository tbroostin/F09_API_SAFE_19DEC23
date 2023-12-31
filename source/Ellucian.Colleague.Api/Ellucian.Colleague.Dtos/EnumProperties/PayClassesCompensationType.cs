// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{   
   /// <summary>
    /// The compensation type associated with the pay class (e.g. salary or wages).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayClassesCompensationType
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
       
                           
         /// <summary>
        /// salary
        /// </summary>
        [EnumMember(Value = "salary")]
        Salary,
                     
         /// <summary>
        /// wages
        /// </summary>
        [EnumMember(Value = "wages")]
        Wages,
        }
} 


