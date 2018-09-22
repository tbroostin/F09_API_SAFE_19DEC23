// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Student course information for display in the Student Statement
    /// </summary>
    public class StudentStatementScheduleItem
    {
        /// <summary>
        /// ID of the course section, i.e. "MATH-100-01"
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Title of the course section
        /// </summary>
        public string SectionTitle { get; set; }

        /// <summary>
        /// Term description for the course section
        /// </summary>
        public string SectionTerm { get; set; }

        /// <summary>
        /// Number of credits for the course section
        /// </summary>
        public decimal Credits { get; set; }

        /// <summary>
        /// Number of continuing education units for the course section
        /// </summary>
        public decimal ContinuingEducationUnits { get; set; }

        /// <summary>
        /// Meeting days for the course section
        /// </summary>
        public string MeetingDays { get; set; }

        /// <summary>
        /// Meeting times for the course section
        /// </summary>
        public string MeetingTimes { get; set; }

        /// <summary>
        /// Meeting locations for the course section
        /// </summary>
        public string MeetingLocations { get; set; }

        /// <summary>
        /// Course section start and end dates
        /// </summary>
        public string SectionDates { get; set; }
    }
}
