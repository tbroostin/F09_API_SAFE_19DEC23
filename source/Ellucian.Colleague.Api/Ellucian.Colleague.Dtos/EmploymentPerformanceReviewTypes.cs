//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined ratings that may be used on employment performance reviews. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmploymentPerformanceReviewTypes : CodeItem2
    {    
       /// <summary>
       /// The frequency at which the performance review occurs.
       /// </summary>
       [JsonProperty("frequency", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public FrequencyDtoProperty Frequency { get; set; }
         
     }      
}          
