// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Nonacademic Event 
    /// </summary>
    public class NonAcademicEvent
    {
        /// <summary>
        /// Unique ID for the non-academic event
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Term code for the non-academic event
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Date on which event is held
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Time of day which event begins
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Time of day which event ends
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Location code for the location (campus) where the event is held
        /// </summary>
        public string LocationCode { get; set; }

        /// <summary>
        /// Building code for the building where the event is held
        /// </summary>
        public string BuildingCode { get; set; }

        /// <summary>
        /// Room code for the room where the event is held
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Code for the attendance type (valcode table)
        /// </summary>
        public string EventTypeCode { get; set; }

        /// <summary>
        /// Title for the event; may include things like topic, speaker, etc.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description for the event
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Place the event is held
        /// </summary>
        public string Venue { get; set; }
    }
}
