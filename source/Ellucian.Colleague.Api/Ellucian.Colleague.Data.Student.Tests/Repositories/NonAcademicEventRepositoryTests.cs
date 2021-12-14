// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class NonAcademicEventRepositoryTests : BaseRepositorySetup
    {
        NonAcademicEventRepository eventRepo;
        TestNonAcademicEventRepository testDataRepository;
        IEnumerable<NonAcademicEvent> allExpectedEvents;
        Collection<NaaEvents> eventResp2 = new Collection<NaaEvents>() { new NaaEvents() { Recordkey = "1", NaaevTitle = "Event 1", NaaevTerm = "2017/FA" } };
        List<string> ids = new List<string>() { "1", "2", "3", "4" };
        List<string> idError = new List<string>() { "error" };
        ApiSettings apiSettingsMock;

        [TestInitialize]
        public async void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();
            apiSettingsMock = new ApiSettings("null");

            testDataRepository = new TestNonAcademicEventRepository();
            allExpectedEvents = testDataRepository.GetEventsByIdsTest(ids);
            Collection<NaaEvents> eventResp = BuildEventResponse();
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NaaEvents>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(eventResp);
            eventRepo = new NonAcademicEventRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
        }

        [TestCleanup]
        public void Cleanup()
        {
            eventRepo = null;
            apiSettingsMock = null;
        }

        [TestMethod]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_Null_EventIds()
        {
            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(null);
            Assert.AreEqual(0, events.Count());

        }

        [TestMethod]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_Empty_EventIds()
        {
            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(new List<string>().ToArray());
            Assert.AreEqual(0, events.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_DataReader_Returns_Null()
        {
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NaaEvents>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
            eventRepo = new NonAcademicEventRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(ids);
        }

        [TestMethod]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_DataReader_Returns_Empty_Collection()
        {
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NaaEvents>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<NaaEvents>());
            eventRepo = new NonAcademicEventRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(ids);
            Assert.AreEqual(0, events.Count());
        }


        [TestMethod]
        public async Task ValidateGetEventsById()
        {
            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(ids);
            Assert.AreEqual(allExpectedEvents.Count(), events.Count());
            int cntr = 0;
            foreach(var expectedEvent in allExpectedEvents)
            {
                var currentEvent = events.ElementAt(cntr);
                Assert.AreEqual(expectedEvent.BuildingCode, currentEvent.BuildingCode);
                Assert.AreEqual(expectedEvent.Date, currentEvent.Date);
                Assert.AreEqual(expectedEvent.Description, currentEvent.Description);
                Assert.AreEqual(expectedEvent.EndTime.Value.Hour, currentEvent.EndTime.Value.Hour);
                Assert.AreEqual(expectedEvent.EndTime.Value.Minute, currentEvent.EndTime.Value.Minute);
                Assert.AreEqual(expectedEvent.EventTypeCode, currentEvent.EventTypeCode);
                Assert.AreEqual(expectedEvent.Id, currentEvent.Id);
                Assert.AreEqual(expectedEvent.LocationCode, currentEvent.LocationCode);
                Assert.AreEqual(expectedEvent.RoomCode, currentEvent.RoomCode);
                Assert.AreEqual(expectedEvent.StartTime.Value.Hour, currentEvent.StartTime.Value.Hour);
                Assert.AreEqual(expectedEvent.StartTime.Value.Minute, currentEvent.StartTime.Value.Minute);
                Assert.AreEqual(expectedEvent.TermCode, currentEvent.TermCode);
                Assert.AreEqual(expectedEvent.Title, currentEvent.Title);
                Assert.AreEqual(expectedEvent.Venue, currentEvent.Venue);
                cntr++;
            }
        }

        [TestMethod]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_DataReader_ReturnsFewer()
        {

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NaaEvents>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(eventResp2);
            eventRepo = new NonAcademicEventRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(ids);
            Assert.AreEqual(1, events.Count());
        }

        [TestMethod]
        public async Task NonAcademicEventRepository_GetEventsByIdsAsync_DataReader_ReturnsInvalidRecord()
        {
            var eventsWithInvalid = new Collection<NaaEvents>()
            {
                eventResp2[0],
                new NaaEvents() { Recordkey = "1", NaaevTitle = string.Empty, NaaevTerm = "2017/FA" }
            };
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NaaEvents>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(eventsWithInvalid);
            eventRepo = new NonAcademicEventRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
            IEnumerable<NonAcademicEvent> events = await eventRepo.GetEventsByIdsAsync(ids);
            Assert.AreEqual(1, events.Count());
        }

        private Collection<NaaEvents> BuildEventResponse()
        {
            Collection<NaaEvents> eventData = new Collection<NaaEvents>();
            foreach (var item in allExpectedEvents)
            {
                NaaEvents eventContract = new NaaEvents();
                eventContract.NaaevBuilding = item.BuildingCode;
                eventContract.NaaevEndTime = item.EndTime.Value.DateTime;
                eventContract.NaaevLocation = item.LocationCode;
                eventContract.NaaevRoom = item.RoomCode;
                eventContract.NaaevStartDate = item.Date;
                eventContract.NaaevStartTime = item.StartTime.Value.DateTime;
                eventContract.NaaevTitle = item.Title;
                eventContract.NaaevVenue = item.Venue;
                eventContract.NaaevType = item.EventTypeCode;
                eventContract.NaaevTerm = item.TermCode;
                eventContract.Recordkey = item.Id;
                eventContract.NaaevDescription = item.Description;

                eventData.Add(eventContract);
            }

            return eventData;
        }
    }
}
