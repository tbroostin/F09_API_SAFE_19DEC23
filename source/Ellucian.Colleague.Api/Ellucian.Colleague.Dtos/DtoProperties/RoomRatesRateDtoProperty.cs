//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The rate charged for the room for the period specified. 
    /// </summary>
   [JsonObject(MemberSerialization.OptIn)]
    public class RoomRatesRateDtoProperty
    {    
        /// <summary>
       /// The monetary value
       /// </summary>
          
       [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public Decimal? Value { get; set; }
     
        /// <summary>
       /// The ISO 4217 currency code
       /// </summary>
          
       [JsonProperty("currency", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public RoomRatesCurrency Currency { get; set; }
 

     }      
}  

