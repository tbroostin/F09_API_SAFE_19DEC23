// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Query criteria object for selecting events and reminders for a user
    /// </summary>
    [Serializable]
    public class PortalEventsAndRemindersQueryCriteria
    {
        /// <summary>
        /// Earliest date (inclusive) for which to select events and reminders for the user
        /// </summary>
        public DateTime StartDate { get; private set; }
        /// <summary>
        /// Latest date (inclusive) for which to select events and reminders for the user
        /// </summary>
        public DateTime EndDate { get; private set; }
        /// <summary>
        /// Collection of event type codes to be used when selecting events and reminders for the user
        /// </summary>
        /// <remarks>These should be internal codes from the EVENT.TYPES valcode table</remarks>
        public IEnumerable<string> EventTypeCodes { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortalEventsAndRemindersQueryCriteria"/> object
        /// </summary>
        /// <param name="startDate">Earliest date (inclusive) for which to select events and reminders for the user; the current date will be used if no Start Date is provided</param>
        /// <param name="endDate">Latest date (inclusive) for which to select events and reminders for the user; the current date will be used if no End Date is provided</param>
        /// <param name="eventTypes">Collection of event type codes to be used when selecting events and reminders for the user; Course Section Meeting events will be selected if no types are provided</param>
        public PortalEventsAndRemindersQueryCriteria(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> eventTypeCodes = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentOutOfRangeException("startDate", string.Format("Start Date {0} is later than End Date {1}; when querying events and reminders for a user, the Start Date cannot be later than the End Date when both dates are provided.", 
                    startDate.Value.ToShortDateString(),
                    endDate.Value.ToShortDateString()));
            }
            if (startDate.HasValue && !endDate.HasValue && startDate.Value > DateTime.Today)
            {
                throw new ArgumentOutOfRangeException("startDate", string.Format("Start Date {0} is later than the current date; When querying events and reminders for a user and no End Date is provided, the current date is used as an End Date and the Start Date cannot be later than the End Date.",
                    startDate.Value.ToShortDateString()));
            }
            if (endDate.HasValue && !startDate.HasValue && endDate.Value < DateTime.Today)
            {
                throw new ArgumentOutOfRangeException("endDate", string.Format("End Date {0} is earlier than the current date; When querying events and reminders for a user and no Start Date is provided, the current date is used as a Start Date and the End Date cannot be earlier than the Start Date.",
                    endDate.Value.ToShortDateString()));
            }
            StartDate = startDate.HasValue ? startDate.Value : DateTime.Today;
            EndDate = endDate.HasValue ? endDate.Value : DateTime.Today;
            EventTypeCodes = eventTypeCodes != null ? eventTypeCodes : new List<string>() { "CS" };
        }
    }
}
