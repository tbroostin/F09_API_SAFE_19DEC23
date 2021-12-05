// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Defines the sort order for the catalog sections
    /// </summary>
    [Serializable]
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
