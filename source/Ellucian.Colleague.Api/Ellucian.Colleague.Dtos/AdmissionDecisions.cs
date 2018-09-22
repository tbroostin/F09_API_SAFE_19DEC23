//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Decisions made on admission applications. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AdmissionDecisions : BaseModel2
    {    
        /// <summary>
       /// The admission application on which this decision was made.
       /// </summary>
         
       [JsonProperty("application")]
       [FilterProperty("criteria")]
       public GuidObject2 Application { get; set; }
     
       /// <summary>
       /// The type of decision on the admission application.
       /// </summary>
         
       [JsonProperty("decisionType")]
       public GuidObject2 DecisionType { get; set; }
     
        /// <summary>
       /// The date of the decision on the admission application.
       /// </summary>
       //[JsonConverter(typeof(DateOnlyConverter))]  
       [JsonProperty("decidedOn")]
       public DateTime DecidedOn { get; set; }
         
     }      
}          
