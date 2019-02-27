// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Enumeration of possible sort fields
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WebSortField
    {
        /// <summary>
        /// Description sort field
        /// </summary>
        Description,

        /// <summary>
        /// Status sort field
        /// </summary>
        Status,

        /// <summary>
        /// Status Date sort field
        /// </summary>
        StatusDate,

        /// <summary>
        /// Due Date sort field
        /// </summary>
        DueDate,

        /// <summary>
        /// Office Description sort field
        /// </summary>
        OfficeDescription
    }
}
