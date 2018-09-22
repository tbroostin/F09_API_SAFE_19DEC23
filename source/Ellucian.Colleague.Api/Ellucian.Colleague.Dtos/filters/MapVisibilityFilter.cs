// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// MapVisibility named query
    /// </summary>
    public class MapVisibilityFilter
    {
        /// <summary>
        /// BuildingMapVisibility
        /// </summary>        
        [DataMember(Name = "mapVisibility", EmitDefaultValue = false)]
        [FilterProperty("mapVisibility")]
        public BuildingMapVisibility Visibility { get; set; }
    }
}
