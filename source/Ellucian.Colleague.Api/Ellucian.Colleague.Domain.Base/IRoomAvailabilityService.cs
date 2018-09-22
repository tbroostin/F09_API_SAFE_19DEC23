// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base
{
    public interface IRoomAvailabilityService
    {
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
        IEnumerable<DateTime> BuildDateList(DateTime startDate, DateTime endDate, FrequencyType frequency, int interval, IEnumerable<DayOfWeek> recurrenceDays, IEnumerable<DateTime> specialDays, int backDatingLimit);
    
        /// <summary>
        /// Get rooms whose capacity meets or exceeds the supplied occupancies
        /// </summary>
        /// <param name="rooms">Collection of rooms</param>
        /// <param name="maxOccupancy">Maximum Occupancy</param>
        /// <returns>Collection of rooms with capacity</returns>
        IEnumerable<Room> GetRoomsWithCapacity(IEnumerable<Room> rooms, int maxOccupancy);
    }
}
