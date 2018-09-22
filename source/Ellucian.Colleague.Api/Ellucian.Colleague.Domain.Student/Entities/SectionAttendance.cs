// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance information for a specific class at a specific class meeting 
    /// </summary>
    [Serializable]
    public class SectionAttendance
    {
        private string _SectionId;
        /// <summary>
        /// Id of the section (Required)
        /// </summary>
        public string SectionId { get { return _SectionId; } }

        private SectionMeetingInstance _MeetingInstance;
        /// <summary>
        /// The meeting instance (Required)
        /// </summary>
        public SectionMeetingInstance MeetingInstance { get { return _MeetingInstance; } }

        private readonly List<StudentSectionAttendance> _StudentAttendances = new List<StudentSectionAttendance>();
        /// <summary>
        /// List of student attendances to be updated for this section
        /// </summary>
        public ReadOnlyCollection<StudentSectionAttendance> StudentAttendances { get; private set; }

        public SectionAttendance(string sectionId, SectionMeetingInstance meetingInstance)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A section Id is required for a section attendance");
            }

            if (meetingInstance == null)
            {
                throw new ArgumentNullException("meetingInstance", "A meeting instance is required for a section attendance");
            }

            _SectionId = sectionId;
            _MeetingInstance = meetingInstance;
            // Initialize public readonly collections
            StudentAttendances = _StudentAttendances.AsReadOnly();
        }

        /// <summary>
        /// Add an equated course ID to a course
        /// </summary>
        /// <param name="courseId">Equated course ID</param>
        public void AddStudentSectionAttendance(StudentSectionAttendance studentSectionAttendance)
        {
            if (studentSectionAttendance == null)
            {
                throw new ArgumentNullException("studentSectionAttendance", "studentSectionAttendance must be provided to add to SectionAttendance StudentAttendances.");
            }

            _StudentAttendances.Add(studentSectionAttendance);

        }
    }
}
