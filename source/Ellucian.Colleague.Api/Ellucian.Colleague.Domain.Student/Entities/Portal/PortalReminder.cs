// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Reminder to be displayed in the Portal
    /// </summary>
    [Serializable]
    public class PortalReminder
    {
        /// <summary>
        /// Unique reminder identifier
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Date on which the action is to take place
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Time at which the action is to take place
        /// </summary>
        public DateTimeOffset StartTime { get; private set; }

        /// <summary>
        /// Date on which the action is anticipated to end
        /// </summary>
        public DateTime? EndDate { get; private set; }

        /// <summary>
        /// Time at which the action is anticipated to end
        /// </summary>
        public DateTimeOffset? EndTime { get; private set; }

        /// <summary>
        /// City in which the reminder action is to take place
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// Region in which the reminder action is to take place
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// The type of reminder
        /// <remarks>This will contain the internal reminder type code and its associated description</remarks>
        /// </summary>
        public string ReminderType { get; private set; }

        /// <summary>
        /// Text of the reminder
        /// </summary>
        public string ShortText { get; private set; }

        /// <summary>
        /// Comma-separated list of participants for the reminder
        /// </summary>
        public string Participants { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortalReminder"/> object
        /// </summary>
        /// <param name="id">Unique reminder identifier</param>
        /// <param name="startDate">Date on which the action is to take place</param>
        /// <param name="startTime">Time at which the action is to take place</param>
        /// <param name="endDate">Date on which the action is anticipated to end</param>
        /// <param name="endTime">Time at which the action is anticipated to end</param>
        /// <param name="city">City in which the reminder action is to take place</param>
        /// <param name="region">Region in which the reminder action is to take place</param>
        /// <param name="reminderType">The type of reminder</param>
        /// <param name="shortText">Comma-separated list of participants for the reminder</param>
        public PortalReminder(string id,
            DateTime startDate,
            DateTimeOffset startTime,
            DateTime? endDate,
            DateTimeOffset? endTime,
            string city,
            string region,
            string reminderType,
            string shortText,
            string participants)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "Unique reminder identifier is required when building Portal reminder information.");
            }
            if (endDate.HasValue && endDate.Value < startDate)
            {
                throw new ArgumentException("endDate", "Reminder end date cannot be earlier than reminder start date.");
            }
            if (string.IsNullOrWhiteSpace(reminderType))
            {
                throw new ArgumentNullException("reminderType", "Reminder type is required when building Portal reminder information.");
            }
            if (string.IsNullOrWhiteSpace(shortText))
            {
                throw new ArgumentNullException("shortText", "Reminder text is required when building Portal reminder information.");
            }
            Id = id;
            StartDate = startDate;
            StartTime = startTime;
            EndDate = endDate;
            EndTime = endTime;
            City = city;
            Region = region;
            ReminderType = reminderType;
            ShortText = shortText;
            Participants = participants;
        }
    }
}
