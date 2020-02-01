// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information for one midterm grading complete indication for a section to be added.
    /// </summary>
    public class SectionMidtermGradingCompleteForPost
    {
        /// <summary>
        /// A number 1 to 6 indicating which of the six midterm grades the complete indication applies to
        /// </summary>
        public int? MidtermGradeNumber { get; set; }

        /// <summary>
        /// Person ID of the faculty member that indicated that midterm grading is complete
        /// </summary>
        public string CompleteOperator { get; set; }

        /// <summary>
        /// Date and time at which the faculty member indicated that midterm grading is complete
        /// </summary>
        public DateTimeOffset? DateAndTime { get; set; }

    }
}
