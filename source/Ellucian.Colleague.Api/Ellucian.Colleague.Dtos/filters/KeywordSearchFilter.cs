// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Keyword Search named query
    /// </summary>
    //[JsonConverter(typeof(UserConverter))]
    public class KeywordSearchFilter
    {
        /// <summary>
        /// KeywordSearch
        /// </summary>        
        [DataMember(Name = "keywordSearch", EmitDefaultValue = false)]
        [FilterProperty("keywordSearch")]
        public string Search { get; set; }
    }
}
