// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This object defines display options for filters shown in the course catalog. 
    /// </summary>
    public class CatalogFilterOption3
    {
        /// <summary>
        /// Indicates the type of catalog filter 
        /// </summary>
        public CatalogFilterType3 Type { get; set; }
        /// <summary>
        /// Indicates whether this item should be hidden or not.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
