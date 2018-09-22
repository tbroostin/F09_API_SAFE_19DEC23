// Copyright 2015 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student home location and accompanying start and end date
    /// </summary>
    public class StudentHomeLocation
    {
        /// <summary>
        /// Ex: East
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Start Date for the student home location
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End Date for the student student location
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Flag to indicate primary home location.  Set when
        /// incoming term provided to set primary to most
        /// recent home location that intersects with that
        /// term.
        /// </summary>
        public bool IsPrimary { get; set; }
    }
}