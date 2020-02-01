// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This entity holds the individual office hours information for a faculty
    /// This consists of the data from a single row in FCTY
    /// </summary>
    public class OfficeHours
    {

        /// <summary>
        /// Faculty office start date
        /// </summary>
        public DateTime? OfficeStartDate { get; set; }

        /// <summary>
        /// Faculty office end date
        /// </summary>
        public DateTime? OfficeEndDate { get; set; }

        /// <summary>
        /// Faculty office start time
        /// </summary>
        public DateTime? OfficeStartTime { get; set; }

        /// <summary>
        /// Faculty office end time
        /// </summary>
        public DateTime? OfficeEndTime { get; set; }

        /// <summary>
        /// Faculty office building
        /// </summary>
        public string OfficeBuilding { get; set; }

        /// <summary>
        /// Faculty office room
        /// </summary>
        public string OfficeRoom { get; set; }

        /// <summary>
        /// The frequency of repeatation of faculty office hours whether its weekly, monthly, yearly and so on
        /// </summary>
        public string OfficeFrequency { get; set; }

        /// <summary>
        /// The days of the week when the faculty will be available
        /// Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday
        /// </summary>
        public List<DayOfWeek> DaysOfWeek { get; set; }

    }
}
