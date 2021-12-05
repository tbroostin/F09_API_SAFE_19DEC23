// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestEventRepository : IEventRepository
    {

        public IEnumerable<Event> Get()
        {
            var cals = BuildEvents();
            return cals;
        }

        public IEnumerable<Event> GetEventsWithMidnightUTCTime()
        {
            var cals = BuildEventsWithMidnightTime();
            return cals;
        }


        public Event Get(string id)
        {
            var cals = BuildEvents();
            return cals.Where(cs => cs.Id == id).First();
        }

        public IEnumerable<Event> Get(string type, IEnumerable<string> ids, DateTime? startDate, DateTime? endDate)
        {
            return BuildEvents();
        }

        public CampusCalendar GetCalendar(string calendarId)
        {
            var calendars = BuildCalendars();
            return calendars.Where(c => c.Id == calendarId).First();
        }

        public IEnumerable<string> GetRoomIdsWithConflicts(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, string building, IEnumerable<string> allBuildingsFromLocation)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetRoomIdsWithConflicts2(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, IEnumerable<string> allBuildingsFromLocation)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetRoomIdsWithConflicts3Async(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, IEnumerable<string> allBuildingsFromLocation)
        {
            throw new NotImplementedException();
        }

        private ICollection<Event> BuildEventsWithMidnightTime()
        {
            DateTime current = DateTime.Now;
            var offset = current.ToLocalTime() - current.ToUniversalTime();
            ICollection<Event> events = new List<Event>();

            string secId1 = "100";
            string title1 = "Testing ICal.Net calendar";
            DateTime theDate = new DateTime(2017, 7, 18, 0, 0, 0);

            //Set the time as midnight UTC, comes up as START;VALUE=DATE:20170720 in dday string without adding HasTime

            //theStart and theEnd is required only for the time for the offset
            DateTimeOffset theStart1 = new DateTimeOffset(new DateTime(1001, 1, 1, 20, 0, 0), offset);
            DateTimeOffset theEnd1 = new DateTimeOffset(new DateTime(1001, 1, 1, 20, 30, 0), offset);

            //Also build an all-day event 
            DateTimeOffset allDayStart = new DateTimeOffset(new DateTime(1001, 1, 1, 20, 0, 0), offset);
            DateTimeOffset allDayEnd = new DateTimeOffset(new DateTime(1001, 1, 1, 20, 0, 0), offset);

            var test = BuildDateTimeOffset(theDate, theStart1);

            string room2 = "CONF1*800";

            //Add events that start at midnight UTC
            var event1 = new Event("1", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate, theStart1), BuildDateTimeOffset(theDate, theEnd1));
            event1.AddRoom(room2);
            events.Add(event1);
            var event2 = new Event("2", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(1.0), theStart1), BuildDateTimeOffset(theDate.AddDays(1.0), theEnd1));
            event2.AddRoom(room2);
            events.Add(event2);
            var event3 = new Event("3", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(2.0), theStart1), BuildDateTimeOffset(theDate.AddDays(2.0), theEnd1));
            event3.AddRoom(room2);
            events.Add(event3);

            //Add All-Day events
            var event4 = new Event("4", title1, "FI", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(3.0), theStart1), BuildDateTimeOffset(theDate.AddDays(3.0), theEnd1));
            event4.AddRoom(room2);
            events.Add(event4);
            var event5 = new Event("5", title1, "FI", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(4.0), allDayStart), BuildDateTimeOffset(theDate.AddDays(10.0), allDayEnd.AddHours(3)));
            event2.AddRoom(room2);
            events.Add(event5);

            return events;
        }

        private ICollection<Event> BuildEvents()
        {
            DateTime current = DateTime.Now;
            var offset = current.ToLocalTime() - current.ToUniversalTime();
            ICollection<Event> events = new List<Event>();
            // simple testing data for specific CS (Course Section Meeting) cases
            string secId1 = "1111111";
            string title1 = "CS-120 C# Programming";
            DateTime theDate = new DateTime(2012, 8, 1, 0, 0, 0);
            DateTimeOffset theStart1 = new DateTimeOffset(new DateTime(1968, 1, 1, 7, 0, 0), offset);
            DateTimeOffset theEnd1 = new DateTimeOffset(new DateTime(1968, 1, 1, 7, 50, 0), offset);
            string room2 = "BEND*990";
            // meetings on 8/1, 8/2, 8/3, 8/4, 8/5
            var event1 = new Event("1", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate, theStart1), BuildDateTimeOffset(theDate, theEnd1));
            event1.AddRoom(room2);
            events.Add(event1);
            var event2 = new Event("2", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(1.0), theStart1), BuildDateTimeOffset(theDate.AddDays(1.0), theEnd1));
            event2.AddRoom(room2);
            events.Add(event2);
            var event3 = new Event("3", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(2.0), theStart1), BuildDateTimeOffset(theDate.AddDays(2.0), theEnd1));
            event3.AddRoom(room2);
            events.Add(event3);
            var event4 = new Event("4", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(3.0), theStart1), BuildDateTimeOffset(theDate.AddDays(3.0), theEnd1));
            event4.AddRoom(room2);
            events.Add(event4);
            var event5 = new Event("5", title1, "CS", "MC", secId1, BuildDateTimeOffset(theDate.AddDays(4.0), theStart1), BuildDateTimeOffset(theDate.AddDays(4.0), theEnd1));
            event5.AddRoom(room2);
            events.Add(event5);

            string secId2 = "2222222";
            string title2 = "CS-220 Advanced C# Programming";
            DateTime theStart2 = new DateTime(1968, 1, 1, 8, 0, 0);
            DateTime theEnd2 = new DateTime(1968, 1, 1, 8, 50, 0);
            // meetings on 8/1, 8/2, 8/3
            var event6 = new Event("6", title2, "CS", "MC", secId2, BuildDateTimeOffset(theDate, theStart2), BuildDateTimeOffset(theDate, theEnd2));
            event6.AddRoom(room2);
            events.Add(event6);
            var event7 = new Event("7", title2, "CS", "MC", secId2, BuildDateTimeOffset(theDate.AddDays(1.0), theStart2), BuildDateTimeOffset(theDate.AddDays(1.0), theEnd2));
            event7.AddRoom(room2);
            events.Add(event7);
            var event8 = new Event("8", title2, "CS", "MC", secId2, BuildDateTimeOffset(theDate.AddDays(2.0), theStart2), BuildDateTimeOffset(theDate.AddDays(2.0), theEnd2));
            event8.AddRoom(room2);
            events.Add(event8);

            // third section with no meeting place (building or room)
            string secId3 = "3333333";
            string title3 = "CS-221 Online C# Programming";
            DateTime theStart3 = new DateTime(1968, 1, 1, 14, 0, 0);
            DateTime theEnd3 = new DateTime(1968, 1, 1, 14, 50, 0);
            events.Add(new Event("9", title3, "CS", "MC", secId3, BuildDateTimeOffset(theDate, theStart3), BuildDateTimeOffset(theDate, theEnd3)));
            events.Add(new Event("10", title3, "CS", "MC", secId3, BuildDateTimeOffset(theDate.AddDays(1.0), theStart3), BuildDateTimeOffset(theDate.AddDays(1.0), theEnd3)));
            events.Add(new Event("11", title3, "CS", "MC", secId3, BuildDateTimeOffset(theDate.AddDays(2.0), theStart3), BuildDateTimeOffset(theDate.AddDays(2.0), theEnd3)));

            // simple testing data for specific FI (Faculty Information) cases
            string facId1 = "1111111";
            string facId2 = "2222222";
            string title4 = "Office Hours";
            DateTime date4 = DateTime.Today;
            DateTime start4 = new DateTime(1968, 1, 1, 23, 0, 0);
            DateTime end4 = new DateTime(1968, 1, 1, 23, 50, 0);
            string room3 = "BEND*995";
            string room4 = "BEND*996";
            var events12 = new Event("12", title4, "FI", "MC", facId1, BuildDateTimeOffset(date4, start4), BuildDateTimeOffset(theDate, end4));
            events12.AddRoom(room3);
            events.Add(events12);
            var events13 = new Event("13", title4, "FI", "MC", facId1, BuildDateTimeOffset(date4.AddDays(1.0), start4), BuildDateTimeOffset(theDate, end4));
            events13.AddRoom(room3);
            events.Add(events13);
            var events14 = new Event("14", title4, "FI", "MC", facId1, BuildDateTimeOffset(date4.AddDays(2.0), start4), BuildDateTimeOffset(theDate, end4));
            events14.AddRoom(room3);
            events.Add(events14);
            var events15 = new Event("15", title4, "FI", "MC", facId2, BuildDateTimeOffset(date4, start4), BuildDateTimeOffset(theDate, end4));
            events15.AddRoom(room4);
            events.Add(events15);
            var events16 = new Event("16", title4, "FI", "MC", facId2, BuildDateTimeOffset(date4.AddDays(1.0), start4), BuildDateTimeOffset(theDate, end4));
            events16.AddRoom(room4);
            events.Add(events16);

            // conflict checking test data
            string room5 = "MOR*201";
            var events17 = new Event("17", title4, "FI", "MC", facId2, BuildDateTimeOffset(date4, start4.AddHours(1)), BuildDateTimeOffset(date4, end4.AddHours(2)));
            events17.AddRoom(room5);
            events.Add(events17);
            string room6 = "MOR*202";
            var events18 = new Event("18", title4, "FI", "MC", facId2, BuildDateTimeOffset(date4, start4.AddHours(2)), BuildDateTimeOffset(date4, end4.AddHours(3)));
            events18.AddRoom(room6);
            events.Add(events18);

            return events;
        }

        private DateTimeOffset BuildDateTimeOffset(DateTime date, DateTimeOffset time)
        {
            return new DateTimeOffset(new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second), time.Offset);
        }

        private ICollection<CampusCalendar> BuildCalendars()
        {
            ICollection<CampusCalendar> calendars = new List<CampusCalendar>();
            string[,] campusCalendarsData = GetCampusCalendarsData();
            int campusCalendarsCount = campusCalendarsData.Length / 5;
            for (int i = 0; i < campusCalendarsCount; i++)
            {
                // Parse out the data
                string id = campusCalendarsData[i, 0].Trim();
                string description = campusCalendarsData[i, 1].Trim();
                DateTime startOfDay = DateTime.Parse(campusCalendarsData[i, 2].Trim());
                DateTime endOfDay = DateTime.Parse(campusCalendarsData[i, 3].Trim());
                string bookPastNoDays = campusCalendarsData[i, 4].Trim();
                calendars.Add(new CampusCalendar(id, description, startOfDay.TimeOfDay, endOfDay.TimeOfDay) { BookPastNumberOfDays = int.Parse(bookPastNoDays) });
            }
            return calendars;
        }

        /// <summary>
        /// Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        private static string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable = {
                                                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                                                { "2001", "2001 Calendar Year",     "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "30"},
                                                { "2004", "2004 Campus Calendar",   "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                                { "2005", "2005 Campus Calendar",   "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                                { "MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                             };
            return campusCalendarsTable;
        }

        public Task<IEnumerable<string>> GetRoomIdsWithConflicts3Async(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, IEnumerable<string> allBuildingsFromLocation, bool isMidnight = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CampusCalendar>> GetCalendarsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
