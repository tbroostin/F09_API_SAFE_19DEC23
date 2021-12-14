// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information needed to delete faculty office hours.
    /// </summary>
    public class DeleteOfficeHours
    {
        /// <summary>
        /// Office Hours Unique Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Office hours Start Date
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Office hours End Date
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Office hours Starts by Time
        /// </summary>
        public string StartsByTime { get; set; }
        /// <summary>
        /// Office hours Ends by Time
        /// </summary>
        public string EndsByTime { get; set; }
        /// <summary>
        /// Office Hours Building Code
        /// </summary>
        public string BuildingCode { get; set; }
        /// <summary>
        /// Office Hours Room Code
        /// </summary>
        public string RoomCode { get; set; }
        /// <summary>
        /// Office Hours Frequency
        /// </summary>
        public string Frequency { get; set; }
        /// <summary>
        /// Days of the week that the faculty member has office hours
        /// </summary>
        public string DaysOfWeek { get; set; }
    }
}
