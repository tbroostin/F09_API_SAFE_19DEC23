//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A student's attendance for a nonacademic event
    /// </summary>
    [Serializable]
    public class NonAcademicAttendance
    {
        private string _id;
        /// <summary>
        /// Unique identifier for the nonacademic event attendance
        /// </summary>
        public string Id { get { return _id; } }

        private string _personId;
        /// <summary>
        /// Unique identifier for the person who attended the nonacademic event
        /// </summary>
        public string PersonId { get { return _personId; } }

        private string _eventId;
        /// <summary>
        /// Unique identifier for the nonacademic event that the person attended
        /// </summary>
        public string EventId { get { return _eventId; } }

        private decimal? _unitsEarned;
        /// <summary>
        /// The number of units that the person earned for attending the nonacademic event
        /// </summary>
        public decimal? UnitsEarned { get { return _unitsEarned; } }

        /// <summary>
        /// Creates a new <see cref="NonAcademicAttendance"/> object.
        /// </summary>
        /// <param name="id">Unique identifier for the nonacademic event attendance</param>
        /// <param name="personId">Unique identifier for the person who attended the nonacademic event</param>
        /// <param name="eventId"> Unique identifier for the nonacademic event that the person attended</param>
        /// <param name="unitsEarned">The number of units that the person earned for attending the nonacademic event</param>
        public NonAcademicAttendance(string id, string personId, string eventId, decimal? unitsEarned = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A nonacademic event attendance must have a unique identifier.");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A nonacademic event attendance must have a unique identifier for the person who attended the event.");
            }
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException("eventId", "A nonacademic event attendance must have a unique identifier for the event that the person attended.");
            }
            if (unitsEarned.HasValue && unitsEarned.Value < 0)
            {
                throw new ArgumentOutOfRangeException("unitsEarned", "The number of units that the person earned for attending the nonacademic event must be greater than or equal to zero, if specified.");
            }

            _id = id;
            _personId = personId;
            _eventId = eventId;
            _unitsEarned = unitsEarned;
        }
    }
}
