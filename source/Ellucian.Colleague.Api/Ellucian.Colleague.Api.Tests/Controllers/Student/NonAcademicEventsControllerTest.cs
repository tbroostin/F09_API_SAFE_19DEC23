// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class NonAcademicEventsControllerTests
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


        private INonAcademicEventRepository nonacademicEventRepository;
        private Mock<INonAcademicEventRepository> nonAcademicEventRepositoryMock;
        private NonAcademicEventsController nonacademicEventsController;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.NonAcademicEvent> events;
        private List<string> eventIds;
        ILogger logger = new Mock<ILogger>().Object;
        NonAcademicEventQueryCriteria criteria;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            nonAcademicEventRepositoryMock = new Mock<INonAcademicEventRepository>();
            nonacademicEventRepository = nonAcademicEventRepositoryMock.Object;
            logger = new Mock<ILogger>().Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.NonAcademicEvent, NonAcademicEvent>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NonAcademicEvent, NonAcademicEvent>()).Returns(adapter);

            eventIds = new List<string>() { "1", "2", "3", "4" };
            events = new TestNonAcademicEventRepository().GetEventsByIdsTest(eventIds);
            criteria = new NonAcademicEventQueryCriteria()
            {
                EventIds = eventIds,
            };

            nonacademicEventsController = new NonAcademicEventsController(adapterRegistry, nonacademicEventRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            nonacademicEventsController = null;
            nonacademicEventRepository = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_Null_Ids_and_QueryString()
        {
            criteria.EventIds = null;
            var eventsResult = await nonacademicEventsController.QueryNonAcademicEventsAsync(criteria);
        }

        [TestMethod]
        public async Task QueryNonAcademicEventsAsync_Ids()
        {
            nonAcademicEventRepositoryMock.Setup(x => x.GetEventsByIdsAsync(eventIds)).Returns(Task.FromResult(events));
            var eventsResult = await nonacademicEventsController.QueryNonAcademicEventsAsync(criteria);
            Assert.AreEqual(4, eventsResult.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                nonAcademicEventRepositoryMock.Setup(x => x.GetEventsByIdsAsync(eventIds)).Throws(new ApplicationException());
                var eventsResult = await nonacademicEventsController.QueryNonAcademicEventsAsync(criteria);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_NoCriteria_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                var eventsResult = await nonacademicEventsController.QueryNonAcademicEventsAsync(null);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_NoEventIds_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                var eventsResult = await nonacademicEventsController.QueryNonAcademicEventsAsync(new NonAcademicEventQueryCriteria());
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                nonAcademicEventRepositoryMock.Setup(x => x.GetEventsByIdsAsync(eventIds))
                            .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
               await nonacademicEventsController.QueryNonAcademicEventsAsync(criteria);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryNonAcademicEventsAsync_Generic_Exception()
        {
            try
            {
                nonAcademicEventRepositoryMock.Setup(x => x.GetEventsByIdsAsync(eventIds)).Throws(new Exception());
                await nonacademicEventsController.QueryNonAcademicEventsAsync(criteria);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }
    }
}
