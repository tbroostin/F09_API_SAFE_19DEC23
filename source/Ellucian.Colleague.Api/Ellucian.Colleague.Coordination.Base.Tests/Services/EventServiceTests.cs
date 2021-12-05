// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Data.Base.Tests;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EventServiceTests : CurrentUserSetup
    {
        private void VerifyICalZuluDateTime(string dateTimeString, DateTime eventTime)
        {
            Assert.AreEqual('Z', dateTimeString[15]);
            Assert.AreEqual('T', dateTimeString[8]);
            Assert.AreEqual(eventTime, new DateTime(int.Parse(dateTimeString.Substring(0, 4)), int.Parse(dateTimeString.Substring(4, 2)), int.Parse(dateTimeString.Substring(6, 2)), int.Parse(dateTimeString.Substring(9, 2)), int.Parse(dateTimeString.Substring(11, 2)), int.Parse(dateTimeString.Substring(13, 2))));
        }

        public Mock<IEventRepository> eventRepositoryMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> allCals;
        public AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event> eventDtoAdapter;
        public CampusCalendarEntityToDtoAdapter campusCalendarEntityToDtoAdapter;
        public EventService eventService;

        public Mock<IRoleRepository> roleRepositoryMock;
        public ICurrentUserFactory currentUserFactory;

        [TestInitialize]
        public void Initialize()
        {
            eventRepositoryMock = new Mock<IEventRepository>();

            adapterRegistryMock = new Mock<IAdapterRegistry>();

            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();

            currentUserFactory = new PersonUserFactory();
            allCals = new TestEventRepository().Get();
            eventDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event>())
                .Returns(eventDtoAdapter);
            campusCalendarEntityToDtoAdapter = new CampusCalendarEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Base.Entities.CampusCalendar, Dtos.Base.CampusCalendar>())
                .Returns(() => campusCalendarEntityToDtoAdapter);

            eventService = new EventService(eventRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {

            allCals = null;

            eventDtoAdapter = null;
            eventService = null;
        }

        [TestClass]
        public class EventService_GetSectionEvents : EventServiceTests
        {


            [TestInitialize]
            public new void Initialize()
            {
                base.Initialize();
            }



            [TestMethod]
            public void GetSectionEvents_ValidSingleSectionWithEvents()
            {
                string secId = "1111111";
                List<string> secIds = new List<string>() { secId };
                var csEvents = allCals.Where(c => c.Pointer == secId && c.Type == "CS");
                eventRepositoryMock.Setup(repo => repo.Get("CS", secIds, null, null)).Returns(csEvents);
                string iCal = eventService.GetSectionEvents(secIds, null, null).iCal;
                string[] separator = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(separator, 99, System.StringSplitOptions.None);
                Assert.AreEqual(6, res.Length); // 5 events split into 6 pieces (+1 for "END:VCALENDAR" after last VEVENT)
                // Find the DTSTART and DTEND in each item and verify the correct format (do not process the last item, it's the END:VCALENDAR portion)
                for (int i = 0; i < res.Count() - 1; i++)
                {
                    var eventItem = csEvents.ElementAt(i);
                    separator[0] = "\r\n";
                    string[] eventPieces = res[i].Split(separator, 99, StringSplitOptions.None);
                    string startPiece = eventPieces.Where(e => e.StartsWith("DTSTART:")).FirstOrDefault();
                    if (startPiece != null)
                    {
                        // ical date is formatted: YYYYMMDDTHHMMSSZ (T and Z are hardcoded characters)
                        var dateTimeString = startPiece.Substring(8);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.StartTime.UtcDateTime);
                    }
                    string endPiece = eventPieces.Where(e => e.StartsWith("DTEND:")).FirstOrDefault();
                    if (endPiece != null)
                    {
                        var dateTimeString = endPiece.Substring(6);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.EndTime.UtcDateTime);
                    }
                }

            }

            [TestMethod]
            public void GetSectionEvents_SingleSectionFirstThreeDates()
            {
                string secId = "1111111";
                List<string> secIds = new List<string>() { secId };
                DateTime start = new DateTime(2012, 8, 1);
                DateTime end = new DateTime(2012, 8, 3);
                eventRepositoryMock.Setup(repo => repo.Get("CS", secIds, start, end)).Returns(allCals.Where(c => c.Pointer == secId && c.Type == "CS" && c.Start.Year == 2012 && c.Start.Month == 8 && c.Start.Day <= 3));
                string iCal = eventService.GetSectionEvents(secIds, start, end).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(4, res.Length); // 3 events split into 4 pieces (+1 for "END:VCALENDAR" after last VEVENT)
            }

            [TestMethod]
            public void GetSectionEvents_ValidMultipleSectionsWithEvents()
            {
                string secId1 = "1111111";
                string secId2 = "2222222";
                List<string> secIds = new List<string>() { secId1, secId2 };
                eventRepositoryMock.Setup(repo => repo.Get("CS", secIds, null, null)).Returns(allCals.Where(c => (c.Pointer == secId1 || c.Pointer == secId2) && c.Type == "CS"));
                string iCal = eventService.GetSectionEvents(secIds, null, null).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(9, res.Length); // 8 (5+3) events split into 9 pieces (+1 for "END:VCALENDAR" after last VEVENT)
            }

            [TestMethod]
            public void GetSectionEvents_InvalidSection()
            {
                string secId = "9999999";
                List<string> secIds = new List<string>() { secId };
                eventRepositoryMock.Setup(repo => repo.Get("CS", secIds, null, null)).Returns(allCals.Where(c => c.Pointer == secId && c.Type == "CS"));
                string iCal = eventService.GetSectionEvents(secIds, null, null).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(1, res.Length); // no splits occur
            }
        }

        [TestClass]
        public class EventService_GetFacultyEvents : EventServiceTests
        {
            //private Mock<IEventRepository> eventRepositoryMock;
            //private IEventRepository calsRepo;
            //private Mock<IAdapterRegistry> adapterRegistryMock;
            //private IAdapterRegistry adapterRegistry;
            //private ILogger logger;
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> allCals;
            public IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> testUTCCals;
            //private AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event> eventDtoAdapter;
            //private EventService eventService;

            [TestInitialize]
            public new void Initialize()
            {

                base.Initialize();
                testUTCCals = new TestEventRepository().GetEventsWithMidnightUTCTime();


                //eventDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event>(adapterRegistry, logger);
                //adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Event, Ellucian.Colleague.Dtos.Base.Event>()).Returns(eventDtoAdapter);
                //eventService = new EventService(adapterRegistryMock.Object, calsRepoMock.Object);
            }

            //[TestCleanup]
            //public void Cleanup()
            //{

            //    allCals = null;
            //    testUTCCals = null;

            //    eventDtoAdapter = null;
            //    eventService = null;
            //}

            [TestMethod]
            public void GetFacultyEvents_SingleFacultyWithEvents()
            {
                string facId = "1111111";
                List<string> facIds = new List<string>() { facId };
                var facEvents = allCals.Where(c => c.Pointer == facId && c.Type == "FI");
                eventRepositoryMock.Setup(repo => repo.Get("FI", facIds, null, null)).Returns(facEvents);
                string iCal = eventService.GetFacultyEvents(facIds, null, null).iCal;
                string[] separator = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(separator, 99, System.StringSplitOptions.None);
                Assert.AreEqual(4, res.Length); // 3 events split into 4 pieces (+1 for "END:VCALENDAR" after last VEVENT)
                // Find the DTSTART and DTEND in each item and verify the correct format (do not process the last item, it's the END:VCALENDAR portion)
                for (int i = 0; i < res.Count() - 1; i++)
                {
                    var eventItem = allCals.Where(c => c.Type == "FI").ElementAt(i);
                    separator[0] = "\r\n";
                    string[] eventPieces = res[i].Split(separator, 99, StringSplitOptions.None);
                    string startPiece = eventPieces.Where(e => e.StartsWith("DTSTART:")).FirstOrDefault();
                    if (startPiece != null)
                    {
                        // ical date is formatted: YYYYMMDDTHHMMSSZ (T and Z are hardcoded characters)
                        var dateTimeString = startPiece.Substring(8);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.StartTime.UtcDateTime);
                    }
                    string endPiece = eventPieces.Where(e => e.StartsWith("DTEND:")).FirstOrDefault();
                    if (endPiece != null)
                    {
                        var dateTimeString = endPiece.Substring(6);
                        VerifyICalZuluDateTime(dateTimeString, eventItem.EndTime.UtcDateTime);
                    }
                }
            }

            [TestMethod]
            public void GetFacultyEvents_MultipleFacultyWithEvents()
            {
                string facId1 = "1111111";
                string facId2 = "2222222";
                List<string> facIds = new List<string>() { facId1, facId2 };
                eventRepositoryMock.Setup(repo => repo.Get("FI", facIds, null, null)).Returns(allCals.Where(c => (c.Pointer == facId1 || c.Pointer == facId2) && c.Type == "FI"));
                string iCal = eventService.GetFacultyEvents(facIds, null, null).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(8, res.Length); // 7 events split into 8 pieces (+1 for "END:VCALENDAR" after last VEVENT)
            }

            [TestMethod]
            public void GetFacultyEvents_InvalidFaculty()
            {
                string facId = "9999999";
                List<string> facIds = new List<string>() { facId };
                eventRepositoryMock.Setup(repo => repo.Get("FI", facIds, null, null)).Returns(allCals.Where(c => c.Pointer == facId && c.Type == "FI"));
                string iCal = eventService.GetFacultyEvents(facIds, null, null).iCal;
                string[] seps = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(seps, 99, System.StringSplitOptions.None);
                Assert.AreEqual(1, res.Length); // no splits occur
            }

            [TestMethod]
            public void GetSectionEvents_MidnightUTC()
            {
                //Check if the event start time is midnight utc (8pm EST) the ical event contains the time component along with date and not an all-day event
                string secId = "100";
                List<string> secIds = new List<string>() { secId };
                eventRepositoryMock.Setup(repo => repo.Get("CS", secIds, null, null)).Returns(testUTCCals.Where(c => c.Pointer == secId && c.Type == "CS"));
                string iCal = eventService.GetSectionEvents(secIds, null, null).iCal;
                string[] separator = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(separator, 99, System.StringSplitOptions.None);

                Assert.AreEqual(4, res.Length);

                for (int i = 0; i < res.Count() - 1; i++)
                {
                    var eventItem = testUTCCals.Where(c => c.Type == "CS").ElementAt(i);
                    separator[0] = "\r\n";
                    string[] eventPieces = res[i].Split(separator, 99, StringSplitOptions.None);
                    string startPiece = eventPieces.Where(e => e.StartsWith("DTSTART:")).FirstOrDefault(); //The midnight value came as DTSTART; instead of DTSTART: - so was null before the fix
                    string endPiece = eventPieces.Where(e => e.StartsWith("DTEND:")).FirstOrDefault();

                    Assert.IsNotNull(startPiece);
                    Assert.IsNotNull(endPiece);


                    // ical date is formatted: YYYYMMDDTHHMMSSZ (T and Z are hardcoded characters)
                    var startDateTimeString = startPiece.Substring(8);
                    VerifyICalZuluDateTime(startDateTimeString, eventItem.StartTime.UtcDateTime);

                    var endDateTimeString = endPiece.Substring(6);
                    VerifyICalZuluDateTime(endDateTimeString, eventItem.EndTime.UtcDateTime);

                }


            }

            [TestMethod]
            public void GetFacultyEvents_AllDayEvent()
            {
                string facId = "100";
                List<string> facIds = new List<string>() { facId };
                eventRepositoryMock.Setup(repo => repo.Get("FI", facIds, null, null)).Returns(testUTCCals.Where(c => c.Pointer == facId && c.Type == "FI"));
                string iCal = eventService.GetFacultyEvents(facIds, null, null).iCal;
                string[] separator = new string[1] { "END:VEVENT" };
                string[] res = iCal.Split(separator, 99, System.StringSplitOptions.None);

                Assert.AreEqual(3, res.Length);

                for (int i = 0; i < res.Count() - 1; i++)
                {
                    var eventItem = testUTCCals.Where(c => c.Type == "FI").ElementAt(i);
                    separator[0] = "\r\n";
                    string[] eventPieces = res[i].Split(separator, 99, StringSplitOptions.None);
                    string startPiece = eventPieces.Where(e => e.StartsWith("DTSTART:")).FirstOrDefault();
                    string endPiece = eventPieces.Where(e => e.StartsWith("DTEND:")).FirstOrDefault();

                    Assert.IsNotNull(startPiece);
                    Assert.IsNotNull(endPiece);


                    // ical date is formatted: YYYYMMDDTHHMMSSZ (T and Z are hardcoded characters)
                    if (startPiece != null)
                    {
                        var startDateTimeString = startPiece.Substring(8);
                        VerifyICalZuluDateTime(startDateTimeString, eventItem.StartTime.UtcDateTime);
                    }

                    if (endPiece != null)
                    {
                        var endDateTimeString = endPiece.Substring(6);
                        VerifyICalZuluDateTime(endDateTimeString, eventItem.EndTime.UtcDateTime);
                    }
                }


            }

        }

        [TestClass]
        public class EventService_GetAllCampusCalendarsAsync : EventServiceTests
        {
            public List<Domain.Base.Entities.CampusCalendar> expectedCalendars;

            [TestInitialize]
            public void Init()
            {
                Initialize();

                expectedCalendars = new List<Domain.Base.Entities.CampusCalendar>()
                {
                    new Domain.Base.Entities.CampusCalendar("HOLIDAYS", "holidays", TimeSpan.FromHours(8), TimeSpan.FromHours(17))
                    {
                        BookPastNumberOfDays = 10,
                    },
                    new Domain.Base.Entities.CampusCalendar("SNOWDAYS", "snow days", TimeSpan.FromHours(8), TimeSpan.FromHours(17))
                    {
                        BookPastNumberOfDays = 20
                    }
                };

                expectedCalendars[0].AddBookedEventDate(new DateTime(2018, 1, 1));
                expectedCalendars[0].AddSpecialDay(new Domain.Base.Entities.SpecialDay("1", "New Years Day", "HOLIDAYS", "HOL", true, true, new DateTimeOffset(new DateTime(2018, 1, 1)), new DateTimeOffset(new DateTime(2018, 1, 1))));

                expectedCalendars[1].AddBookedEventDate(new DateTime(2018, 2, 14));
                expectedCalendars[1].AddSpecialDay(new Domain.Base.Entities.SpecialDay("2", "Winter Storm Maria", "SNOWDAYS", "SNOW", false, true, new DateTimeOffset(new DateTime(2018, 2, 14)), new DateTimeOffset(new DateTime(2018, 2, 14))));

                eventRepositoryMock.Setup(e => e.GetCalendarsAsync())
                    .Returns(() => Task.FromResult(expectedCalendars != null ? expectedCalendars.AsEnumerable() : null));
            }

            [TestMethod]
            public async Task GetCalendarsTest()
            {
                var calendarDtos = await eventService.GetCampusCalendarsAsync();
                Assert.AreEqual(expectedCalendars.Count, calendarDtos.Count());
            }

            [TestMethod]
            public async Task NoCalendarsTest()
            {
                expectedCalendars = null;
                var calendarDtos = await eventService.GetCampusCalendarsAsync();

                Assert.AreEqual(0, calendarDtos.Count());
            }
        }
    }
}
