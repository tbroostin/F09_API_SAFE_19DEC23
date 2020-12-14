// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Colleague Self-Service Course Catalog Search Result Views
    /// </summary>
    [Serializable]
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

