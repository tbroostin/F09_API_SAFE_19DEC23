// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Course section availability information configuration
    /// </summary>
    [Serializable]
    public class SectionAvailabilityInformationConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not to show negative seat counts for course sections
        /// </summary>
        public bool ShowNegativeSeatCounts { get; private set; }

        /// <summary>
        /// Flag indicating whether or not to include seats taken in course section availability information
        /// </summary>
        public bool IncludeSeatsTakenInAvailabilityInformation { get; private set; }

        /// <summary>
        /// Creates a new <see cref="SectionAvailabilityInformationConfiguration"/> object
        /// </summary>
        /// <param name="showNegativeSeatCounts"></param>
        /// <param name="includeSeatsTakenInAvailabilityInformation"></param>
        public SectionAvailabilityInformationConfiguration(bool showNegativeSeatCounts, 
            bool includeSeatsTakenInAvailabilityInformation)
        {
            ShowNegativeSeatCounts = showNegativeSeatCounts;
            IncludeSeatsTakenInAvailabilityInformation = includeSeatsTakenInAvailabilityInformation;
        }
    }
}
