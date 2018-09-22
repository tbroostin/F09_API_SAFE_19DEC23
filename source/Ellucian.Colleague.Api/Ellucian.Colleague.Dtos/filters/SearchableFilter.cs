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
    public class SearchableFilter
    {
        /// <summary>
        /// KeywordSearch
        /// </summary>        
        [DataMember(Name = "searchable", EmitDefaultValue = false)]
        [FilterProperty("searchable")]
        public SectionsSearchable Search { get; set; }
    }
}
