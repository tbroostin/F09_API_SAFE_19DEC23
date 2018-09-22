// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains a list of student attendances to be updated for a specific meeting instance in a specific section
    /// </summary>
    public class SectionAttendanceResponse
    {
        /// <summary>
        /// Section Id updated with the attendance information.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Section meeting instance used for update (describing the meeting date, start time, end time and instructional method) 
        /// </summary>
        public SectionMeetingInstance MeetingInstance { get; set; }

        /// <summary>
        /// Student section attendance items successfully updated for the section.
        /// </summary>
        public IEnumerable<StudentAttendance> UpdatedStudentAttendances { get; set; }

        /// <summary>
        /// Student section attendance items that failed on update and why.
        /// </summary>
        public IEnumerable<StudentSectionAttendanceError> StudentAttendanceErrors { get; set; }

        /// <summary>
        /// Student attendance items successfully deleted for the section.
        /// </summary>
        public IEnumerable<string> StudentCourseSectionsWithDeletedAttendances { get; set; }

    }
}
