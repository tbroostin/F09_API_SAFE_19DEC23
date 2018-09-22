// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance information related to a section attendance update
    /// </summary>
    [Serializable]
    public class SectionAttendanceResponse
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

        private readonly List<StudentAttendance> _UpdatedStudentAttendances = new List<StudentAttendance>();
        /// <summary>
        /// Student attendance items successfully updated for the section.
        /// </summary>
        public ReadOnlyCollection<StudentAttendance> UpdatedStudentAttendances { get; private set; }

        private readonly List<StudentSectionAttendanceError> _StudentAttendanceErrors = new List<StudentSectionAttendanceError>();
        /// <summary>
        /// Student section attendance items that failed on update and why.
        /// </summary>
        public ReadOnlyCollection<StudentSectionAttendanceError> StudentAttendanceErrors { get; private set; }

        private readonly List<string> _StudentCourseSectionsWithDeletedAttendances = new List<string>();
        /// <summary>
        /// Student attendance items successfully deleted for the section.
        /// </summary>
        public ReadOnlyCollection<string> StudentCourseSectionsWithDeletedAttendances { get; private set; }

        public SectionAttendanceResponse(string sectionId, SectionMeetingInstance meetingInstance)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A section Id is required for a section attendance response");
            }

            if (meetingInstance == null)
            {
                throw new ArgumentNullException("meetingInstance", "A meeting instance is required for a section attendance response");
            }
            _SectionId = sectionId;
            _MeetingInstance = meetingInstance;

            // Initialize public readonly collections
            UpdatedStudentAttendances = _UpdatedStudentAttendances.AsReadOnly();
            StudentAttendanceErrors = _StudentAttendanceErrors.AsReadOnly();
            StudentCourseSectionsWithDeletedAttendances = _StudentCourseSectionsWithDeletedAttendances.AsReadOnly();
        }

        /// <summary>
        /// Adds an updated attendance
        /// </summary>
        /// <param name="studentAttendance">Updated student attendance</param>
        public void AddUpdatedStudentAttendance(StudentAttendance studentAttendance)
        {
            if (studentAttendance == null)
            {
                throw new ArgumentNullException("studentAttendance", "Student Attendance is required to add a new updated Student Attendance to a SectionAttendanceResponse.");
            }
            _UpdatedStudentAttendances.Add(studentAttendance);
        }

        /// <summary>
        /// Adds a student attendance error
        /// </summary>
        /// <param name="studentAttendanceError">Student attendance error</param>
        public void AddStudentAttendanceError(StudentSectionAttendanceError studentAttendanceError)
        {
            if (studentAttendanceError == null)
            {
                throw new ArgumentNullException("studentAttendanceError", "Student attendance error is required to add a new StudentAttendanceError.");
            }
            _StudentAttendanceErrors.Add(studentAttendanceError);
        }

        /// <summary>
        /// Adds a student course section with deleted attendance
        /// </summary>
        /// <param name="studentCourseSecId">Student course section with deleted attendance</param>
        public void AddStudentCourseSectionsWithDeletedAttendance(string studentCourseSecId)
        {
            if (string.IsNullOrEmpty(studentCourseSecId))
            {
                throw new ArgumentNullException("studentCourseSecId", "Student Course Section ID is required to add a new student course section with deleted attendance to a SectionAttendanceResponse.");
            }
            _StudentCourseSectionsWithDeletedAttendances.Add(studentCourseSecId);
        }
    }
}
