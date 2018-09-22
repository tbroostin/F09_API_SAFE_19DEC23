// Copyright 2015 -2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrequencyType = Ellucian.Colleague.Domain.Base.Entities.FrequencyType;

namespace Ellucian.Colleague.Coordination.Base.Tests
{
    [TestClass]
    public class RecurrenceUtilityTests
    {
        private CampusCalendar _campusCalendar;

        [TestInitialize]
        public void Initialize()
        {
            _campusCalendar = BuildCalendars().First();
        }

        private ICollection<CampusCalendar> BuildCalendars()
        {
            ICollection<CampusCalendar> calendars = new List<CampusCalendar>();
            var campusCalendarsData = GetCampusCalendarsData();
            var campusCalendarsCount = campusCalendarsData.Length/5;
            for (var i = 0; i < campusCalendarsCount; i++)
            {
                // Parse out the data
                var id = campusCalendarsData[i, 0].Trim();
                var description = campusCalendarsData[i, 1].Trim();
                var startOfDay = DateTime.Parse(campusCalendarsData[i, 2].Trim());
                var endOfDay = DateTime.Parse(campusCalendarsData[i, 3].Trim());
                var bookPastNoDays = campusCalendarsData[i, 4].Trim();
                calendars.Add(new CampusCalendar(id, description, startOfDay.TimeOfDay, endOfDay.TimeOfDay)
                {
                    BookPastNumberOfDays = int.Parse(bookPastNoDays)
                });
            }
            return calendars;
        }

        /// <summary>
        ///     Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        private static string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable =
            {
                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                {"2015", "2015 Calendar Year", "1/1/2015 12:00:00 AM", "12/31/2015 11:59:00 PM", "9999"},
                {"2016", "2016 Campus Calendar", "1/1/2016 12:00:00 AM", "12/31/2016 11:59:00 PM", "9999"},
                {"2017", "2017 Campus Calendar", "1/1/2017 12:00:00 AM", "12/31/2017 11:59:00 PM", "9999"},
                {"MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM", "1/1/1900 11:59:00 PM", "9999"}
            };
            return campusCalendarsTable;
        }

