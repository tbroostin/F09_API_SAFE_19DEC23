//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The amount associated with the activity. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class LedgerActivityAmount
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
       public LedgerActivitiesCurrency Currency { get; set; }
         

 

     }      
}  

