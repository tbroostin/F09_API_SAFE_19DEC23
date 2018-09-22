// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A nonacademic type of event that can be used for attendance tracking. 
    /// </summary>
    [Serializable]
    public class NonAcademicEvent
    {
        private readonly string _id;
        private DateTimeOffset? _startTime;
        private DateTimeOffset? _endTime;
        private string _termCode;
        private string _title;

        /// <summary>
        /// Unique ID for the nonacademic event
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Term ID for the nonacademic event
        /// </summary>
        public string TermCode { get { return _termCode; } }

        /// <summary>
        /// Date on which event is held
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Time of day at which event begins
        /// </summary>
        public DateTimeOffset? StartTime
        {
            get { return _startTime; }
            set
            {
                if (EndTime.HasValue && value > EndTime)
                {
                    throw new ArgumentException("Start time cannot be later than the end time.");
                }
                _startTime = value;
            }
        }

        /// <summary>
        /// Time of day at which event ends
        /// </summary>
        public DateTimeOffset? EndTime
        {
            get { return _endTime; }
            set
            {
                if (StartTime.HasValue && value < StartTime)
                {
                    throw new ArgumentException("End time cannot be earlier than the start time.");
                }
                _endTime = value;
            }
        }

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
        public string Title { get { return _title; } }

        /// <summary>
        /// Description for the event
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Place the event is held
        /// </summary>
        public string Venue { get; set; }

        /// <summary>
        /// Create a new nonacademic event
        /// </summary>
        /// <param name="id">Id of the event. Required.</param>
        /// <param name="termCode">Term code of the event. Required.</param>
        /// <param name="title">Title of the event.Required.</param>
        public NonAcademicEvent(string id, string termCode, string title)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is required.");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code is required.");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "Title is required");
            }
            _id = id;
            _termCode = termCode;
            _title = title;
        }
    }
}