        [TestMethod]
        public void GetRecurrenceDates_GetDateByOrdinalDay()
        {
            var retval = RecurrenceUtility.GetDateByOrdinalDay(2015, 9, DayOfWeek.Friday, 1);
            Assert.AreEqual(retval.Date, new DateTime(2015, 9, 4));

            //3rd occurrence of a Monday
            var meetingDates = new List<DateTime>();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 10, 19)
            };
            var dayOfWeek = DayOfWeek.Monday;
            var counter = timePeriod.StartOn.Value.DateTime;
            var lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, 3));
                counter = counter.AddMonths(1);
            }
            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 3, 16),
                new DateTime(2015, 4, 20),
                new DateTime(2015, 5, 18),
                new DateTime(2015, 6, 15),
                new DateTime(2015, 7, 20),
                new DateTime(2015, 8, 17),
                new DateTime(2015, 9, 21),
                new DateTime(2015, 10, 19)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);


            //-1st occurrence of a Friday
            meetingDates = new List<DateTime>();
            timePeriod = new RepeatTimePeriod2 {StartOn = new DateTime(2015, 9, 25), EndOn = new DateTime(2016, 5, 27)};
            dayOfWeek = DayOfWeek.Friday;
            counter = timePeriod.StartOn.Value.DateTime;
            lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, -1));
                counter = counter.AddMonths(1);
            }
            meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 25),
                new DateTime(2015, 10, 30),
                new DateTime(2015, 11, 27),
                new DateTime(2015, 12, 25),
                new DateTime(2016, 1, 29),
                new DateTime(2016, 2, 26),
                new DateTime(2016, 3, 25),
                new DateTime(2016, 4, 29),
                new DateTime(2016, 5, 27)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);


            //-3rd occurrence of a Wednesday
            meetingDates = new List<DateTime>();
            timePeriod = new RepeatTimePeriod2 {StartOn = new DateTime(2015, 9, 16), EndOn = new DateTime(2016, 5, 11)};
            dayOfWeek = DayOfWeek.Wednesday;
            counter = timePeriod.StartOn.Value.DateTime;
            lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, -3));
                counter = counter.AddMonths(1);
            }
            meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 16),
                new DateTime(2015, 10, 14),
                new DateTime(2015, 11, 11),
                new DateTime(2015, 12, 16),
                new DateTime(2016, 1, 13),
                new DateTime(2016, 2, 10),
                new DateTime(2016, 3, 16),
                new DateTime(2016, 4, 13),
                new DateTime(2016, 5, 11)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Daily_RepeatRuleDaily_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            RepeatRuleDaily repeatRule = null;
            var frequencyType = FrequencyType.Daily;

            RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Daily_TimePeriod_Null()
        {
            RepeatTimePeriod2 timePeriod = null;
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            var repeatRule = new RepeatRuleDaily {Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            var frequencyType = FrequencyType.Daily;

            RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType, _campusCalendar);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Daily_RepeatRuleEnds_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            RepeatRuleEnds repeatRuleEnds = null;
            var repeatRule = new RepeatRuleDaily {Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            var frequencyType = FrequencyType.Daily;

            RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType, _campusCalendar);
        }

        /// <summary>
        ///     Daily repetition between 2/16 and 2/20/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            var repeatRule = new RepeatRuleDaily {Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 17),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 19),
                new DateTime(2015, 2, 20),
                new DateTime(2015, 2, 21)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Daily repetition between 2/16 and 2/22/15.  Every second day
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Date_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 22)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 22)};
            var repeatRule = new RepeatRuleDaily {Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 2};
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 20),
                new DateTime(2015, 2, 22)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Daily repetition between 2/16 and 2/20/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 20)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 5};
            var repeatRule = new RepeatRuleDaily {Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 17),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 19),
                new DateTime(2015, 2, 20)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Weekly_dayOfWeek_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 10, 2)};
            
            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = null
            };
            var frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Weekly_RepeatRuleEnds_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
           
            var dayOfWeek = new List<HedmDayOfWeek?> {HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday};

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = null,
                Interval = 1,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 10, 2)};
            var dayOfWeek = new List<HedmDayOfWeek?> {HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday};

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 9),
                new DateTime(2015, 9, 11),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 23),
                new DateTime(2015, 9, 25),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(10, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Date_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 10, 2)};
            var dayOfWeek = new List<HedmDayOfWeek?> {HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday};

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 2,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 10};
            var dayOfWeek = new List<HedmDayOfWeek?> {HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday};

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 9),
                new DateTime(2015, 9, 11),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 23),
                new DateTime(2015, 9, 25),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(10, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatRuleEnds_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
           var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 15};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = null,
                Interval = 2,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof (ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatRuleMonthly_RepeatBy_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 6};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 15};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = null
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof (ArgumentException))]
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatRuleMonthly_InvalidDayOfMonth_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 6};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 60};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        /// <summary>
        ///     Monthly repetition, every 2nd month on the 15th, between September 2015 and July 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 6};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 15};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 3, 15),
                new DateTime(2016, 5, 15),
                new DateTime(2016, 7, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every month on the 15th, between September 2015 and February 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Repetitions_2()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 2, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 6};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 15};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 10, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2015, 12, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 2, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every month on the 15th, between September 2015 and February 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 2, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2016, 2, 15)};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfMonth = 15};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 10, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2015, 12, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 2, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every 3rd Monday, between September 2 2015 and October 19 2015
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatBy_Occurrence()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 10, 19)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 10, 19)};
            var repeatRuleDayOfWeek = new RepeatRuleDayOfWeek {Day = HedmDayOfWeek.Monday, Occurrence = 3};
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy {DayOfWeek = repeatRuleDayOfWeek};

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 3, 16),
                new DateTime(2015, 4, 20),
                new DateTime(2015, 5, 18),
                new DateTime(2015, 6, 15),
                new DateTime(2015, 7, 20),
                new DateTime(2015, 8, 17),
                new DateTime(2015, 9, 21),
                new DateTime(2015, 10, 19)
            };

            Assert.AreEqual(9, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16/15 and 2/16/19
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTimeOffset(2015, 2, 16, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                EndOn = new DateTimeOffset(2019, 2, 16, 04, 00, 00, new TimeSpan(-4, 0, 0))
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2019, 2, 16)};
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 1
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2016, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2018, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16 and 2/16/19
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2019, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 5};
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 1
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2016, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2018, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16/2015 and 2/16/2019.  every second year
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2019, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Repetitions = 3};
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 2
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 11/01/2015 and 11/01/2022.  Every 7 years
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions_LargeInterval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 11, 01),
                EndOn = new DateTime(2022, 11, 01)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2022, 11, 01)};
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 7
            };
            var frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 11, 01),
                new DateTime(2022, 11, 01)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }
    }

    [TestClass]
    public class RecurrenceUtilityTests2
    {
        private CampusCalendar _campusCalendar;

        [TestInitialize]
        public void Initialize()
        {
            _campusCalendar = BuildCalendars().First();
        }

        private ICollection<CampusCalendar> BuildCalendars()
        {
            ICollection<CampusCalendar> calendars = new List<CampusCalendar>();
            var campusCalendarsData = GetCampusCalendarsData();
            var campusCalendarsCount = campusCalendarsData.Length / 5;
            for (var i = 0; i < campusCalendarsCount; i++)
            {
                // Parse out the data
                var id = campusCalendarsData[i, 0].Trim();
                var description = campusCalendarsData[i, 1].Trim();
                var startOfDay = DateTime.Parse(campusCalendarsData[i, 2].Trim());
                var endOfDay = DateTime.Parse(campusCalendarsData[i, 3].Trim());
                var bookPastNoDays = campusCalendarsData[i, 4].Trim();
                calendars.Add(new CampusCalendar(id, description, startOfDay.TimeOfDay, endOfDay.TimeOfDay)
                {
                    BookPastNumberOfDays = int.Parse(bookPastNoDays)
                });
            }
            return calendars;
        }

        /// <summary>
        ///     Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        private static string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable =
            {
                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                {"2015", "2015 Calendar Year", "1/1/2015 12:00:00 AM", "12/31/2015 11:59:00 PM", "9999"},
                {"2016", "2016 Campus Calendar", "1/1/2016 12:00:00 AM", "12/31/2016 11:59:00 PM", "9999"},
                {"2017", "2017 Campus Calendar", "1/1/2017 12:00:00 AM", "12/31/2017 11:59:00 PM", "9999"},
                {"MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM", "1/1/1900 11:59:00 PM", "9999"}
            };
            return campusCalendarsTable;
        }

        [TestMethod]
        public void GetRecurrenceDates_GetDateByOrdinalDay()
        {
            var retval = RecurrenceUtility.GetDateByOrdinalDay(2015, 9, DayOfWeek.Friday, 1);
            Assert.AreEqual(retval.Date, new DateTime(2015, 9, 4));

            //3rd occurrence of a Monday
            var meetingDates = new List<DateTime>();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 10, 19)
            };
            var dayOfWeek = DayOfWeek.Monday;
            var counter = timePeriod.StartOn.Value.DateTime;
            var lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, 3));
                counter = counter.AddMonths(1);
            }
            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 3, 16),
                new DateTime(2015, 4, 20),
                new DateTime(2015, 5, 18),
                new DateTime(2015, 6, 15),
                new DateTime(2015, 7, 20),
                new DateTime(2015, 8, 17),
                new DateTime(2015, 9, 21),
                new DateTime(2015, 10, 19)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);


            //-1st occurrence of a Friday
            meetingDates = new List<DateTime>();
            timePeriod = new RepeatTimePeriod2 { StartOn = new DateTime(2015, 9, 25), EndOn = new DateTime(2016, 5, 27) };
            dayOfWeek = DayOfWeek.Friday;
            counter = timePeriod.StartOn.Value.DateTime;
            lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, -1));
                counter = counter.AddMonths(1);
            }
            meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 25),
                new DateTime(2015, 10, 30),
                new DateTime(2015, 11, 27),
                new DateTime(2015, 12, 25),
                new DateTime(2016, 1, 29),
                new DateTime(2016, 2, 26),
                new DateTime(2016, 3, 25),
                new DateTime(2016, 4, 29),
                new DateTime(2016, 5, 27)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);


            //-3rd occurrence of a Wednesday
            meetingDates = new List<DateTime>();
            timePeriod = new RepeatTimePeriod2 { StartOn = new DateTime(2015, 9, 16), EndOn = new DateTime(2016, 5, 11) };
            dayOfWeek = DayOfWeek.Wednesday;
            counter = timePeriod.StartOn.Value.DateTime;
            lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month,
                DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));

            while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
            {
                meetingDates.Add(RecurrenceUtility.GetDateByOrdinalDay(counter.Year, counter.Month, dayOfWeek, -3));
                counter = counter.AddMonths(1);
            }
            meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 16),
                new DateTime(2015, 10, 14),
                new DateTime(2015, 11, 11),
                new DateTime(2015, 12, 16),
                new DateTime(2016, 1, 13),
                new DateTime(2016, 2, 10),
                new DateTime(2016, 3, 16),
                new DateTime(2016, 4, 13),
                new DateTime(2016, 5, 11)
            };

            Assert.AreEqual(9, meetingDates.Count);
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Daily_RepeatRuleDaily_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            RepeatRuleDaily repeatRule = null;
            var frequencyType = FrequencyType.Daily;

            RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Daily_TimePeriod_Null()
        {
            RepeatTimePeriod2 timePeriod = null;
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var frequencyType = FrequencyType.Daily;

            RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType, _campusCalendar);
        }


        /// <summary>
        ///     Daily repetition between 2/16 and 2/20/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 17),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 19),
                new DateTime(2015, 2, 20),
                new DateTime(2015, 2, 21)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Daily repetition between 2/16 and 2/22/15.  Every second day
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Date_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 22)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 22) };
            var repeatRule = new RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 2 };
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 20),
                new DateTime(2015, 2, 22)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Daily repetition between 2/16 and 2/20/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Daily_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 20)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 5 };
            var repeatRule = new RepeatRuleDaily { Type = Dtos.FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            const FrequencyType frequencyType = FrequencyType.Daily;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 2, 17),
                new DateTime(2015, 2, 18),
                new DateTime(2015, 2, 19),
                new DateTime(2015, 2, 20)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Weekly_dayOfWeek_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 10, 2) };

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = null
            };
            var frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }


        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 10, 2) };
            var dayOfWeek = new List<HedmDayOfWeek?> { HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday };

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 9),
                new DateTime(2015, 9, 11),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 23),
                new DateTime(2015, 9, 25),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(10, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Date_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 10, 2) };
            var dayOfWeek = new List<HedmDayOfWeek?> { HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday };

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 2,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Weekly repetition, every Wednedsday and Friday, between 9/2 and 10/2/15
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Weekly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 2),
                EndOn = new DateTime(2015, 10, 2)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 10 };
            var dayOfWeek = new List<HedmDayOfWeek?> { HedmDayOfWeek.Wednesday, HedmDayOfWeek.Friday };

            var repeatRule = new RepeatRuleWeekly
            {
                Type = Dtos.FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = dayOfWeek
            };
            const FrequencyType frequencyType = FrequencyType.Weekly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 2),
                new DateTime(2015, 9, 4),
                new DateTime(2015, 9, 9),
                new DateTime(2015, 9, 11),
                new DateTime(2015, 9, 16),
                new DateTime(2015, 9, 18),
                new DateTime(2015, 9, 23),
                new DateTime(2015, 9, 25),
                new DateTime(2015, 9, 30),
                new DateTime(2015, 10, 2)
            };

            Assert.AreEqual(10, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }



        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatRuleMonthly_RepeatBy_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 6 };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 15 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = null
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatRuleMonthly_InvalidDayOfMonth_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 6 };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 60 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);
        }

        /// <summary>
        ///     Monthly repetition, every 2nd month on the 15th, between September 2015 and July 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 7, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 6 };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 15 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 2,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 3, 15),
                new DateTime(2016, 5, 15),
                new DateTime(2016, 7, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every month on the 15th, between September 2015 and February 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Repetitions_2()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 2, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 6 };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 15 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            const FrequencyType frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 10, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2015, 12, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 2, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every month on the 15th, between September 2015 and February 2016
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 9, 15),
                EndOn = new DateTime(2016, 2, 15)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2016, 2, 15) };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 15 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 9, 15),
                new DateTime(2015, 10, 15),
                new DateTime(2015, 11, 15),
                new DateTime(2015, 12, 15),
                new DateTime(2016, 1, 15),
                new DateTime(2016, 2, 15)
            };

            Assert.AreEqual(6, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, every 3rd Monday, between September 2 2015 and October 19 2015
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatBy_Occurrence()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 10, 19)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 10, 19) };
            var repeatRuleDayOfWeek = new RepeatRuleDayOfWeek { Day = HedmDayOfWeek.Monday, Occurrence = 3 };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfWeek = repeatRuleDayOfWeek };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Ends = repeatRuleEnds,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2015, 3, 16),
                new DateTime(2015, 4, 20),
                new DateTime(2015, 5, 18),
                new DateTime(2015, 6, 15),
                new DateTime(2015, 7, 20),
                new DateTime(2015, 8, 17),
                new DateTime(2015, 9, 21),
                new DateTime(2015, 10, 19)
            };

            Assert.AreEqual(9, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Monthly repetition, 8th day of month
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatBy_DayOfMonth()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2017, 11, 08),
                EndOn = new DateTime(2018, 1, 08)
            };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 8 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2017, 11, 08),
                new DateTime(2017, 12, 08),
                new DateTime(2018, 1, 08)
            };

            Assert.AreEqual(3, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }


        /// <summary>
        ///     Monthly repetition, 8th day of month with a start on after the first expeced occurance
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Monthly_RepeatBy_DayOfMonth_StartOn_AfterFirstDate()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2017, 11, 15),
                EndOn = new DateTime(2018, 2, 15)
            };
            var repeatRuleRepeatBy = new RepeatRuleRepeatBy { DayOfMonth = 8 };

            var repeatRule = new RepeatRuleMonthly
            {
                Type = Dtos.FrequencyType2.Monthly,
                Interval = 1,
                RepeatBy = repeatRuleRepeatBy
            };
            var frequencyType = FrequencyType.Monthly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2017, 12, 08),
                new DateTime(2018, 1, 08),
                new DateTime(2018, 2, 08)
            };

            Assert.AreEqual(3, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16/15 and 2/16/19
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Date()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTimeOffset(2015, 2, 16, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                EndOn = new DateTimeOffset(2019, 2, 16, 04, 00, 00, new TimeSpan(-4, 0, 0))
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2019, 2, 16) };
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 1
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2016, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2018, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16 and 2/16/19
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2019, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 5 };
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 1
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2016, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2018, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(5, meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 2/16/2015 and 2/16/2019.  every second year
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions_Interval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2019, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 3 };
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 2
            };
            const FrequencyType frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 2, 16),
                new DateTime(2017, 2, 16),
                new DateTime(2019, 2, 16)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }

        /// <summary>
        ///     Yearly repetition between 11/01/2015 and 11/01/2022.  Every 7 years
        /// </summary>
        [TestMethod]
        public void GetRecurrenceDates_Yearly_Repetitions_LargeInterval()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 11, 01),
                EndOn = new DateTime(2022, 11, 01)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2022, 11, 01) };
            var repeatRule = new RepeatRuleYearly
            {
                Type = Dtos.FrequencyType2.Yearly,
                Ends = repeatRuleEnds,
                Interval = 7
            };
            var frequencyType = FrequencyType.Yearly;

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(repeatRule, timePeriod, frequencyType,
                _campusCalendar);

            var meetingDatesExpected = new List<DateTime>
            {
                new DateTime(2015, 11, 01),
                new DateTime(2022, 11, 01)
            };

            Assert.AreEqual(meetingDatesExpected.Count(), meetingDates.Count());
            CollectionAssert.AreEqual(meetingDatesExpected, meetingDates.ToList());
        }
    }
}
