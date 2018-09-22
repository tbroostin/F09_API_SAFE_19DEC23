// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains a list of student attendances to be updated for a specific meeting instance in a specific section
    /// </summary>
    public class SectionAttendance
    {
        /// <summary>
        /// Section Id being updated with the attendance information.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Section meeting instance describing the meeting date, start time, end time and instructional method related to the attendance information being updated. 
        /// This must be a valid meeting instance for the associated section or for one of the section's cross-listed sections.
        /// </summary>
        public SectionMeetingInstance MeetingInstance { get; set; }

        /// <summary>
        /// Collection of student Section attendances to be updated for the section.
        /// </summary>
        public List<StudentSectionAttendance> StudentAttendances { get; set; }
    }
}
