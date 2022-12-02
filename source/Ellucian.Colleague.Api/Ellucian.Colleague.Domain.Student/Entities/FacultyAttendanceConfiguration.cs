// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how faculty attendance is handled.
    /// </summary>
    [Serializable]
    public class FacultyAttendanceConfiguration
    {
        /// <summary>
        /// The line number for the census date after which attendance entry should be disabled in Self-Service
        /// </summary>
        public int? CloseAttendanceCensusTrackNumber { get; set; }

        /// <summary>
        /// The number of days past the census date identified in the Close Attendance Entry After Census Date field after which attendance entry should be disabled in Self-Service
        /// </summary>
        public int? CloseAttendanceNumberOfDaysPastCensusTrackDate { get; set; }

        /// <summary>
        /// The number of days past the section end date identified on SECT (SEC.END.DATE) after which attendance entry should be disabled in Self-Service
        /// </summary>
        public int? CloseAttendanceNumberOfDaysPastSectionEndDate { get; set; }
    }
}