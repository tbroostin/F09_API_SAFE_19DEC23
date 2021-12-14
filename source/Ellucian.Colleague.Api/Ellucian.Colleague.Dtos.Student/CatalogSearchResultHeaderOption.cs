// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This object defines display options for course catalog search result headers.
    /// </summary>
    public class CatalogSearchResultHeaderOption
    {
        /// <summary>
        /// Indicates the type of catalog search result header.
        /// </summary>
        public CatalogSearchResultHeaderType Type { get; set; }
        /// <summary>
        /// Indicates whether this item should be hidden or not.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
