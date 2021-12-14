// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Events and reminders to be displayed in the Portal for the authenticated user
    /// </summary>
    public class PortalEventsAndReminders
    {
        /// <summary>
        /// Events to be displayed in the Portal for the authenticated user
        /// </summary>
        public IEnumerable<PortalEvent> Events { get; set; }

        /// <summary>
        /// Reminders to be displayed in the Portal for the authenticated user
        /// </summary>
        public IEnumerable<PortalReminder> Reminders { get; set; }

        /// <summary>
        /// Host short date format for the Colleague environment
        /// </summary>
        public string HostShortDateFormat { get; set; }
    }
}
