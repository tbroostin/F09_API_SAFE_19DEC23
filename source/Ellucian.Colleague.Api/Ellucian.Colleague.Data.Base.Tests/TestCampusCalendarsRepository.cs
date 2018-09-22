using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public class TestCampusCalendarsRepository
    {
        private  Collection<CampusCalendar> _campusCalendars = new Collection<CampusCalendar>();
        public  Collection<CampusCalendar> CampusCalendars
        {
            get
            {
                if (_campusCalendars.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _campusCalendars;
            }
        }

        /// <summary>
        /// Performs data setup for campus calendars to be used in tests
        /// </summary>
        private  void GenerateDataContracts()
        {
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

                CampusCalendar campusCalendar = new CampusCalendar()
                {
                    Recordkey = id,
                    CmpcDesc = description,
                    CmpcDayStartTime = startOfDay,
                    CmpcDayEndTime = endOfDay,
                    CmpcBookPastNoDays = bookPastNoDays,
                    CmpcSchedules = new List<string>(),
                    CmpcSpecialDays = new List<string>()
                };

                string[,] campusCalendarBookedEventDaysData = GetCampusCalendarBookedEventDayData();
                int bookedEventCount = campusCalendarBookedEventDaysData.Length / 2;
                List<string> bookedEvents = new List<string>();
                for (int j = 0; j < bookedEventCount; j++)
                {
                    if (campusCalendarBookedEventDaysData[j, 0].Trim() == campusCalendar.Recordkey)
                    {
                        string scheduleId = campusCalendarBookedEventDaysData[j, 1].Trim();
                        bookedEvents.Add(scheduleId);
                    }
                }

                if (bookedEvents != null && bookedEvents.Count > 0)
                {
                    campusCalendar.CmpcSchedules.AddRange(bookedEvents);
                }
                _campusCalendars.Add(campusCalendar);
            }
        }

        /// <summary>
        /// Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        public  string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable = {
                                                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                                                { "2001", "2001 Calendar Year",     "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "30"},
                                                { "2004", "2004 Campus Calendar",   "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                                { "2005", "2005 Campus Calendar",   "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                                { "MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:00 PM", "9999"},
                                                { "HOLIDAY", "Holiday Calendar",    "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:59 PM", "20"},
                                                { "SPECIALDAYS", "Extra special days", "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:59 PM", "20"},
                                                { "SNOW", "Snow Days",              "1/1/1900 12:00:00 AM",  "1/1/1900 11:59:59 PM", "20"},
                                             };
            return campusCalendarsTable;
        }



        /// <summary>
        /// Gets campus calendar special day raw data
        /// </summary>
        /// <returns>String array of campus calendar special day data</returns>
        public  string[,] GetCampusCalendarBookedEventDayData()
        {
            string[,] campusCalendarBookedEventDayData = { // CalID  SchedId
                                                        {"HOLIDAY", "596675" },
                                                        {"HOLIDAY", "596676" },
                                                        {"HOLIDAY", "596677" },
                                                        {"HOLIDAY", "596678" },
                                                        {"SPECIALDAYS", "596675" },
                                                        {"SPECIALDAYS", "596676" },
                                                        {"SPECIALDAYS", "596677" },
                                                        {"SPECIALDAYS", "596678" },
                                                        {"SNOW", "596675" },
                                                        {"SNOW", "596676" },
                                                        {"SNOW", "596677" },
                                                        {"SNOW", "596678" },
                                                        {"2001","596675"},
                                                        {"2001","596676"},
                                                        {"2001","596677"},
                                                        {"2001","596678"},
                                                        {"2001","596679"},
                                                        {"2001","596680"},
                                                        {"2001","596681"},
                                                        {"2001","596682"},
                                                        {"2001","596683"},
                                                        {"2001","596684"},
                                                        {"2001","596685"},
                                                        {"2001","596686"},
                                                        {"2001","596687"},
                                                        {"2001","596688"},
                                                        {"2001","596689"},
                                                        {"2001","596690"},
                                                        {"2001","596691"},
                                                        {"2001","596692"},
                                                        {"2001","596693"},
                                                        {"2001","596694"},
                                                        {"2001","596695"},
                                                        {"2001","596696"},
                                                        {"2001","596697"},
                                                        {"2001","596698"},
                                                        {"2001","596699"},
                                                        {"2001","596700"},
                                                        {"MAIN","51118"},
                                                        {"MAIN","51119"},
                                                        {"MAIN","51120"},
                                                        {"MAIN","51121"},
                                                        {"MAIN","51122"},
                                                        {"MAIN","51123"},
                                                        {"MAIN","51124"},
                                                        {"MAIN","51125"},
                                                        {"MAIN","51126"},
                                                        {"MAIN","51127"},
                                                        {"MAIN","51128"},
                                                        {"MAIN","51129"},
                                                        {"MAIN","51130"},
                                                        {"MAIN","51131"},
                                                        {"MAIN","51132"},
                                                        {"MAIN","51133"},
                                                        {"MAIN","51134"},
                                                        {"MAIN","51135"},
                                                        {"MAIN","51136"},
                                                        {"MAIN","51137"},
                                                        {"MAIN","51138"},
                                                        {"MAIN","51139"},
                                                        {"MAIN","51140"},
                                                        {"MAIN","51141"},
                                                        {"MAIN","51142"},
                                                        {"MAIN","51143"},
                                                        {"MAIN","51144"},
                                                        {"MAIN","51145"},
                                                        {"MAIN","51146"},
                                                        {"MAIN","51147"},
                                                        {"MAIN","51148"},
                                                        {"MAIN","51149"},
                                                        {"MAIN","51150"},
                                                        {"MAIN","51151"},
                                                        {"MAIN","51152"},
                                                        {"MAIN","51153"},
                                                        {"MAIN","51154"},
                                                        {"MAIN","51155"},
                                                        {"MAIN","51156"},
                                                        {"MAIN","51157"},
                                                        {"MAIN","51158"},
                                                        {"MAIN","51159"},
                                                        {"MAIN","51160"},
                                                        {"MAIN","51161"},
                                                        {"MAIN","51162"},
                                                        {"MAIN","51163"},
                                                        {"MAIN","51164"},
                                                        {"MAIN","51165"},
                                                        {"MAIN","51166"},
                                                        {"MAIN","51167"}
                                                     };
            return campusCalendarBookedEventDayData;
        }
    }
}
