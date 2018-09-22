// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A specific meeting for a section
    /// </summary>
    public class SectionMeetingInstance
    {
        /// <summary>
        /// Id of the SectionMeetingInstance 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Section Id (Required)
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Meeting Date (Required)
        /// </summary>
        public DateTime MeetingDate { get; set; }
        /// <summary>
        /// Time of Day that the section meeting instance begins
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }
        /// <summary>
        /// Time of Day that the section meeting instance ends
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }
        /// <summary>
        /// Instructional Method associated to this meeting 
        /// </summary>
        public string InstructionalMethod { get; set; }
    }
}
