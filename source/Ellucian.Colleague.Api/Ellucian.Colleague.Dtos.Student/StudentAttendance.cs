// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains attendance information for a specific student, section and meeting
    /// </summary>
    public class StudentAttendance
    {
        /// <summary>
        /// Id of the student (Required)
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Section Id (Required)
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Meeting Date (Required)
        /// </summary>
        public DateTime MeetingDate { get; set; }

        /// <summary>
        /// Indicates student's actual attendance type (Required unless MinutesAttended is provided) 
        /// </summary>
        public string AttendanceCategoryCode { get; set; }

        /// <summary>
        /// Time of Day that the section meeting instance begins.
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }
        /// <summary>
        /// Time of Day that the section meeting instance ends.
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }
        /// <summary>
        /// Instructional Method associated to this meeting 
        /// </summary>
        public string InstructionalMethod { get; set; }

        /// <summary>
        /// Reason for the attendance category given 
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Id of the student course section 
        /// </summary>
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// The number of minutes that the student attended this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both. (Required unless AttendanceCategoryCode is provided)
        /// </summary>
        public int? MinutesAttended { get; set; }

        /// <summary>
        /// The number of minutes that the student attended up to and including this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? MinutesAttendedToDate { get; set; }

        /// <summary>
        /// The number of minutes that the student attended for the section for the duration of the course section, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? CumulativeMinutesAttended { get; set; }

    }
}
