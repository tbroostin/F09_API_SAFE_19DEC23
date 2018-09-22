// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains attendance information used to update a specific student in a specific section
    /// </summary>
    public class StudentSectionAttendance
    {
        /// <summary>
        /// Id of the student course section (Unique for a student in a specific section) (Required)
        /// </summary>
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// Code indicating student's attendance - present, absent, etc. (Either a category code, minutes or a comment must be provided.) 
        /// </summary>
        public string AttendanceCategoryCode { get; set; }

        /// <summary>
        /// Reason for the attendance category given. (Either a category code or a comment must be provided.)
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The number of minutes that the student attended this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both. (Required unless AttendanceCategoryCode is provided)
        /// </summary>
        public int? MinutesAttended { get; set; }
    }
}
