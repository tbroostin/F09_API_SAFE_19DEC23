// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The meeting time information related to a section
    /// </summary>
    public class SectionMeeting
    {
        /// <summary>
        /// Code of the instructional method used during this meeting
        /// </summary>
        public string InstructionalMethodCode { get; set; }

        /// <summary>
        /// String-based start time for this section
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// String-based end time for this section
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// List of <see cref="DayOfWeek">days of week</see> this meeting occurs
        /// </summary>
        public IEnumerable<DayOfWeek> Days { get; set; }

        /// <summary>
        /// Room in which this meeting occurs
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// Start date of this meeting
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of this meeting
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Frequency of this meeting
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// Indicates whether this meeting time has an on-line type of instructional method
        /// </summary>
        public bool IsOnline { get; set; }

    }
}
