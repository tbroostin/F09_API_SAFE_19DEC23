// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance tracking type for a course section
    /// </summary>
    [Serializable]
    public enum AttendanceTrackingType
    {
        /// <summary>
        /// Attendance is tracked with a flag indicating presence or absence
        /// </summary>
        PresentAbsent,
        /// <summary>
        /// Attendance is tracked in hours and minutes by date when the course section does not have any scheduled section meetings
        /// </summary>
        HoursByDateWithoutSectionMeeting,
        /// <summary>
        /// Attendance is tracked in hours and minutes for each section meeting on the course section 
        /// </summary>
        HoursBySectionMeeting,
        /// <summary>
        /// Attendance is tracked in cumulative hours and minutes for the course section with no regard for date or time
        /// </summary>
        CumulativeHours,
        /// <summary>
        /// Attendance is tracked with a flag indicating presence or absence with no associated course section meeting
        /// </summary>
        PresentAbsentWithoutSectionMeeting
    }
}
