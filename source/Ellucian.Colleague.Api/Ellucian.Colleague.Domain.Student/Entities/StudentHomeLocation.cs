// Copyright 2015 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student home location and its start and end date.
    /// </summary>
    [Serializable]
    public class StudentHomeLocation
    {
        private readonly string _location;
        private readonly DateTime? _startDate;

        /// <summary>
        /// Student home location
        /// Ex: East
        /// </summary>
        public string Location { get { return _location; } }        
        /// <summary>
        /// Start Date for the location
        /// </summary>
        public DateTime? StartDate { get { return _startDate; } }
        /// <summary>
        /// End Date for the location
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Flag to indicate primary home location.  Set when
        /// incoming term provided to set primary to most
        /// recent home location that intersects with that
        /// term.
        /// </summary>
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Initialize the Student Home Location method
        /// </summary>
        /// <param name="location">Student home location</param>
        /// <param name="startDate">Student home location start date</param>
        /// <param name="endDate">Student home location end date</param>
        public StudentHomeLocation(string location, DateTime? startDate, DateTime? endDate, bool isPrimary)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException("location");
            }
            if (startDate == null || startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate");
            }
            _location = location;
            _startDate = startDate;
            EndDate = endDate;
            IsPrimary = isPrimary;
        }
    }
}