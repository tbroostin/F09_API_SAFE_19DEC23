// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance for a specific class in a specific class meeting of a section
    /// </summary>
    [Serializable]
    public class StudentSectionAttendance
    {
        private string _StudentCourseSectionId;
        /// <summary>
        /// Id of the student course section record this attendance is associated with
        /// </summary>
        public string StudentCourseSectionId { get { return _StudentCourseSectionId; } }

        // Indicates student's presence or absence for the specific section meeting instance. 
        private string _AttendanceCategoryCode;
        /// <summary>
        /// Indicates student's presence or absence for the specific section meeting instance.
        /// </summary>
        public string AttendanceCategoryCode { get { return _AttendanceCategoryCode; } }

        private string _Comment;
        /// <summary>
        /// Comment or reason related to the attendence category given.
        /// </summary>
        public string Comment { get { return _Comment; } }

        private int? _MinutesAttended;
        /// <summary>
        /// The number of minutes that the student attended this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? MinutesAttended { get { return _MinutesAttended; } }

        /// <summary>
        /// Constructor to make a new StudentSectionAttendance entity.
        /// </summary>
        /// <param name="studentCourseSectionId">Student Course Sec Id being updated (student/section)</param>
        /// <param name="attendanceCategoryCode">Attendance type - present, absent, etc.</param>
        /// <param name="comment">Reason for attendance value.</param>
        public StudentSectionAttendance(string studentCourseSectionId, string attendanceCategoryCode, int? minutesAttended, string comment)
        {
            if (string.IsNullOrEmpty(studentCourseSectionId))
            {
                throw new ArgumentNullException("attendanceCategoryCode", "An attendance category code is required for a student section attendance");
            }
            if (!string.IsNullOrEmpty(attendanceCategoryCode) && minutesAttended.HasValue)
            {
                throw new ArgumentException("A student's attendance can have category or minutes attended but not both.");
            }
            _StudentCourseSectionId = studentCourseSectionId;
            _AttendanceCategoryCode = attendanceCategoryCode;
            _MinutesAttended = minutesAttended;
            _Comment = comment;
        }
        
    }
}
