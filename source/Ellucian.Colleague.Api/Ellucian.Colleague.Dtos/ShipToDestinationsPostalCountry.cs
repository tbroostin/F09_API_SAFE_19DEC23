// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Dtos.EnumProperties
{   
   /// <summary>
    /// The name of the country when mail is being addressed from an international sender.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShipToDestinationsPostalCountry
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
       
                           
         /// <summary>
        /// AUSTRALIA
        /// </summary>
        [EnumMember(Value = "AUSTRALIA")]
        AUSTRALIA,
        }
} 


