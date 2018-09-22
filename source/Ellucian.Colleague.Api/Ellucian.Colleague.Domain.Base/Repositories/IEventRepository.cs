// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for event repositories
    /// </summary>
    public interface IEventRepository 
    {
        /// <summary>
        /// Gets the specified calendar schedule identifier.
        /// </summary>
        /// <param name="calendarScheduleId">The calendar schedule identifier.</param>
        /// <returns></returns>
         Event Get(string calendarScheduleId);

         /// <summary>
         /// Gets the specified calendar schedule type.
         /// </summary>
         /// <param name="calendarScheduleType">Type of the calendar schedule.</param>
         /// <param name="calendarSchedulePointers">The calendar schedule pointers.</param>
         /// <param name="startDate">The start date.</param>
         /// <param name="endDate">The end date.</param>
         /// <returns></returns>
        IEnumerable<Event> Get(string calendarScheduleType, IEnumerable<string> calendarSchedulePointers, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Gets a campus calendar
        /// </summary>
        /// <returns>Campus calendar</returns>
        CampusCalendar GetCalendar(string calendarId);

        /// <summary>
        /// Get all calendars
        /// </summary>
        /// <returns>An enumerable of CampusCalendar objects</returns>
        Task<IEnumerable<CampusCalendar>> GetCalendarsAsync();
    
        /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="meetingDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="building">Building code used to filter rooms</param>
        /// <param name="allBuildingsFromLocation">Collection of building codes from a location used as a filter</param>
        /// <returns>Collection of room IDs</returns>
        IEnumerable<string> GetRoomIdsWithConflicts(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, string building, IEnumerable<string> allBuildingsFromLocation);
       /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
       /// </summary>
       /// <param name="startTime"></param>
       /// <param name="endTime"></param>
       /// <param name="meetingDates"></param>
       /// <param name="allBuildingsFromLocation"></param>
       /// <returns></returns>
        IEnumerable<string> GetRoomIdsWithConflicts2(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, IEnumerable<string> allBuildingsFromLocation);
        /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="meetingDates"></param>
        /// <param name="allBuildingsFromLocation"></param>
        /// <param name="isMidnight"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetRoomIdsWithConflicts3Async(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, IEnumerable<string> allBuildingsFromLocation, bool isMidnight = false);

    }
}
