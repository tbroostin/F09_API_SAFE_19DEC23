//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The list of valid vendor address usages. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class VendorAddressUsages : BaseModel2
    {    
        /// <summary>
       /// The full name of the vendor address usage.
       /// </summary>
         
       [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public string Title { get; set; }
     
        /// <summary>
       /// The description of the vendor address usage.
       /// </summary>
         
       [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public string Description { get; set; }
         
     }      
}          
