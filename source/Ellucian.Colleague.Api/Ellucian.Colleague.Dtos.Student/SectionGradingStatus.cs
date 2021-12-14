// Copyright 2021 Ellucian Company L.P. and its affiliates
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Course section grading status
    /// </summary>
    public class SectionGradingStatus
    {
        /// <summary>
        /// The date on which the last final grade was posted for the course section
        /// </summary>
        public DateTime? FinalGradesPostedDate { get; set; }

        /// <summary>
        /// The time of day at which the last final grade was posted for the course section
        /// </summary>
        public DateTimeOffset? FinalGradesPostedTime { get; set; }

        /// <summary>
        /// Unique identifier for the person who posted the last final grade for the course section
        /// </summary>
        public string GradesPostedByPersonId { get; set; }

        /// <summary>
        /// Unique identifier for the course section
        /// </summary>
        public string CourseSectionId { get; set; }
    }
}
