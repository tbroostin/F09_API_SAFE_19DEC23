// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance for a specific class in a specific class meeting of a section
    /// </summary>
    [Serializable]
    public class StudentAttendance
    {
        private string _StudentId;
        /// <summary>
        /// Id of the student this attendance is associated with
        /// </summary>
        public string StudentId { get { return _StudentId; } }

        private string _SectionId;
        /// <summary>
        /// Id of the section for this meeting instance (Required)
        /// </summary>
        public string SectionId { get { return _SectionId; } }

        private DateTime _MeetingDate;
        /// <summary>
        /// Date of the meeting instance (Required)
        /// </summary>
        public DateTime MeetingDate { get { return _MeetingDate; } }


        private string _AttendanceCategoryCode;
        /// <summary>
        /// Indicates student's presence or absence for the specific section meeting instance.
        /// </summary>
        public string AttendanceCategoryCode { get { return _AttendanceCategoryCode; } }

        #region OptionalProperties

        /// <summary>
        /// Time of day when this meeting instance begins
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Time of day when this calendar event ends
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Instructional Method code associated to this meeting instance if applicable.
        /// </summary>
        public string InstructionalMethod { get; set; }

        private string _Comment;
        /// <summary>
        /// Comment or reason related to the attendence category given.
        /// </summary>
        public string Comment { get { return _Comment; } }

        /// <summary>
        /// Id of the student course section record this attendance is associated with
        /// </summary>
        public string StudentCourseSectionId { get; set; }

        private int? _MinutesAttended;
        /// <summary>
        /// The number of minutes that the student attended this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? MinutesAttended { get { return _MinutesAttended; } }

        private int? _MinutesAttendedToDate;
        /// <summary>
        /// The number of minutes that the student attended up to and including this attendance, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? MinutesAttendedToDate
        {
            get
            {
                return _MinutesAttendedToDate;
            }
            set
            {
                if (MinutesAttended.HasValue && value.HasValue && MinutesAttended.Value > value.Value)
                {
                    throw new ApplicationException(string.Format("Minutes attended to date ({0}) cannot be less than Minutes attended for this date ({1}).", value.ToString(), MinutesAttended.Value.ToString()));
                }
                if (CumulativeMinutesAttended.HasValue && value.HasValue && CumulativeMinutesAttended.Value < value.Value)
                {
                    throw new ApplicationException(string.Format("Cumulative minutes attended ({0}) cannot be less than Minutes attended to date ({1}).", CumulativeMinutesAttended.Value.ToString(), value.ToString()));
                }

                _MinutesAttendedToDate = value;
            }
        }

        private int? _CumulativeMinutesAttended;
        /// <summary>
        /// The number of minutes that the student attended for the duration of the course section, when applicable;
        /// Attendance can be recorded with presence and absence or in minutes, but not both.
        /// </summary>
        public int? CumulativeMinutesAttended
        {
            get
            {
                return _CumulativeMinutesAttended;
            }
            set
            {
                if (MinutesAttended.HasValue && value.HasValue && MinutesAttended.Value > value.Value)
                {
                    throw new ApplicationException(string.Format("Cumulative minutes attended ({0}) cannot be less than Minutes attended for this date ({1}).", value.ToString(), MinutesAttended.Value.ToString()));
                }
                if (MinutesAttendedToDate.HasValue && value.HasValue && MinutesAttendedToDate.Value > value.Value)
                {
                    throw new ApplicationException(string.Format("Cumulative minutes attended ({0}) cannot be less than Minutes attended to date ({1}).", value.ToString(), MinutesAttendedToDate.Value.ToString()));
                }

                _CumulativeMinutesAttended = value;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new <see cref="StudentAttendance"/> object.
        /// </summary>
        /// <param name="studentId">Id of the student this attendance is associated with</param>
        /// <param name="sectionId">Id of the section this attendance is associated with</param>
        /// <param name="meetingDate">Date of the meeting instance</param>
        /// <param name="attendanceCategoryCode">Indicates student's presence or absence for the specific section meeting instance</param>
        /// <param name="minutesAttended">The number of minutes that the student attended this attendance</param>
        /// <param name="comment">Comment or reason related to the attendence category given</param>
        public StudentAttendance(string studentId, string sectionId, DateTime meetingDate, string attendanceCategoryCode, int? minutesAttended, string comment = null)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id must be provided.");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section Id must be provided.");
            }
            if (meetingDate == DateTime.MinValue)
            {
                throw new ArgumentException("meetingDate", "Meeting Date must be provided and not be min value.");
            }
            if (!string.IsNullOrEmpty(attendanceCategoryCode) && minutesAttended.HasValue)
            {
                throw new ArgumentException("A student's attendance can be recorded with presence/absence, or in minutes attended, but not both.");
            }
            if (minutesAttended.HasValue && minutesAttended.Value < 0)
            {
                throw new ArgumentOutOfRangeException("minutesAttended", "When recording attendance in minutes attended, the number of minutes cannot be less than zero.");
            }
            _StudentId = studentId;
            _SectionId = sectionId;
            _MeetingDate = meetingDate;
            _MinutesAttended = minutesAttended;
            _AttendanceCategoryCode = attendanceCategoryCode;
            _Comment = comment;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            // No id so only a match if all values same BUT null/empty for the instructional method can also be considered match.
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            if (obj is StudentAttendance)
            {
                StudentAttendance studentAttendance = obj as StudentAttendance;
                if (studentAttendance == null)
                    return false;
                var tempInstructionalMethod = this.InstructionalMethod ?? string.Empty;
                if (this.StudentId == studentAttendance.StudentId && 
                    this.SectionId == studentAttendance.SectionId && 
                    this.StartTime == studentAttendance.StartTime && 
                    this.EndTime == studentAttendance.EndTime && 
                    this.MeetingDate == studentAttendance.MeetingDate &&
                    this.MinutesAttended == studentAttendance.MinutesAttended &&
                    ((string.IsNullOrEmpty(this.AttendanceCategoryCode) && string.IsNullOrEmpty(studentAttendance.AttendanceCategoryCode)) || 
                    (this.AttendanceCategoryCode != null && this.AttendanceCategoryCode.Equals(studentAttendance.AttendanceCategoryCode, StringComparison.InvariantCultureIgnoreCase))) &&
                    tempInstructionalMethod.Equals(studentAttendance.InstructionalMethod??string.Empty, StringComparison.InvariantCultureIgnoreCase))

                    return true;
                else
                    return false;
            }
            return false;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
