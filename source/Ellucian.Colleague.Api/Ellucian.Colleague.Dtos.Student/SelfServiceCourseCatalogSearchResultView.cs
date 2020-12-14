// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Colleague Self-Service Course Catalog Search Result Views
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SelfServiceCourseCatalogSearchResultView
    {
        /// <summary>
        /// Course catalog search result view
        /// </summary>
        CatalogListing,
        /// <summary>
        /// Section catalog search result view
        /// </summary>
        SectionListing
    }
}

