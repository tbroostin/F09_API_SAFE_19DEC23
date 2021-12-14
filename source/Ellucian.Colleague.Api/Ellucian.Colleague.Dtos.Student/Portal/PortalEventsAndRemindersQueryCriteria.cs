// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Query criteria for selecting events and reminders to be displayed in the Portal
    /// </summary>
    public class PortalEventsAndRemindersQueryCriteria
    {
        /// <summary>
        /// Earliest date (inclusive) for which to select events and reminders to be displayed in the Portal
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Latest date (inclusive) for which to select events and reminders to be displayed in the Portal
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Collection of event type codes to be used when selecting events and reminders to be displayed in the Portal; these should be internal event type codes.
        /// </summary>
        public IEnumerable<string> EventTypeCodes { get; set; }
    }
}
