// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Base.Tests;
using slf4net;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class EventRepositoryTests : BaseRepositorySetup
    {

        IEnumerable<Event> allCals;
        Collection<CalendarSchedules> calsResponseData;
        Collection<CalendarSchedules> cs4ResponseData;
        Collection<CalendarSchedules> fi4ResponseData;
        Collection<CalendarSchedules> section1first3Data;
        Collection<CalendarSchedules> section1last2Data;
        Collection<CalendarSchedules> section2first3Data;
        Collection<CalendarSchedules> section2last2Data;
        Collection<CalendarSchedules> section3first3Data;
        Collection<CalendarSchedules> roomsWithConflictsData;
        Collection<CalendarSchedules> roomsWithConflictsData2;
        EventRepository eventsRepo;

        public Collection<Ellucian.Colleague.Data.Base.DataContracts.CampusCalendar> campusCalendars;// = TestCampusCalendarsRepository.CampusCalendars;
        public Collection<Ellucian.Colleague.Data.Base.DataContracts.CalendarSchedules> calendarSchedules;// = TestCalendarSchedulesRepository.CalendarSchedules;
        public Collection<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> intlParams = TestIntlParamsRepository.IntlParams;

        public TestCampusCalendarsRepository campusCalendarsTestData;
        public TestCalendarSchedulesRepository calendarSchedulesTestData;
        public TestSpecialDaysRepository specialDaysTestData;


        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");
            colleagueTimeZone = apiSettings.ColleagueTimeZone;

            campusCalendarsTestData = new TestCampusCalendarsRepository();
            calendarSchedulesTestData = new TestCalendarSchedulesRepository();
            specialDaysTestData = new TestSpecialDaysRepository();

            campusCalendars = campusCalendarsTestData.CampusCalendars;
            calendarSchedules = calendarSchedulesTestData.CalendarSchedules;

            foreach (var calendar in campusCalendars)
            {
                calendar.CmpcSpecialDays = specialDaysTestData.specialDayRecords
                    .Where(sd => sd.CmsdCampusCalendar == calendar.Recordkey)
                    .Select(sd => sd.Recordkey).ToList();
            }

            allCals = new TestEventRepository().Get();
            calsResponseData = BuildCalendarSchedulesResponse(allCals);
            IEnumerable<Event> cs4 = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "4").AsEnumerable();
            cs4ResponseData = BuildCalendarSchedulesResponse(cs4);
            IEnumerable<Event> oh4 = allCals.Where(cs => cs.Type == "FI" && cs.Pointer == "4").AsEnumerable();
            fi4ResponseData = BuildCalendarSchedulesResponse(oh4);
            DateTime the4th = new DateTime(2012, 8, 4);
            /* test data setup
             * section 1111111 meets 8/1, 8/2, 8/3, 8/4, 8/5
             * section 2222222 meets 8/1, 8/2, 8/3
             * section 3333333 meets 8/1, 8/2, 8/3
            */
            IEnumerable<Event> s1f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111" && cs.Start < the4th).AsEnumerable();
            section1first3Data = BuildCalendarSchedulesResponse(s1f3days);
            IEnumerable<Event> s1l2days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111" && cs.Start >= the4th).AsEnumerable();
            section1last2Data = BuildCalendarSchedulesResponse(s1l2days);
            IEnumerable<Event> s2f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "2222222" && cs.Start < the4th).AsEnumerable();
            section2first3Data = BuildCalendarSchedulesResponse(s2f3days);
            IEnumerable<Event> s2l2days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "2222222" && cs.Start >= the4th).AsEnumerable();
            section2last2Data = BuildCalendarSchedulesResponse(s2l2days);
            IEnumerable<Event> s3f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "3333333" && cs.Start < the4th).AsEnumerable();
            section3first3Data = BuildCalendarSchedulesResponse(s3f3days);

            // room conflict checking test data
            IEnumerable<Event> conflictDays = allCals.Where(cs => cs.Id == "17" || cs.Id == "18");
            roomsWithConflictsData = BuildCalendarSchedulesResponse(conflictDays);
            
            IEnumerable<Event> conflictDays2 = allCals.Where(cs => cs.Id == "18");
            roomsWithConflictsData2 = BuildCalendarSchedulesResponse(conflictDays2);
            
            eventsRepo = BuildEventRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            allCals = null;
            calsResponseData = null;
            eventsRepo = null;
            section1first3Data = null;
            section1last2Data = null;
            section2first3Data = null;
            section2last2Data = null;
            section3first3Data = null;

        }

        [TestClass]
        public class EventRepository_GetTests : EventRepositoryTests
        {
            [TestMethod]
            public void EventRepository_Get_OneSectionFirstThreeDays()
            {
                List<string> secs = new List<string>();
                secs.Add("1111111");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 1));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 3));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                // make sure the setup is correct
                Assert.AreEqual(3, section1first3Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section1first3Data.Count(), cals.Count());
            }

            [TestMethod]
            public void EventRepository_Get_OneSectionLastTwoDays()
            {
                List<string> secs = new List<string>();
                secs.Add("1111111");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 4));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 5));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                Assert.AreEqual(2, section1last2Data.Count());
                Assert.AreEqual(section1last2Data.Count(), cals.Count());
            }

            [TestMethod]
            public void EventRepository_Get_TwoSectionsFirstThreeDays()
            {
                List<string> secs = new List<string>();
                secs.Add("1111111"); secs.Add("2222222");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 1));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 3));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                Assert.AreEqual(3, section1first3Data.Count());
                Assert.AreEqual(3, section2first3Data.Count());
                Assert.AreEqual(section1first3Data.Count() + section2first3Data.Count(), cals.Count());
            }

            [TestMethod]
            public void EventRepository_Get_TwoSectionsLastTwoDays()
            {
                List<string> secs = new List<string>();
                secs.Add("1111111"); secs.Add("2222222");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 4));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 5));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                Assert.AreEqual(2, section1last2Data.Count());
                Assert.AreEqual(0, section2last2Data.Count());
                Assert.AreEqual(section1last2Data.Count() + section2last2Data.Count(), cals.Count());
            }

            [TestMethod]
            public void EventRepository_Get_EventsWithNoBldgOrRoom()
            {
                List<string> secs = new List<string>();
                secs.Add("3333333");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 1));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 3));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                // make sure the setup is correct
                Assert.AreEqual(3, section3first3Data.Count());
                // make sure we return the correct cals count
                Assert.AreEqual(section3first3Data.Count(), cals.Count());
            }

            [TestMethod]
            public void EventRepository_Get_ValidSingleCals()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Description, check.Description);
            }

            [TestMethod]
            public void EventRepository_Get_InvalidSingleCals()
            {
                string calsId = "9999999999";
                Event cal = eventsRepo.Get(calsId);
                Assert.IsNull(cal);
            }

            [TestMethod]
            public void EventRepository_Get_Id()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Id, check.Id);
            }

            [TestMethod]
            public void EventRepository_Get_Type()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Type, check.Type);
            }

            [TestMethod]
            public void EventRepository_Get_Pointer()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Pointer, check.Pointer);
            }

            [TestMethod]
            public void EventRepository_Get_Start()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Start, check.Start);
            }

            [TestMethod]
            public void EventRepository_Get_End()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.End, check.End);
            }

            [TestMethod]
            public void EventRepository_Get_Location()
            {
                string calsId = "1";
                Event cal = eventsRepo.Get(calsId);
                Event check = allCals.Where(cs => cs.Id == calsId).First();
                Assert.AreEqual(cal.Location, check.Location);
            }

            [TestMethod]
            public void EventRepository_Get_SkipsItemWithNoDateValue()
            {
                DateTime the4th = new DateTime(2012, 8, 4);
                IEnumerable<Event> s1f3days = allCals.Where(cs => cs.Type == "CS" && cs.Pointer == "1111111" && cs.Start < the4th).AsEnumerable();
                var section1First3WithBogus = BuildCalendarSchedulesResponse(s1f3days);
                // Set up response for this request to include an additional item
                var repoCal1 = new CalendarSchedules();
                repoCal1.CalsBuildings = new List<string>() { "DORM" };
                repoCal1.CalsRooms = new List<string>() { "101" };
                repoCal1.Recordkey = "TEST101A";
                repoCal1.CalsDate = null;
                repoCal1.CalsDescription = "BAD TEST";
                repoCal1.CalsLocation = "MAIN";
                repoCal1.CalsStartTime = new DateTime(2008, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                repoCal1.CalsEndTime = new DateTime(2008, 1, 1, DateTime.Now.AddHours(1.0).Hour, DateTime.Now.Minute, DateTime.Now.Second);
                repoCal1.CalsPointer = "12";
                repoCal1.CalsType = "FI";
                repoCal1.CalsBldgRoomEntityAssociation = new List<CalendarSchedulesCalsBldgRoom>();
                section1First3WithBogus.Add(repoCal1);
                var repoCal2 = new CalendarSchedules();
                repoCal2.CalsBuildings = new List<string>() { "DORM" };
                repoCal2.CalsRooms = new List<string>() { "101" };
                repoCal2.Recordkey = "TEST101A";
                repoCal2.CalsDate = new DateTime(1968,1,1);
                repoCal2.CalsDescription = "BAD TEST";
                repoCal2.CalsLocation = "MAIN";
                repoCal2.CalsStartTime = new DateTime(2008, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                repoCal2.CalsEndTime = new DateTime(2008, 1, 1, DateTime.Now.AddHours(1.0).Hour, DateTime.Now.Minute, DateTime.Now.Second);
                repoCal2.CalsPointer = "12";
                repoCal2.CalsType = "FI";
                repoCal2.CalsBldgRoomEntityAssociation = new List<CalendarSchedulesCalsBldgRoom>();
                section1First3WithBogus.Add(repoCal2);
                dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string>(), true)).Returns(section1First3WithBogus);

                List<string> secs = new List<string>();
                secs.Add("1111111");
                DateTime? startDate = new DateTime?(new DateTime(2012, 8, 1));
                DateTime? endDate = new DateTime?(new DateTime(2012, 8, 3));
                IEnumerable<Event> cals = eventsRepo.Get("CS", secs, startDate, endDate);
                // make sure the setup is correct
                Assert.AreEqual(5, section1First3WithBogus.Count());
                // make sure we return the 3 items, without the 4th bogus one
                Assert.AreEqual(3, cals.Count()); 
            }
        }

        [TestClass]
        public class EventRepository_GetCalendar : EventRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetCalendar()
            {
                base.Initialize();
                SetupCampusCalendars();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetCalendar_NullCalendarId()
            {
                var result = eventsRepo.GetCalendar(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetCalendar_EmptyCalendarId()
            {
                var result = eventsRepo.GetCalendar(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void EventRepository_GetCalendar_InvalidCalendarId()
            {
                var result = eventsRepo.GetCalendar("INVALID_ID");
            }

            [TestMethod]
            public void EventRepository_GetCalendar_ValidCalendarId()
            {
                var result = eventsRepo.GetCalendar("MAIN");
                var calendar = campusCalendars.First(cc => cc.Recordkey == "MAIN");
                var startTime = calendar.CmpcDayStartTime.Value.TimeOfDay;
                var endTime = calendar.CmpcDayEndTime.Value.TimeOfDay;
                var specialDayDates = calendarSchedules.Where(cs => cs.CalsCampusCalendar == calendar.Recordkey).Select(c => c.CalsDate).ToList();
                Assert.AreEqual(calendar.Recordkey, result.Id);
                Assert.AreEqual(calendar.CmpcDesc, result.Description);
                Assert.AreEqual(startTime, result.DefaultStartOfDay);
                Assert.AreEqual(endTime, result.DefaultEndOfDay);
                Assert.AreEqual(int.Parse(calendar.CmpcBookPastNoDays), result.BookPastNumberOfDays);
                Assert.AreEqual(calendar.CmpcSchedules.Count(), result.BookedEventDates.Count);
                for (int i = 0; i < result.BookedEventDates.Count; i++)
                {
                    Assert.AreEqual(specialDayDates[i], result.BookedEventDates[i]);
                }
            }

            [TestMethod]
            public void EventRepository_GetCalendar_VerifyCache()
            {
                // Verify that after we get the configuration, it's stored in the cache
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                string cacheKey = eventsRepo.BuildFullCacheKey("CampusCalendar_MAIN");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<Domain.Base.Entities.CampusCalendar>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the configuration
                var result = eventsRepo.GetCalendar("MAIN");

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<Domain.Base.Entities.CampusCalendar>(), It.IsAny<CacheItemPolicy>(), null));
            }
        }

        [TestClass]
        public class EventRepository_GetRoomIdsWithConflicts : EventRepositoryTests
        {
            DateTime conflictDate = DateTime.Today;

            [TestInitialize]
            public void Initialize_GetRoomIdsWithConflicts()
            {
                base.Initialize();
                SetupCampusCalendars();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetRoomIdsWithConflicts_NullMeetingDates()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(conflictDate, conflictDate.AddHours(2), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetRoomIdsWithConflicts_EmptyMeetingDates()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(conflictDate, conflictDate.AddHours(2), new List<DateTime>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EventRepository_GetRoomIdsWithConflicts_InvalidTimeRange()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(conflictDate, conflictDate.AddHours(-1), new List<DateTime> { conflictDate });
            }

            [TestMethod]
            public void EventRepository_GetRoomIdsWithConflicts_ValidTimeAndDates_RoomsListedOnConflicts()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(new DateTimeOffset(conflictDate.AddHours(1)), new DateTimeOffset(conflictDate.AddHours(2)), new List<DateTime>() { conflictDate, conflictDate.AddDays(1) });
                var rooms = roomsWithConflictsData.SelectMany(r => r.CalsBldgRoomEntityAssociation).ToList();
                Assert.AreEqual(2, result.Count());
                for (int i = 0; i < result.Count(); i++)
                {
                    Assert.AreEqual(rooms.ToList()[i].CalsBuildingsAssocMember + "*" + rooms.ToList()[i].CalsRoomsAssocMember, result.ToList()[i]);
                }
            }

            [TestMethod]
            public void EventRepository_GetRoomIdsWithConflicts_ValidTimeAndDates_NoRoomsListedOnConflicts()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(new DateTimeOffset(conflictDate.AddHours(3)), new DateTimeOffset(conflictDate.AddHours(4)), new List<DateTime>() { conflictDate, conflictDate.AddDays(1) });
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void EventRepository_GetRoomIdsWithConflicts_NoConflictsInDateRange()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts(conflictDate, conflictDate.AddHours(1), new List<DateTime>() { conflictDate.AddDays(1), conflictDate.AddDays(2) });
                Assert.AreEqual(0, result.Count());
            }
        }

        [TestClass]
        public class EventRepository_GetRoomIdsWithConflicts2 : EventRepositoryTests
        {
            DateTime conflictDate = DateTime.Today;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> allRoomEntities;

            [TestInitialize]
            public void Initialize_GetRoomIdsWithConflicts2()
            {
                base.Initialize();
                SetupCampusCalendars();
                allRoomEntities = new TestRoomRepository().Get();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetRoomIdsWithConflicts2_NullMeetingDates()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts2(conflictDate, conflictDate.AddHours(2),null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EventRepository_GetRoomIdsWithConflicts2_EmptyMeetingDates()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts2(conflictDate, conflictDate.AddHours(2), new List<DateTime>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EventRepository_GetRoomIdsWithConflicts2_InvalidTimeRange()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts2(conflictDate, conflictDate.AddHours(-1), new List<DateTime> { conflictDate }, null);
            }

            
            [TestMethod]
            public void EventRepository_GetRoomIdsWithConflicts2_ValidTimeAndDates_NoRoomsListedOnConflicts()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts2(new DateTimeOffset(conflictDate.AddHours(3)), new DateTimeOffset(conflictDate.AddHours(4)), new List<DateTime>() { conflictDate, conflictDate.AddDays(1) }, null);
                Assert.AreEqual(0, result.Count());
            }
           
            [TestMethod]
            public void EventRepository_GetRoomIdsWithConflicts2_NoConflictsInDateRange()
            {
                var result = eventsRepo.GetRoomIdsWithConflicts2(conflictDate, conflictDate.AddHours(1), new List<DateTime>() { conflictDate.AddDays(1), conflictDate.AddDays(2) }, null);
                Assert.AreEqual(0, result.Count());
            }
        }

        [TestClass]
        public class EventRepository_GetAllCalendarsAsyncTests : EventRepositoryTests
        {
            [TestMethod]
            public async Task AllCalendarsReturned()
            {
                var actual = await eventsRepo.GetCalendarsAsync();
                Assert.AreEqual(campusCalendars.Count, actual.Count());
            }

            [TestMethod]
            public async Task NoCampusCalendarRecordsTest()
            {
                campusCalendars = new Collection<DataContracts.CampusCalendar>();
                var actual = await eventsRepo.GetCalendarsAsync();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NoCalendarScheduleRecordsTest()
            {
                calendarSchedules = new Collection<CalendarSchedules>();
                var actual = await eventsRepo.GetCalendarsAsync();

                Assert.IsFalse(actual.Any(c => c.BookedEventDates.Any()));
            }

            [TestMethod]
            public async Task NoCampusSpecialDayRecordsTest()
            {
                specialDaysTestData.specialDayRecords = new Collection<CampusSpecialDay>();
                var actual = await eventsRepo.GetCalendarsAsync();

                Assert.IsFalse(actual.Any(c => c.SpecialDays.Any()));
            }

            [TestMethod]
            public async Task NoCalendarDayTypesValcodeTest()
            {
                specialDaysTestData.CalendarDayTypes = null;
                var actual = await eventsRepo.GetCalendarsAsync();

                Assert.IsFalse(actual.SelectMany(a => a.SpecialDays).Any());
            }

            //SpecialDays tests
            [TestMethod]
            public async Task SpecialDayStartDateIsRequiredTest()
            {
                foreach (var rec   in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdStartDate = null;
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                Assert.IsFalse(actual.SelectMany(a => a.SpecialDays).Any());

            }

            [TestMethod]
            public async Task SpecialDayEndDateIsRequiredTest()
            {
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdEndDate = null;
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                Assert.IsFalse(actual.SelectMany(a => a.SpecialDays).Any());

            }

            [TestMethod]
            public async Task SpecialDayIsHolidayTest()
            {
                //modify all special day records to that type equals "HO" which has a corresponding valcode entry with specail action code 1 = "HO"
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdType = "HO";
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                //assert all special days are holidays
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(s => s.IsHoliday));
            }

            [TestMethod]
            public async Task SpecialDayIsPayrollHolidayTest()
            {
                //modify all special day records to that type equals "HO" which has a corresponding valcode entry with specail action code 1 = "HO"
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdType = "HO";
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                //assert all special days are holidays
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(s => s.IsPayrollHoliday));
            }

            [TestMethod]
            public async Task SpecialDayIsNotPayrollHolidayTest()
            {
                //modify all special day records to that type equals "SNOW" which has a corresponding valcode entry with no special action code
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdType = "SNOW";
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                //assert all special days are not holidays
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(s => !s.IsPayrollHoliday));
            }

            [TestMethod]
            public async Task SpecialDayIsNotHolidayTest()
            {
                //modify all special day records to that type equals "SNOW" which has a corresponding valcode entry with no special action code
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdType = "SNOW";
                }

                var actual = await eventsRepo.GetCalendarsAsync();

                //assert all special days are not holidays
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(s => !s.IsHoliday));
            }

            [TestMethod]
            public async Task SpecialDayIsFullDayTest()
            {
                //modify all special day records so that the start and end times are null
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdStartTime = null;
                    rec.CmsdEndTime = null;
                }

                var actual = await eventsRepo.GetCalendarsAsync();
                //assert all special day records are full days
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(sd => sd.IsFullDay));
            }

            [TestMethod]
            public async Task SpecialDayIsNotFullDayTest()
            {
                //modify all special day records so that the start and end times are null
                foreach (var rec in specialDaysTestData.specialDayRecords)
                {
                    rec.CmsdStartTime = rec.CmsdStartDate.Value.AddHours(8);
                    rec.CmsdEndTime = rec.CmsdEndDate.Value.AddHours(15);
                }

                var actual = await eventsRepo.GetCalendarsAsync();
                //assert all special day records are not full days
                Assert.IsTrue(actual.SelectMany(a => a.SpecialDays).All(sd => !sd.IsFullDay));
            }

        }


        private Collection<CalendarSchedules> BuildCalendarSchedulesResponse(IEnumerable<Event> events)
        {
            Collection<CalendarSchedules> repoCalendarSchedules = new Collection<CalendarSchedules>();
            foreach (var calEvent in events)
            {
                var repoCal = new CalendarSchedules();
                repoCal.CalsBuildings = calEvent.Buildings.ToList();
                repoCal.CalsRooms = calEvent.Rooms.ToList();
                repoCal.Recordkey = calEvent.Id.ToString();
                repoCal.CalsDate = new DateTime(calEvent.Start.Year, calEvent.Start.Month, calEvent.Start.Day, 0, 0, 0);
                repoCal.CalsDescription = calEvent.Description;
                repoCal.CalsLocation = calEvent.LocationCode;
                repoCal.CalsStartTime = calEvent.Start;
                repoCal.CalsEndTime = calEvent.End;
                repoCal.CalsPointer = calEvent.Pointer;
                repoCal.CalsType = calEvent.Type;
                repoCalendarSchedules.Add(repoCal);
                repoCal.CalsBldgRoomEntityAssociation = new List<CalendarSchedulesCalsBldgRoom>();
                for( int roomIdx = 0; roomIdx < repoCal.CalsRooms.Count(); roomIdx++)
                {
                    var assocMember = new CalendarSchedulesCalsBldgRoom()
                    {
                        CalsBuildingsAssocMember = repoCal.CalsBuildings[roomIdx],
                        CalsRoomsAssocMember = repoCal.CalsRooms[roomIdx]
                    };
                    repoCal.CalsBldgRoomEntityAssociation.Add(assocMember);
                }
            }

            return repoCalendarSchedules;
        }

        private EventRepository BuildEventRepository()
        {


            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "", true)).Returns(calsResponseData);

            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '4' BY CALS.DATE BY CALS.START.TIME", true)).Returns(cs4ResponseData);
            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'FI' AND WITH CALS.POINTER = '4' BY CALS.DATE BY CALS.START.TIME", true)).Returns(fi4ResponseData);
            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111' AND WITH CALS.DATE GE '08/01/2012' AND WITH CALS.DATE LE '08/03/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section1first3Data);
            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111' AND WITH CALS.DATE GE '08/04/2012' AND WITH CALS.DATE LE '08/05/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section1last2Data);
            //                                                                                                                       "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '1111111' AND WITH CALS.DATE GE '08/04/2012' AND WITH CALS.DATE LE '08/05/2012' BY CALS.DATE BY CALS.START.TIME"

            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '2222222' AND WITH CALS.DATE GE '08/01/2012' AND WITH CALS.DATE LE '08/03/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section2first3Data);
            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '2222222' AND WITH CALS.DATE GE '08/04/2012' AND WITH CALS.DATE LE '08/05/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section2last2Data);
            //

            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '3333333' AND WITH CALS.DATE GE '08/01/2012' AND WITH CALS.DATE LE '08/03/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section3first3Data);
            
            // return conflicts based on dates and start and end time
            DateTime conflictDate = DateTime.Today;
            string criteria = string.Format("WITH CALS.DATE GE '{0}'"
            + " AND CALS.DATE LE '{1}'",
                UniDataFormatter.UnidataFormatDate(conflictDate, "MDY", "/"),
                UniDataFormatter.UnidataFormatDate(conflictDate.AddDays(1), "MDY", "/"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", criteria)).Returns(roomsWithConflictsData.Select(r => r.Recordkey).ToArray());
            string startEndCriteria = string.Format("WITH CALS.BLDG.ROOM.IDX NE ''"
            + " AND CALS.START.TIME LE '{0}' AND CALS.END.TIME GT '{0}'",
                conflictDate.AddHours(1).TimeOfDay.ToString(@"hh\:mm\:ss"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", It.IsAny<string[]>(), startEndCriteria)).Returns(roomsWithConflictsData.Select(r => r.Recordkey).ToArray());
            string startCriteria = string.Format("WITH CALS.BLDG.ROOM.IDX NE ''"
            + " AND CALS.START.TIME GT '{0}' AND CALS.START.TIME LT '{1}'",
                conflictDate.AddHours(1).TimeOfDay.ToString(@"hh\:mm\:ss"),
                conflictDate.AddHours(2).TimeOfDay.ToString(@"hh\:mm\:ss"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", It.IsAny<string[]>(), startCriteria)).Returns(new string[] { });
            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string[]>(), It.IsAny<bool>())).Returns(roomsWithConflictsData);

            // return conflicts based on dates, but not start and end time
            startEndCriteria = string.Format("WITH CALS.START.TIME LE '{0}' AND CALS.END.TIME GT '{0}'",
                conflictDate.AddHours(3).TimeOfDay.ToString(@"hh\:mm\:ss"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", It.IsAny<string[]>(), startEndCriteria)).Returns(new string[]{});
            startCriteria = string.Format("WITH CALS.START.TIME GT '{0}' AND CALS.START.TIME LT '{1}'",
                conflictDate.AddHours(3).TimeOfDay.ToString(@"hh\:mm\:ss"),
                conflictDate.AddHours(4).TimeOfDay.ToString(@"hh\:mm\:ss"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", It.IsAny<string[]>(), startCriteria)).Returns(new string[]{});

            // return no conflicts based on dates
            criteria = string.Format("WITH CALS.BLDG.ROOM.IDX NE ''"
            + " AND CALS.DATE GE '{0}'"
            + " AND CALS.DATE LE '{1}'",
                UniDataFormatter.UnidataFormatDate(conflictDate.AddDays(1), "MDY", "/"),
                UniDataFormatter.UnidataFormatDate(conflictDate.AddDays(2), "MDY", "/"));
            dataReaderMock.Setup<string[]>(acc => acc.Select("CALENDAR.SCHEDULES", criteria)).Returns(new string[]{});

            dataReaderMock.Setup<Collection<CalendarSchedules>>(acc => acc.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", "WITH CALS.TYPE = 'CS' AND WITH CALS.POINTER = '3333333' AND WITH CALS.DATE GE '08/01/2012' AND WITH CALS.DATE LE '08/03/2012' BY CALS.DATE BY CALS.START.TIME", true)).Returns(section3first3Data);

            CalendarSchedules cal = calsResponseData.Where(s => s.Recordkey == "1").First();
            dataReaderMock.Setup<CalendarSchedules>(acc => acc.ReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", It.IsAny<string>(), true)).Returns(cal);

            // EventTypes mock
            ApplValcodes eventTypesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "CS", ValActionCode1AssocMember = "CS" },
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "FI", ValActionCode1AssocMember = "FI"},
                                                                      new ApplValcodesVals() { ValInternalCodeAssocMember = "XS", ValActionCode1AssocMember = ""}}
            };
            dataReaderMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "EVENT.TYPES", true)).Returns(eventTypesResponse);

            dataReaderMock.Setup<Data.Base.DataContracts.IntlParams>(acc => acc.ReadRecord<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", It.IsAny<bool>())).Returns(new DataContracts.IntlParams() { Recordkey = "INTP", HostShortDateFormat = "MDY", HostDateDelimiter = "/" });

   
            apiSettings = new ApiSettings("API") { ColleagueTimeZone = TimeZoneInfo.Local.Id };

            //these mocks were added on 5/3/18 after updating the repository to expand the campusCalendar object
            dataReaderMock.Setup(r => r.ReadRecord<DataContracts.CampusCalendar>(It.IsAny<string>(), true))
                .Returns<string, bool>((id, b) => campusCalendars.FirstOrDefault(cc => cc.Recordkey == id));
            dataReaderMock.Setup(r => r.BulkReadRecord<DataContracts.CalendarSchedules>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => new Collection<DataContracts.CalendarSchedules>(calendarSchedules.Where(cs => ids.Contains(cs.Recordkey)).ToList()));
            dataReaderMock.Setup(r => r.BulkReadRecord<DataContracts.CampusSpecialDay>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => new Collection<CampusSpecialDay>(specialDaysTestData.specialDayRecords.Where(sd => ids.Contains(sd.Recordkey)).ToList()));

            dataReaderMock.Setup(r => r.ReadRecord<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES", true))
                .Returns<string, string, bool>((e, id, b) => specialDaysTestData.CalendarDayTypes);

            dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.CampusCalendar>("", true))
                .Returns<string, bool>((s, b) => Task.FromResult(campusCalendars));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.CalendarSchedules>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(new Collection<DataContracts.CalendarSchedules>(calendarSchedules.Where(c => ids.Contains(c.Recordkey)).ToList())));
            dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.CampusSpecialDay>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => Task.FromResult(new Collection<DataContracts.CampusSpecialDay>(specialDaysTestData.specialDayRecords.Where(sd => ids.Contains(sd.Recordkey)).ToList())));

            dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES", true))
                .Returns<string, string, bool>((e, id, b) => Task.FromResult(specialDaysTestData.CalendarDayTypes));
                

            eventsRepo = new EventRepository(apiSettings, cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return eventsRepo;
        }

        private void SetupCampusCalendars()
        {
            // Read a CAMPUS.CALENDAR record
            dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.CampusCalendar>(
                reader => reader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.CampusCalendar>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return campusCalendars.FirstOrDefault(cc => cc.Recordkey == id); });

            // Read a CALENDAR.SCHEDULES record
            dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.CalendarSchedules>(
                reader => reader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.CalendarSchedules>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return calendarSchedules.FirstOrDefault(cs => cs.Recordkey == id); });

            // Read INTL.PARMS record
            dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>(
                reader => reader.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return intlParams.FirstOrDefault(ip => ip.Recordkey == id); });
        }
    }
}
