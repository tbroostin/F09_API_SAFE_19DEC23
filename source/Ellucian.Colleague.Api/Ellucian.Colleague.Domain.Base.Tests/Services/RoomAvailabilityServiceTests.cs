// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Domain.Base.Tests.Services
{
    [TestClass]
    public class RoomAvailabilityServiceTests
    {
        static DateTime pastStartDate;
        static DateTime pastEndDate;
        static DateTime startDate;
        static DateTime endDate;
        static Dtos.Recurrence recurrencePattern;
        static List<Base.Entities.Room> rooms;
        static List<Dtos.Occupancy> occupancies;
        static List<DateTime> specialDays;
        static List<DateTime> specialDays2;
        static int backDatingLimit;

        [TestInitialize]
        public void Initialize()
        {
            pastStartDate = DateTime.Parse("01/01/2007");
            pastEndDate = DateTime.Parse("5/31/2007");

            rooms = new List<Base.Entities.Room>()
            {
                new Base.Entities.Room("b83ee390-0a91-4e24-9ec4-4b878805baa3","ALU*100","ALU Room 100") { Capacity = 150 },
                new Base.Entities.Room("f8c94473-b682-4677-9b28-df539d94eef2","ARM*100","ARM Room 100") { Capacity = 30 }
            };

            recurrencePattern = new Dtos.Recurrence() { Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, Frequency = Dtos.FrequencyType.Daily, Interval = 1 };
            occupancies = new List<Dtos.Occupancy>() 
            { 
                new Dtos.Occupancy() 
                { 
                    MaximumOccupancy = 100, 
                    RoomLayoutType = Dtos.RoomLayoutType.Classroom 
                }, 
                new Dtos.Occupancy() 
                { 
                    MaximumOccupancy = 50, 
                    RoomLayoutType = Dtos.RoomLayoutType.Classroom 
                } 
            };

            specialDays = new List<DateTime>()
            {
                DateTime.Parse("01/01/2007"),
                DateTime.Parse("01/15/2007"),
                DateTime.Parse("02/19/2007"),
                DateTime.Parse("03/26/2007"),
                DateTime.Parse("05/28/2007"),
                DateTime.Parse("07/04/2007"),
                DateTime.Parse("09/03/2007"),
                DateTime.Parse("10/08/2007"),
                DateTime.Parse("11/12/2007"),
                DateTime.Parse("11/19/2007"),
                DateTime.Parse("12/25/2007"),
                DateTime.Parse("01/01/2008"),
                DateTime.Parse("01/21/2008"),
                DateTime.Parse("02/18/2008"),
                DateTime.Parse("03/24/2008"),
                DateTime.Parse("03/25/2008"),
                DateTime.Parse("03/26/2008"),
                DateTime.Parse("03/27/2008"),
                DateTime.Parse("03/28/2008"),
                DateTime.Parse("05/26/2008"),
                DateTime.Parse("07/04/2008"),
                DateTime.Parse("09/01/2008"),
                DateTime.Parse("10/13/2008"),
                DateTime.Parse("11/11/2008"),
                DateTime.Parse("11/24/2008"),
                DateTime.Parse("12/25/2008"),
                DateTime.Parse("01/01/2009"),
                DateTime.Parse("01/19/2009"),
                DateTime.Parse("02/16/2009"),
                DateTime.Parse("03/23/2009"),
                DateTime.Parse("03/24/2009"),
                DateTime.Parse("03/25/2009"),
                DateTime.Parse("03/26/2009"),
                DateTime.Parse("03/27/2009"),
                DateTime.Parse("05/25/2009"),
                DateTime.Parse("07/03/2009"),
                DateTime.Parse("09/07/2009"),
                DateTime.Parse("10/12/2009"),
                DateTime.Parse("11/11/2009"),
                DateTime.Parse("11/23/2009"),
                DateTime.Parse("12/25/2009"),
                DateTime.Parse("01/01/2010"),
                DateTime.Parse("01/18/2010"),
                DateTime.Parse("02/15/2010"),
                DateTime.Parse("05/31/2010"),
                DateTime.Parse("07/05/2010"),
                DateTime.Parse("09/06/2010"),
                DateTime.Parse("10/11/2010"),
                DateTime.Parse("11/11/2010"),
                DateTime.Parse("11/22/2010")
            };

            startDate = DateTime.Today;
            endDate = DateTime.Today.AddDays(30);
            specialDays2 = new List<DateTime>()
            {
                DateTime.Today.AddDays(-21),
                DateTime.Today.AddDays(14)
            };

            backDatingLimit = 9999;
        }

        [TestClass]
        public class RoomAvailabilityService_GetRoomsWithCapacity : RoomAvailabilityServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomAvailabilityService_GetRoomsWithCapacity_NullRooms()
            {
                var result = RoomAvailabilityService.GetRoomsWithCapacity(null, 100);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomAvailabilityService_GetRoomsWithCapacity_EmptyRooms()
            {
                var result = RoomAvailabilityService.GetRoomsWithCapacity(new List<Base.Entities.Room>(), 100);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RoomAvailabilityService_GetRoomsWithCapacity_NegativeOccupancy()
            {
                var result = RoomAvailabilityService.GetRoomsWithCapacity(rooms, -1);
            }

            [TestMethod]
            public void RoomAvailabilityService_GetRoomsWithCapacity_ValidOccupancy()
            {
                var result = RoomAvailabilityService.GetRoomsWithCapacity(rooms, 50);
                Assert.AreEqual(1, result.Count());
            }
        }

        [TestClass]
        public class RoomAvailabilityService_BuildDateListTests : RoomAvailabilityServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomAvailabilityService_BuildDateList_RecurrencePatternWithNullDaysOfWeek()
            {
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, pastEndDate, ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(recurrencePattern.Frequency), recurrencePattern.Interval, null, specialDays, backDatingLimit);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomAvailabilityService_BuildDateList_RecurrencePatternWithEmptyDaysOfWeek()
            {
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, pastEndDate, ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(recurrencePattern.Frequency), recurrencePattern.Interval, new List<DayOfWeek>(), specialDays, backDatingLimit);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RoomAvailabilityService_BuildDateList_NegativeInterval()
            {
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, pastEndDate, ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(recurrencePattern.Frequency), -1, recurrencePattern.Days, specialDays, backDatingLimit);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RoomAvailabilityService_BuildDateList_ZeroInterval()
            {
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, pastEndDate, ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(recurrencePattern.Frequency), 0, recurrencePattern.Days, specialDays, backDatingLimit);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RoomAvailabilityService_BuildDateList_NegativeBackDatingLimit()
            {
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, pastEndDate, ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(recurrencePattern.Frequency), recurrencePattern.Interval, recurrencePattern.Days, specialDays, -5);
            }

            [TestMethod]
            public void RoomAvailabilityService_BuildDateList_Daily()
            {
                var expectedDates = GetDailyExpectedDates(DateTime.Today.AddDays(-30), endDate, recurrencePattern.Days, specialDays2).ToList();
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, endDate, FrequencyType.Daily, recurrencePattern.Interval, recurrencePattern.Days, specialDays2, 30).ToList();
                CollectionAssert.AreEqual(expectedDates, result);
            }

            [TestMethod]
            public void RoomAvailabilityService_BuildDateList_Weekly()
            {
                var expectedDates = GetDailyExpectedDates(DateTime.Today, endDate, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, specialDays2).ToList();
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, endDate, FrequencyType.Weekly, recurrencePattern.Interval, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, specialDays2, 0).ToList();
                CollectionAssert.AreEqual(expectedDates, result);
            }

            [TestMethod]
            public void RoomAvailabilityService_BuildDateList_Monthly()
            {
                var expectedDates = new List<DateTime>() { DateTime.Parse("11/03/2014"), DateTime.Parse("11/05/2014") };
                var result = RoomAvailabilityService.BuildDateList(DateTime.Parse("11/01/2014"), DateTime.Parse("11/30/2014"), FrequencyType.Monthly, 1, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday }, null, 9999).ToList();
                CollectionAssert.AreEqual(expectedDates, result);
            }

            [TestMethod]
            public void RoomAvailabilityService_BuildDateList_Yearly()
            {
                var expectedDates = new List<DateTime>() { DateTime.Parse("11/03/2014"), DateTime.Parse("11/05/2014"), DateTime.Parse("11/03/2016"), DateTime.Parse("11/05/2016") };
                var result = RoomAvailabilityService.BuildDateList(DateTime.Parse("11/01/2014"), DateTime.Parse("11/30/2017"), FrequencyType.Yearly, 2, new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday }, null, 9999).ToList();
                CollectionAssert.AreEqual(expectedDates, result);
            }

            [TestMethod]
            public void RoomAvailabilityService_BuildDateList_NoSpecialDays()
            {
                var expectedDates = GetDailyExpectedDates(DateTime.Today, endDate, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, null).ToList();
                var result = RoomAvailabilityService.BuildDateList(pastStartDate, endDate, FrequencyType.Weekly, recurrencePattern.Interval, new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }, null, 0).ToList();
                CollectionAssert.AreEqual(expectedDates, result);
            }
        }

        /// <summary>
        /// Converts a FrequencyType DTO enumeration value to its corresponding FrequencyType Domain enumeration value
        /// </summary>
        /// <param name="type">FrequencyType DTO enum value</param>
        /// <returns>FrequencyType Domain enum value</returns>
        private Ellucian.Colleague.Domain.Base.Entities.FrequencyType ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(Dtos.FrequencyType type)
        {
            switch (type)
            {
                case Dtos.FrequencyType.Weekly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly;
                case Dtos.FrequencyType.Monthly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly;
                case Dtos.FrequencyType.Yearly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly;
                default:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily;
            }
        }

        /// <summary>
        /// Gets days of week for a date range
        /// </summary>
        /// <param name="startDate">First date</param>
        /// <param name="endDate">Last date</param>
        /// <param name="dayOfWeek">Day of week</param>
        /// <returns></returns>
        private IEnumerable<DateTime> GetDailyExpectedDates(DateTime startDate, DateTime endDate, IEnumerable<DayOfWeek> daysOfWeek, IEnumerable<DateTime> daysToRemove)
        {
            int numberOfDays = endDate.Subtract(startDate).Days + 1;
            var dates = Enumerable.Range(0, numberOfDays)
                                  .Select(i => startDate.AddDays(i))
                                  .Where(d => daysOfWeek.Contains(d.DayOfWeek)).ToList();
            if (daysToRemove != null && daysToRemove.Count() > 0)
            {
                foreach (var date in daysToRemove)
                {
                    dates.Remove(date);
                }
            }
            return dates;
        }
    }
}
