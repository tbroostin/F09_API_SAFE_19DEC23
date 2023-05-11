// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Course section availability information configuration
    /// </summary>
    public class SectionAvailabilityInformationConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not to show negative seat counts for course sections
        /// </summary>
        public bool ShowNegativeSeatCounts { get; set; }

        /// <summary>
        /// Flag indicating whether or not to include seats taken in course section availability information
        /// </summary>
        public bool IncludeSeatsTakenInAvailabilityInformation { get; set; }
    }
}
