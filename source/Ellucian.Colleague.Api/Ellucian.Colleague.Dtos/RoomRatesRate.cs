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
    public class RoomRatesRate 
    {    
       /// <summary>
       /// The rate charged for the room for the period specified.
       /// </summary>
          
       [JsonProperty("rate", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public RoomRatesRateDtoProperty RateProperty { get; set; }

       /// <summary>
       /// The The interval for which rate is defined
       /// </summary>

       [JsonProperty("period", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public RoomRatesPeriod Period { get; set; }
     
     }      
}  

