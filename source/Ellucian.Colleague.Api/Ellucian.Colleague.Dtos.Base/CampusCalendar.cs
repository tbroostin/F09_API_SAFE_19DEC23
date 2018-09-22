using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Campus Calendar used for scheduling purposes and identification of "special days" on campus
    /// </summary>
    public class CampusCalendar
    {
        /// <summary>
        /// The Id of the CampusCalendar
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Default TimeOfDay that an event or special day will start.
        /// This TimeSpan represents the fraction of a day elapsed since midnight
        /// </summary>
        public TimeSpan DefaultStartOfDay { get; set; }
        /// <summary>
        /// The Default TimeOfDay that an event or special day will end.
        /// This TimeSpan represents the fraction of a day elapsed since midnight
        /// </summary>
        public TimeSpan DefaultEndOfDay { get; set; }

        /// <summary>
        /// The collection of Special Days associated to this Calendar
        /// </summary>
        public List<SpecialDay> SpecialDays { get; set; }

        /// <summary>
        /// The collection of dates that are booked by events created from the special days
        /// on this calendar
        /// </summary>
        public List<DateTime> BookedEventDates { get; set; }

        /// <summary>
        /// The number of days to look back from today that this calendar will book an event
        /// based on the special days
        /// </summary>
        public int BookPastNumberOfDays { get; set; }
    }
}
