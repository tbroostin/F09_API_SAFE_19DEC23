// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how catalog searches should be tailored for a user in self-service.
    /// </summary>
    public class CourseCatalogConfiguration2
    {
        /// <summary>
        /// If provided, this limits catalog searches by date to dates no earlier than this.
        /// </summary>
        public DateTime? EarliestSearchDate { get; set; }

        /// <summary>
        ///If provided, this limits catalog searches by date to dates no later than this.
        /// </summary>
        public DateTime? LatestSearchDate { get; set; }

        /// <summary>
        /// The display options for the course catalog filters and advanced search criteria.  If this list is
        /// blank or missing assume all filters and criteria should be shown.
        /// </summary>
        public List<CatalogFilterOption2> CatalogFilterOptions { get; set; }

        /// <summary>
        /// Indicates whether or not course section fee information should be visible in the course catalog
        /// </summary>
        public bool ShowCourseSectionFeeInformation { get; set; }

        /// <summary>
        /// Indicates whether or not course section book information should be visible in the course catalog's section detail.
        /// </summary>
        public bool ShowCourseSectionBookInformation { get; set; }
    }
}
