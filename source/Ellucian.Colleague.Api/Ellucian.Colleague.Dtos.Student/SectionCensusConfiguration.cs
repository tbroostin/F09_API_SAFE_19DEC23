// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// census configuration parameters
    /// </summary>
    public class SectionCensusConfiguration
    {
        /// <summary>
        /// a collection of section census date parameters
        /// </summary>
        public IEnumerable<CensusDatePositionSubmission> CensusDatePositionSubmissionRange { get; set; }

        /// <summary>
        /// Determines if and/or how the LDA/NA field will be displayed for a section census
        /// </summary>
        public LastDateAttendedNeverAttendedFieldDisplayType LastDateAttendedNeverAttendedCensusRoster { get; set; }

        /// <summary>
        /// The drop reason code that will be used whtn a faculty member drops a student from a section in the Drop Roster.
        /// </summary>
        public string FacultyDropReasonCode { get; set; }
    }
}
