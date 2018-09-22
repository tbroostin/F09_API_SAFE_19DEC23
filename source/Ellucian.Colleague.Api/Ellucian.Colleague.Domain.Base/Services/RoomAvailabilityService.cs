// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using slf4net;
//using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Base.Services
{
    public class RoomAvailabilityService
    {
        /// <summary>
        /// Get rooms whose capacity meets or exceeds the supplied occupancies
        /// </summary>
        /// <param name="rooms">Collection of rooms</param>
        /// <param name="maxOccupancy">Maximum Occupancy</param>
        /// <returns>Collection of rooms with capacity</returns>
        public static IEnumerable<Entities.Room> GetRoomsWithCapacity(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> rooms, int maxOccupancy)
        {
            if (maxOccupancy <= 0)
            {
                throw new ArgumentOutOfRangeException("maxOccupancy", "The maximum occupancy must be greater than zero.");
            }
            if (rooms == null || rooms.Count() == 0)
            {
                throw new ArgumentNullException("rooms", "At least one room must be specified.");
            }

            return rooms.Where(r => r.Capacity >= maxOccupancy).Distinct().ToList();
        }

        /// <summary>
        /// Build a list of dates for a given date range and recurrence patterns
        /// </summary>
        /// <param name="startDate">Earliest Date that can be included in the list</param>
        /// <param name="endDate">Latest Date that can be included in the list</param>
        /// <param name="frequency">Frequency of recurrence</param>
        /// <param name="interval">Interval at which recurrence occurs</param>
        /// <param name="recurrenceDays">Days of week on which recurrence pattern occurs</param>
        /// <param name="specialDays">Special days to be removed from the final list of dates</param>        
        /// <param name="backDatingLimit">Number of days in past that back-dating is permitted</param>
        /// <returns>List of Dates</returns>
        public static IEnumerable<DateTime> BuildDateList(DateTime startDate, DateTime endDate, FrequencyType frequency, int interval, IEnumerable<DayOfWeek> recurrenceDays, IEnumerable<DateTime> specialDays, int backDatingLimit)
        {
            if (recurrenceDays == null || recurrenceDays.Count() == 0)
            {
                throw new ArgumentNullException("recurrenceDays", "At least one day of the week must be specified on the recurrence pattern.");
            }
            if (interval <= 0)
            {
                throw new ArgumentOutOfRangeException("interval", "Recurrence interval must be greater than zero.");
            }
            if (backDatingLimit < 0)
            {
                throw new ArgumentOutOfRangeException("backDatingLimit", "Back dating limit cannot be less than zero.");
            }
            if (specialDays == null)
            {
                specialDays = new List<DateTime>();
            }

            var dates = new List<DateTime>();
            foreach (var day in recurrenceDays)
            {
                // Get the "true" start date using the supplied start date and the back-dating limit
                var trueStartDate = new DateTime(Math.Max(startDate.Ticks, DateTime.Today.AddDays(-backDatingLimit).Ticks));
                if (trueStartDate.DayOfWeek != day)
                {
                    int diff = (int)day - (int)trueStartDate.DayOfWeek;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    trueStartDate = trueStartDate.AddDays(diff);
                }


                var date = trueStartDate;
                while (date <= endDate)
                {
                    if (!specialDays.Contains(date) 
                        && !dates.Contains(date) 
                        && (frequency != FrequencyType.Weekly|| (frequency == FrequencyType.Weekly && date.DayOfWeek == day)))
                    {
                        if (((frequency == FrequencyType.Daily || frequency == FrequencyType.Weekly) && date.DayOfWeek == day) 
                            || (frequency == FrequencyType.Monthly || frequency == FrequencyType.Yearly))
                        {
                            dates.Add(date);
                        }
                    }
                    switch (frequency)
                    {
                        case FrequencyType.Weekly:
                            date = date.AddDays(7 * interval);
                            break;
                        case FrequencyType.Monthly:
                            date = date.AddMonths(1 * interval);
                            break;
                        case FrequencyType.Yearly:
                            date = date.AddYears(1 * interval);
                            break;
                        default:
                            date = date.AddDays(1 * interval);
                            break;
                    }
                }
            }
            return dates.Distinct().OrderBy(d => d).ToList();
        }
    }
}
