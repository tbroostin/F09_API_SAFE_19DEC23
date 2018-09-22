// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Searchable named query
    /// </summary>
    public class RestrictedVisibilityFilter
    {
        /// <summary>
        /// KeywordSearch
        /// </summary>        
        [DataMember(Name = "restrictedVisibility", EmitDefaultValue = false)]
        [FilterProperty("restrictedVisibility")]
        public RestrictedVisibility RestrictedVisibility { get; set; }
    }
}
