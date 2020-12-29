// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the sort order for the catalog sections
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CatalogSortDirection
    {
         /// <summary>
        /// Ascending order- it is also default order
        /// </summary>
        Ascending,
       /// <summary>
       /// Descending order
       /// </summary>
        Descending
        
        
    }
}
