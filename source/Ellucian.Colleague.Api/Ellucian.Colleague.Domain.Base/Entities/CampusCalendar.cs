/* Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Campus Calendar used for scheduling purposes and identification of "special days" on campus
    /// </summary>
    [Serializable]
    public class CampusCalendar
    {
        private readonly string _id;
        private readonly string _description;
        private readonly TimeSpan _defaultStartOfDay;
        private readonly TimeSpan _defaultEndOfDay;
        private List<DateTime> _bookedEventDates = new List<DateTime>();
        private List<SpecialDay> _specialDays = new List<SpecialDay>();

        /// <summary>
        /// The Id of the CampusCalendar
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get { return _description; } }

        /// <summary>
        /// The Default TimeOfDay that an event or special day will start.
        /// This TimeSpan represents the fraction of a day elapsed since midnight
        /// </summary>
        public TimeSpan DefaultStartOfDay { get { return _defaultStartOfDay; } }
        /// <summary>
        /// The Default TimeOfDay that an event or special day will end.
        /// This TimeSpan represents the fraction of a day elapsed since midnight
        /// </summary>
        public TimeSpan DefaultEndOfDay { get { return _defaultEndOfDay; } }

        /// <summary>
        /// The collection of Special Days associated to this Calendar
        /// </summary>
        public ReadOnlyCollection<SpecialDay> SpecialDays
        {
            get
            {
                return _specialDays.AsReadOnly();
            }
        }

        /// <summary>
        /// The collection of dates that are booked by events created from the special days
        /// on this calendar
        /// </summary>
        public ReadOnlyCollection<DateTime> BookedEventDates
        {
            get
            {
                return _bookedEventDates.AsReadOnly();
            }        
        }

        /// <summary>
        /// The number of days to look back from today that this calendar will book an event
        /// based on the special days
        /// </summary>
        public int BookPastNumberOfDays { get; set; }

        /// <summary>
        /// Constructor for campus calendar
        /// </summary>
        /// <param name="id">Calendar ID</param>
        /// <param name="description">Description of calendar</param>
        /// <param name="defaultStartOfDay">Time of day at which conflict checking begins during scheduling. Represented as a TimeSpan indicating the fraction of day that has elapsed since midnight</param>
        /// <param name="defaultEndOfDay">Time of day at which conflict checking ends during scheduling. Represented as a TimeSpan indicating the fraction of day that has elapsed since midnight</param>
        public CampusCalendar(string id, string description, TimeSpan defaultStartOfDay, TimeSpan defaultEndOfDay)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (defaultEndOfDay < defaultStartOfDay)
            {
                throw new ArgumentOutOfRangeException("defaultEndOfDay", "End of day cannot be earlier than start of day.");
            }

            _id = id;
            _description = description;
            _defaultStartOfDay = defaultStartOfDay;
            _defaultEndOfDay = defaultEndOfDay;
        }

        /// <summary>
        /// Add a booked event day to the calendar
        /// </summary>
        /// <param name="day">Special Day</param>
        public void AddBookedEventDate(DateTime day)
        {
            // Prevent duplicates
            if (!_bookedEventDates.Contains(day))
            {
                _bookedEventDates.Add(day);
            }
        }

        /// <summary>
        /// Add a special day to the calendar
        /// </summary>
        /// <param name="day"></param>
        public void AddSpecialDay(SpecialDay day)
        {
           
            if (!_specialDays.Contains(day) &&  //prevent duplicates
                day.CampusCalendarId == Id) //ensure special day belongs to this calendar
            {
                //insert sorted
                var index = _specialDays.BinarySearch(day);
                if (index < 0)
                {
                    _specialDays.Insert(~index, day);
                }
            }
        }
    }
}
