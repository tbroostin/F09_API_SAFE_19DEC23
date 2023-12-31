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
    /// The valid list of asset types for depreciation purposes. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class FixedAssetType : CodeItem2
    {    
        /// <summary>
       /// The default method of depreciation associated with the fixed asset type.
       /// </summary>
         
       [JsonProperty("depreciationMethod", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
       public string DepreciationMethod { get; set; }
     
        /// <summary>
       /// The default percentage salvage value based on acquisition cost for the fixed asset type.
       /// </summary>
         
       [JsonProperty("salvageValue", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
       public decimal? SalvageValue { get; set; }
     
        /// <summary>
       /// The default useful life in years for the fixed asset type.
       /// </summary>
         
       [JsonProperty("usefulLife", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
       public int? UsefulLife { get; set; }
         
     }      
}          
