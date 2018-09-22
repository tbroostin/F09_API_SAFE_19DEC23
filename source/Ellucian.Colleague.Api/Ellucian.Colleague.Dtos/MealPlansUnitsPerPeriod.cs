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
    /// Meal plan units per period 
    /// </summary>
   [JsonObject(MemberSerialization.OptIn)]
    public class MealPlansUnitsPerPeriod 
    {        
       /// <summary>
       /// The number of units allowed
       /// </summary>  
       [JsonProperty("numberOfUnits", DefaultValueHandling = DefaultValueHandling.Ignore)]  
       public Decimal NumberOfUnits { get; set; }
     
       /// <summary>
       /// The time period for the restriction
       /// </summary>
       [JsonProperty("period", DefaultValueHandling = DefaultValueHandling.Ignore)]  
       public MealPlansTimePeriod TimePeriod { get; set; }
     
     }      
}  

