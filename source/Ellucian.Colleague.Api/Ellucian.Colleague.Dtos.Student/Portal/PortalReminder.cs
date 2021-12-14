// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Reminder to be displayed in the Portal
    /// </summary>
    public class PortalReminder
    {
        /// <summary>
        /// Unique reminder identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date on which the action is to take place
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Time at which the action is to take place
        /// </summary>
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Date on which the action is anticipated to end
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Time at which the action is anticipated to end
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// City in which the reminder action is to take place
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Region in which the reminder action is to take place
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The type of reminder
        /// <remarks>This will contain the internal reminder type code and its associated description</remarks>
        /// </summary>
        public string ReminderType { get; set; }

        /// <summary>
        /// Text of the reminder
        /// </summary>
        public string ShortText { get; set; }

        /// <summary>
        /// Comma-separated list of participants for the reminder
        /// </summary>
        public string Participants { get; set; }
    }
}
