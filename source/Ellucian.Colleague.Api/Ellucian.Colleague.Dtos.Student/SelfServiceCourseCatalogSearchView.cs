// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Colleague Self-Service Course Catalog Search Views
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SelfServiceCourseCatalogSearchView
    {
        /// <summary>
        /// Course subject search view
        /// </summary>
        SubjectSearch,
        /// <summary>
        /// Advanced course catalog search view
        /// </summary>
        AdvancedSearch
    }
}
