using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// SpecialDay is used to identify dates and date ranges that have meaning campus wide, such as holidays,
    /// snow days, etc.
    /// </summary>
    public class SpecialDay
    {
        /// <summary>
        /// The Id of the Special Day
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A Description of the Special Day
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Id of the CampusCalendar to which this special day belongs
        /// </summary>
        public string CampusCalendarId { get; set; }

        /// <summary>
        /// The code indicating the type of special day
        /// </summary>
        public string SpecialDayTypeCode { get; set; }

        /// <summary>
        /// Whether this special day is a Holiday
        /// </summary>
        public bool IsHoliday { get; set; }

        /// <summary>
        /// Whether this special day is a payroll Holiday
        /// </summary>
        public bool IsPayrollHoliday { get; set; }

        /// <summary>
        /// Whether this special day is a full day or not.
        /// </summary>
        public bool IsFullDay { get; set; }

        /// <summary>
        /// The Date and Time this special day starts. This will only contain the Date portion if this is a full day.
        /// </summary>
        public DateTimeOffset StartDateTime { get; set; }

        /// <summary>
        /// The Date and Time this special day ends. This will only contain the Date portion if this is a full day.
        /// </summary>
        public DateTimeOffset EndDateTime { get; set; }
    }
}