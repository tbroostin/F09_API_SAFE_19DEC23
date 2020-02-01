// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This object defines display options for filters shown in the course catalog. 
    /// </summary>
    public class CatalogFilterOption2
    {
        /// <summary>
        /// Indicates the type of catalog filter 
        /// </summary>
        public CatalogFilterType2 Type { get; set; }
        /// <summary>
        /// Indicates whether this item should be hidden or not.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
