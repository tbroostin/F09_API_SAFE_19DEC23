// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Event to be displayed in the Portal
    /// </summary>
    [Serializable]
    public class PortalEvent
    {
        /// <summary>
        /// Unique event identifier
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Unique identifier for the course section associated with the event
        /// </summary>
        public string CourseSectionId { get; private set; }

        /// <summary>
        /// Date on which the event occurs
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Event start time
        /// </summary>
        public DateTimeOffset? StartTime { get; private set; }

        /// <summary>
        /// Event end time
        /// </summary>
        public DateTimeOffset? EndTime { get; private set; }

        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Building in which the event occurs
        /// </summary>
        public string Building { get; private set; }

        /// <summary>
        /// Room in which the event occurs
        /// </summary>
        public string Room { get; private set; }

        /// <summary>
        /// The type of event
        /// <remarks>This will contain the internal event type code and its associated description</remarks>
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        /// Subject for the course section associated with the event
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Course number for the course section associated with the event
        /// </summary>
        public string CourseNumber { get; private set; }

        /// <summary>
        /// Section number for the course section associated with the event
        /// </summary>
        public string SectionNumber { get; private set; }

        /// <summary>
        /// Comma-separated list of participants for the event
        /// </summary>
        public string Participants { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortalEvent"/> object
        /// </summary>
        /// <param name="id">Unique event identifier</param>
        /// <param name="courseSectionId">Unique identifier for the course section associated with the event</param>
        /// <param name="date">Date on which the event occurs</param>
        /// <param name="startTime">Event start time</param>
        /// <param name="endTime">Event end time</param>
        /// <param name="description">Description of the event</param>
        /// <param name="building">Building in which the event occurs</param>
        /// <param name="room">Room in which the event occurs</param>
        /// <param name="eventType">The type of event</param>
        /// <param name="subject">Subject for the course section associated with the event</param>
        /// <param name="courseNumber">Course number for the course section associated with the event</param>
        /// <param name="sectionNumber">Section number for the course section associated with the event</param>
        /// <param name="participants">Comma-separated list of participants for the event</param>
        public PortalEvent(string id, 
            string courseSectionId,
            DateTime? date,
            DateTimeOffset? startTime,
            DateTimeOffset? endTime,
            string description,
            string building,
            string room,
            string eventType,
            string subject,
            string courseNumber,
            string sectionNumber,
            string participants)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "Unique event identifier is required when building Portal event information.");
            }
            if (string.IsNullOrWhiteSpace(courseSectionId))
            {
                throw new ArgumentNullException("courseSectionId", "Unique course section identifier is required when building Portal event information.");
            }
            if (!date.HasValue)
            {
                throw new ArgumentNullException("date", "Date is required when building Portal event information.");
            }
            if (!startTime.HasValue && endTime.HasValue)
            {
                throw new ArgumentException("endTime", "Event cannot have an end time without a start time.");
            }
            if (startTime.HasValue && endTime.HasValue && startTime.Value > endTime.Value)
            {
                throw new ArgumentException("startTime", "Event start time cannot be later than event end time.");
            }
            Id = id;
            CourseSectionId = courseSectionId;
            Date = date.Value;
            StartTime = startTime;
            EndTime = endTime;
            Description = description;
            Building = building;
            Room = room;
            EventType = eventType;
            Subject = subject;
            CourseNumber = courseNumber;
            SectionNumber = sectionNumber;
            Participants = participants;
        }
    }
}
