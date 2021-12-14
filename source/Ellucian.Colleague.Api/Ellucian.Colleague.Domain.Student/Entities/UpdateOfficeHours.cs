// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Information needed to Update faculty office hours.
    /// </summary>
    [Serializable]
    public class UpdateOfficeHours
    {
        /// <summary>
        /// Office Hours Unique Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Old Start Date
        /// </summary>
        public DateTime? OldStartDate { get; set; }
        /// <summary>
        /// Old End Date
        /// </summary>
        public DateTime? OldEndDate { get; set; }
        /// <summary>
        /// Old Starts by Time
        /// </summary>
        public string OldStartsByTime { get; set; }
        /// <summary>
        /// Old Ends by Time
        /// </summary>
        public string OldEndsByTime { get; set; }
        /// <summary>
        /// Old Building Code
        /// </summary>
        public string OldBuildingCode { get; set; }
        /// <summary>
        /// Old Room Code
        /// </summary>
        public string OldRoomCode { get; set; }
        /// <summary>
        /// Old Frequency
        /// </summary>
        public string OldFrequency { get; set; }
        /// <summary>
        /// Old  Days of the week that the faculty member has office hours
        /// </summary>
        public string OldDaysOfWeek { get; set; }

        /// <summary>
        /// New Start Date
        /// </summary>
        public DateTime? NewStartDate { get; set; }
        /// <summary>
        /// New End Date
        /// </summary>
        public DateTime? NewEndDate { get; set; }
        /// <summary>
        /// New Starts by Time
        /// </summary>
        public string NewStartsByTime { get; set; }
        /// <summary>
        /// New Ends by Time
        /// </summary>
        public string NewEndsByTime { get; set; }
        /// <summary>
        /// New Building Code
        /// </summary>
        public string NewBuildingCode { get; set; }
        /// <summary>
        /// New Room Code
        /// </summary>
        public string NewRoomCode { get; set; }
        /// <summary>
        /// New Frequency
        /// </summary>
        public string NewFrequency { get; set; }
        /// <summary>
        /// New  Days of the week that the faculty member has office hours
        /// </summary>
        public string NewDaysOfWeek { get; set; }
    }
}
