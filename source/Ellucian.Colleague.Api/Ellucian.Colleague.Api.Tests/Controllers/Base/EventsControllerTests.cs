using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EventsControllerTests
    {
        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #endregion

        public Mock<ILogger> loggerMock;
        public Mock<IEventService> eventServiceMock;

        public EventsController eventsController;

        public CampusCalendar expectedCampusCalendar;

        [TestInitialize]
        public void BaseInitialize()
        {
            loggerMock = new Mock<ILogger>();
            eventServiceMock = new Mock<IEventService>();

            expectedCampusCalendar = new CampusCalendar()
            {
                Id = "HOLIDAY",
                Description = "Holiday Calendar",
                DefaultStartOfDay = TimeSpan.FromHours(8),
                DefaultEndOfDay = TimeSpan.FromHours(17),
                BookPastNumberOfDays = 30,
                BookedEventDates = new List<DateTime>() { new DateTime(2018, 1, 1) },
                SpecialDays = new List<SpecialDay>()
                {
                    new SpecialDay()
                    {
                        Id = "1",
                        Description = "New Years Day",
                        CampusCalendarId = "HOLIDAY",
                        IsHoliday = true,
                        IsFullDay = true,
                        SpecialDayTypeCode = "HOL",
                        StartDateTime = new DateTimeOffset(new DateTime(2018, 1, 1)),
                        EndDateTime = new DateTimeOffset(new DateTime(2018, 1, 1)),
                    }
                }
            };

            eventServiceMock.Setup(s => s.GetCampusCalendarsAsync())
                .Returns(() => Task.FromResult<IEnumerable<CampusCalendar>>(new List<CampusCalendar>() { expectedCampusCalendar }));

            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            eventsController = new EventsController(eventServiceMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class EventsController_GetAllCampusCalendarsAsyncTests : EventsControllerTests
        {
            [TestMethod]
            public async Task GetCalendarsTest()
            {
                var actual = await eventsController.GetCampusCalendarsAsync();

                Assert.AreEqual(expectedCampusCalendar.Id, actual.First().Id);
            }

            [TestMethod] 
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchExceptionAndThrowTest()
            {
                eventServiceMock.Setup(s => s.GetCampusCalendarsAsync())
                    .Throws(new Exception("foobar"));

                await eventsController.GetCampusCalendarsAsync();
            }
        }
    }
}
