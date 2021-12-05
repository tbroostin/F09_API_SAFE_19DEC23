// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Event to be displayed in the Portal
    /// </summary>
    public class PortalEvent
    {
        /// <summary>
        /// Unique event identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier for the course section associated with the event
        /// </summary>
        public string CourseSectionId { get; set; }

        /// <summary>
        /// Date on which the event occurs
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Event start time
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Event end time
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Building in which the event occurs
        /// </summary>
        public string Building { get; set; }

        /// <summary>
        /// Room in which the event occurs
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// The type of event
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Subject for the course section associated with the event
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Course number for the course section associated with the event
        /// </summary>
        public string CourseNumber { get; set; }

        /// <summary>
        /// Section number for the course section associated with the event
        /// </summary>
        public string SectionNumber { get; set; }

        /// <summary>
        /// Comma-separated list of participants for the event
        /// </summary>
        public string Participants { get; set; }
    }
}
