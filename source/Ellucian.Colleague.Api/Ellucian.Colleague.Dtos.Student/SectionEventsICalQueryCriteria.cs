using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to query a set of section ids to
    /// return section events in ICal format 
    /// </summary>
    public class SectionEventsICalQueryCriteria
    {
        /// <summary>
        /// List of section Id being queried
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }
        /// <summary>
        /// Start Date for calendar schedules
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date for calendar schedules
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
