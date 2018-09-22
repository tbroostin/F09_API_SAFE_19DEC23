// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructionalEventControllerTests
    {
        [TestClass]
        public class Get
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents();
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;

                sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                instructionalEventsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };


                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>(allInstructionalEventsDtos, 5);

                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "")).ReturnsAsync(tuple);
                //sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var instructionalEvents = await instructionalEventsController.GetHedmInstructionalEventsAsync(new Paging(10, 0), "fjf", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_DefaultPaging()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                //var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>(allInstructionalEventsDtos, 5);

                var totalCount = allInstructionalEventsDtos.Count();
                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>(allInstructionalEventsDtos, totalCount);
                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "")).ReturnsAsync(tuple);
                var instructionalEvents = await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventAsync()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent2Async(guid)).Returns(Task.FromResult(allInstructionalEventsDtos.ElementAt(0)));
                var instructionalEvent = await instructionalEventsController.GetHedmAsync(guid);
                Assert.AreEqual(instructionalEvent.Id, allInstructionalEventsDtos.ElementAt(0).Id);
                Assert.AreEqual(instructionalEvent.InstructionalMethod.Id, allInstructionalEventsDtos.ElementAt(0).InstructionalMethod.Id);
                Assert.AreEqual(instructionalEvent.Section.Id, allInstructionalEventsDtos.ElementAt(0).Section.Id);
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetAllInstructionalEventsAsync()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var totalCount = allInstructionalEventsDtos.Count();
                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>(allInstructionalEventsDtos, totalCount);
                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", "", "")).ReturnsAsync(tuple);
                var instructionalEvents = await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instrEvent in results)
                {
                    Assert.AreEqual(instrEvent.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).Id);
                    Assert.AreEqual(instrEvent.InstructionalMethod.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).InstructionalMethod.Id);
                    Assert.AreEqual(instrEvent.Section.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).Section.Id);
                }
            }

            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_PermissionsException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_ArgumentException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new ArgumentException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_RepositoryException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_ConfigurationException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_Exception()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async("asdf"))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.GetHedmAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_PermissionsException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_ArgumentException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());
                await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_RepositoryException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_Exception()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.GetHedmInstructionalEventsAsync(null, "fjf", "");
            }
            #endregion

        }
        [TestClass]
        public class Get_V8
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3> allInstructionalEventsDtos;
            private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
            private QueryStringFilter academicPeriodFilter = new QueryStringFilter("academicPeriod", "");

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents3();
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;

                sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(allInstructionalEventsDtos, 5);

                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                //sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });

                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents3Async(new Paging(10, 0), criteriaFilter, academicPeriodFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_DefaultPaging()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                //var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(allInstructionalEventsDtos, 5);

                var totalCount = allInstructionalEventsDtos.Count();
                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(allInstructionalEventsDtos, totalCount);
                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEventAsync()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent3Async(guid)).Returns(Task.FromResult(allInstructionalEventsDtos.ElementAt(0)));
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent3Async(guid);
                Assert.AreEqual(instructionalEvent.Id, allInstructionalEventsDtos.ElementAt(0).Id);
                Assert.AreEqual(instructionalEvent.InstructionalMethod.Id, allInstructionalEventsDtos.ElementAt(0).InstructionalMethod.Id);
                Assert.AreEqual(instructionalEvent.Section.Id, allInstructionalEventsDtos.ElementAt(0).Section.Id);
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetAllInstructionalEventsAsync()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var totalCount = allInstructionalEventsDtos.Count();
                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(allInstructionalEventsDtos, totalCount);
                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "", "", new List<string>(), new List<string>(), "")).ReturnsAsync(tuple);
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instrEvent in results)
                {
                    Assert.AreEqual(instrEvent.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).Id);
                    Assert.AreEqual(instrEvent.InstructionalMethod.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).InstructionalMethod.Id);
                    Assert.AreEqual(instrEvent.Section.Id, allInstructionalEventsDtos.FirstOrDefault(ai => ai.Id == instrEvent.Id).Section.Id);
                }
            }

            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_PermissionsException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_ArgumentException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new ArgumentException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_RepositoryException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_ConfigurationException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventAsync_Exception()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async("asdf"))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.GetInstructionalEvent3Async("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_PermissionsException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_ArgumentException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_RepositoryException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEventsAsync_Exception()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.GetInstructionalEvent3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstructionalEvent2() { Section = new GuidObject2("fjf") });
                await instructionalEventsController.GetInstructionalEvents3Async(null, criteriaFilter, academicPeriodFilter);
            }
            #endregion

        }

        [TestClass]
        public class Get_V11
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent4> allInstructionalEventsDtos;
            private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
            private QueryStringFilter academicPeriodFilter = new QueryStringFilter("academicPeriod", "");
            private QueryStringFilter instructionalEventInstancesFilter = new QueryStringFilter("instructionalEventInstances", "");

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents4();
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;

                sectionCoordinationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }


            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                instructionalEventsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
                {
                    NoCache = true                    
                };

                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(allInstructionalEventsDtos, 5);

                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.InstructionalEventFilter3() { Section = new GuidObject2("7a6e9b82-e78b-47db-9f30-5becff004921") });

                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>())).ReturnsAsync(tuple);
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(new Paging(10, 0), criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent4>>)httpResponseMessage.Content)
                    .Value as IEnumerable<Dtos.InstructionalEvent4>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_DefaultPaging()
            {
                instructionalEventsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                //var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>(allInstructionalEventsDtos, 5);

                var totalCount = allInstructionalEventsDtos.Count();
                var tuple = new Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>(allInstructionalEventsDtos, totalCount);
                sectionCoordinationServiceMock.Setup(s => s.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>())).ReturnsAsync(tuple);

                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.InstructionalEventFilter3() { Section = new GuidObject2("7a6e9b82-e78b-47db-9f30-5becff004921") });

                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await instructionalEvents.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.InstructionalEvent4> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.InstructionalEvent4>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(instructionalEvents is IHttpActionResult);

                foreach (var instructionalEventsDto in allInstructionalEventsDtos)
                {
                    //var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);
                    var instEvent = results.FirstOrDefault(i => i.Id == instructionalEventsDto.Id);

                    Assert.AreEqual(instructionalEventsDto.Id, instEvent.Id);
                    Assert.AreEqual(instructionalEventsDto.InstructionalMethod.Id, instEvent.InstructionalMethod.Id);
                    Assert.AreEqual(instructionalEventsDto.Section.Id, instEvent.Section.Id);

                }
            }

            [TestMethod]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async()
            {
                instructionalEventsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
                {
                    NoCache = true
                };
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).Returns(Task.FromResult(allInstructionalEventsDtos.ElementAt(0)));
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
                Assert.AreEqual(instructionalEvent.Id, allInstructionalEventsDtos.ElementAt(0).Id);
                Assert.AreEqual(instructionalEvent.InstructionalMethod.Id, allInstructionalEventsDtos.ElementAt(0).InstructionalMethod.Id);
                Assert.AreEqual(instructionalEvent.Section.Id, allInstructionalEventsDtos.ElementAt(0).Section.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async_PermissionsException()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ThrowsAsync(new PermissionsException());
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async_ArgumentException()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ThrowsAsync(new ArgumentException());
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async_RepositoryException()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ThrowsAsync(new RepositoryException());
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async_IntegrationApiException()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ThrowsAsync(new IntegrationApiException());
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvent4Async_Exception()
            {
                string guid = allInstructionalEventsDtos.ElementAt(0).Id;
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ThrowsAsync(new Exception());
                var instructionalEvent = await instructionalEventsController.GetInstructionalEvent4Async(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_ArgumentException()
            {

                var filterGroupName = "criteria";
                instructionalEventsController.Request.Properties.Add(
                      string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.Filters.InstructionalEventFilter3() { Section = null });

                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_PermissionException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_RepositoryException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_IntegrationApiException()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new IntegrationApiException());
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_GetInstructionalEvents4Async_Exception()
            {
                sectionCoordinationServiceMock.Setup(i => i.GetInstructionalEvent4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var instructionalEvents = await instructionalEventsController.GetInstructionalEvents4Async(null, criteriaFilter, academicPeriodFilter, instructionalEventInstancesFilter);
            }
        }

        [TestClass]
        public class Put
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents();

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                instructionalEventsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allInstructionalEventsDtos.ElementAt(0)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task UpdatesInstructionalEventByGuid()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock.Setup(x => x.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>())).ReturnsAsync(instructionalEvent);
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent2Async(guid)).ReturnsAsync(instructionalEvent);
                var result = await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
                Assert.AreEqual(result.Id, instructionalEvent.Id);
                Assert.AreEqual(result.Section, instructionalEvent.Section);
                Assert.AreEqual(result.InstructionalMethod, instructionalEvent.InstructionalMethod);
            }                   
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_PermissionsException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
                //await instructionalEventsController.PutHedmAsync("asdf", It.IsAny<InstructionalEvent2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_KeyNotFoundException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ArgumentNullException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_RepositoryException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_IntegrationApiException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_InvalidOperationException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ConfigurationException()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_Exception()
            {
                Dtos.InstructionalEvent2 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.PutHedmAsync(guid, instructionalEvent);
            }
            #endregion

        }

        [TestClass]
        public class Put_V8
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents3();

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                instructionalEventsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allInstructionalEventsDtos.ElementAt(0)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task UpdatesInstructionalEventByGuid()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock.Setup(x => x.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>())).ReturnsAsync(instructionalEvent);
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent3Async(guid)).ReturnsAsync(instructionalEvent);
                var result = await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
                Assert.AreEqual(result.Id, instructionalEvent.Id);
                Assert.AreEqual(result.Section, instructionalEvent.Section);
                Assert.AreEqual(result.InstructionalMethod, instructionalEvent.InstructionalMethod);
            }
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_PermissionsException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_KeyNotFoundException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ArgumentNullException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new ArgumentNullException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_RepositoryException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_IntegrationApiException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_InvalidOperationException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new InvalidOperationException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ConfigurationException()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_Exception()
            {
                Dtos.InstructionalEvent3 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.PutInstructionalEvent3Async(guid, instructionalEvent);
            }
            #endregion

        }

        [TestClass]
        public class Put_Post_V11
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent4> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents4();

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                instructionalEventsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                instructionalEventsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(allInstructionalEventsDtos.ElementAt(0)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task UpdatesInstructionalEventByGuid()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock.Setup(x => x.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>())).ReturnsAsync(instructionalEvent);
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ReturnsAsync(instructionalEvent);
                var result = await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
                Assert.AreEqual(result.Id, instructionalEvent.Id);
                Assert.AreEqual(result.Section, instructionalEvent.Section);
                Assert.AreEqual(result.InstructionalMethod, instructionalEvent.InstructionalMethod);
            }

            [TestMethod]
            public async Task PostdatesInstructionalEventByGuid()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                instructionalEvent.Id = Guid.Empty.ToString();
                
                sectionCoordinationServiceMock.Setup(x => x.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>())).ReturnsAsync(instructionalEvent);
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(It.IsAny<string>())).ReturnsAsync(instructionalEvent);
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
                Assert.AreEqual(result.Section, instructionalEvent.Section);
                Assert.AreEqual(result.InstructionalMethod, instructionalEvent.InstructionalMethod);
            }


            [TestMethod]
            public async Task UpdatesInstructionalEventByGuid_MeetingId_Null()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                instructionalEvent.Id = "";
                sectionCoordinationServiceMock.Setup(x => x.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>())).ReturnsAsync(instructionalEvent);
                sectionCoordinationServiceMock.Setup(x => x.GetInstructionalEvent4Async(guid)).ReturnsAsync(instructionalEvent);
                var result = await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
                Assert.AreEqual(result.Id, instructionalEvent.Id);
                Assert.AreEqual(result.Section, instructionalEvent.Section);
                Assert.AreEqual(result.InstructionalMethod, instructionalEvent.InstructionalMethod);
            }

            #region Exception Test PUT POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_Meeting_Null_HttpResponseException()
            {
                var result = await instructionalEventsController.PostInstructionalEvent4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_Meeting_Id_Null_HttpResponseException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                instructionalEvent.Id = "";
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_PermissionException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new PermissionsException());
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_ArgumentException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new ArgumentException());
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_RepositoryException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_IntegrationApiException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new IntegrationApiException());
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostdatesInstructionalEventByGuid_Exception()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new Exception());
                var result = await instructionalEventsController.PostInstructionalEvent4Async(instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_MessageBody_Null()
            {
                await instructionalEventsController.PutInstructionalEvent4Async(Guid.NewGuid().ToString(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_MeetingId_Id_Different()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = Guid.NewGuid().ToString();
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_Id_Null()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = string.Empty;
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_NillId_Null()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = Guid.Empty.ToString();
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_PermissionsException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_KeyNotFoundException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ArgumentNullException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new ArgumentNullException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_RepositoryException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_IntegrationApiException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_InvalidOperationException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new InvalidOperationException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_ConfigurationException()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PutInstructionalEventAsync_Exception()
            {
                Dtos.InstructionalEvent4 instructionalEvent = allInstructionalEventsDtos.ElementAt(0);
                string guid = instructionalEvent.Id;
                sectionCoordinationServiceMock
                    .Setup(s => s.UpdateInstructionalEvent4Async(It.IsAny<InstructionalEvent4>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.PutInstructionalEvent4Async(guid, instructionalEvent);
            }
            #endregion

        }

        [TestClass]
        public class Post
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents();

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task CreatesInstructionalEvent()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock.Setup(x => x.CreateInstructionalEvent2Async(iEvent)).ReturnsAsync(allInstructionalEventsDtos.ElementAt(0));

                var instructionalEvent = await instructionalEventsController.PostHedmAsync(iEvent);
                Assert.AreEqual(instructionalEvent.Id, iEvent.Id);
                Assert.AreEqual(instructionalEvent.Section, iEvent.Section);
                Assert.AreEqual(instructionalEvent.InstructionalMethod, iEvent.InstructionalMethod);
            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_PermissionsException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_KeyNotFoundException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ArgumentNullException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_RepositoryException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ArgumentException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new ArgumentException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_IntegrationApiException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_InvalidOperationException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_FormatException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new FormatException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ConfigurationException()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_Exception()
            {
                Dtos.InstructionalEvent2 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent2Async(It.IsAny<InstructionalEvent2>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.PostHedmAsync(iEvent);
            }
            #endregion
        }

        [TestClass]
        public class Post_V8
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent3> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents3();

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task CreatesInstructionalEvent()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);
                sectionCoordinationServiceMock.Setup(x => x.CreateInstructionalEvent3Async(iEvent)).ReturnsAsync(allInstructionalEventsDtos.ElementAt(0));

                var instructionalEvent = await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
                Assert.AreEqual(instructionalEvent.Id, iEvent.Id);
                Assert.AreEqual(instructionalEvent.Section, iEvent.Section);
                Assert.AreEqual(instructionalEvent.InstructionalMethod, iEvent.InstructionalMethod);
            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_PermissionsException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new PermissionsException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_KeyNotFoundException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ArgumentNullException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new ArgumentNullException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_RepositoryException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new RepositoryException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ArgumentException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new ArgumentException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_IntegrationApiException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new IntegrationApiException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_InvalidOperationException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new InvalidOperationException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_FormatException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new FormatException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_ConfigurationException()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new ConfigurationException());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_PostInstructionalEventAsync_Exception()
            {
                Dtos.InstructionalEvent3 iEvent = allInstructionalEventsDtos.ElementAt(0);

                sectionCoordinationServiceMock
                    .Setup(s => s.CreateInstructionalEvent3Async(It.IsAny<InstructionalEvent3>()))
                    .ThrowsAsync(new Exception());
                await instructionalEventsController.PostInstructionalEvent3Async(iEvent);
            }
            #endregion
        }

        [TestClass]
        public class Delete
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

            private InstructionalEventsController instructionalEventsController;

            private Mock<ISectionCoordinationService> sectionCoordinationServiceMock;
            private ISectionCoordinationService sectionCoordinationService;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.InstructionalEvent2> allInstructionalEventsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            string guid = "";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionCoordinationServiceMock = new Mock<ISectionCoordinationService>();
                sectionCoordinationService = sectionCoordinationServiceMock.Object;

                allInstructionalEventsDtos = BuildInstructionalEvents();
                guid = allInstructionalEventsDtos.ElementAt(1).Id;

                instructionalEventsController = new InstructionalEventsController(AdapterRegistry, sectionCoordinationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalEventsController = null;
                sectionCoordinationService = null;
            }

            [TestMethod]
            public async Task InstructionalEventsController_DeleteHedmInstructionalEventByGuid()
            {
                System.Net.Http.HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

                sectionCoordinationServiceMock.Setup(svc => svc.DeleteInstructionalEventAsync(guid)).Returns(Task.FromResult(httpResponseMessage));
                await instructionalEventsController.DeleteHedmAsync(guid);
            }

            #region Exception Test DELETE
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_PermissionsException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new PermissionsException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_KeyNotFoundException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_ArgumentNullException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new ArgumentNullException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteHedmInstructionalEventByGuid_ArgException()
            {
                sectionCoordinationServiceMock
                    .Setup(c => c.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new ArgumentException());
                await instructionalEventsController.DeleteHedmAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_IntegrationApiException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new IntegrationApiException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_RepositoryException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new RepositoryException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_ConfigurationException()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new ConfigurationException());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstructionalEventsController_DeleteInstructionalEventAsync_Exception()
            {
                sectionCoordinationServiceMock
                    .Setup(s => s.DeleteInstructionalEventAsync(It.IsAny<string>()))
                    .Throws(new Exception());
                await instructionalEventsController.DeleteHedmAsync(It.IsAny<string>());
            }
            #endregion
        }

        internal static IEnumerable<Dtos.InstructionalEvent2> BuildInstructionalEvents()
        {
            var instructionalEventDtos = new List<Dtos.InstructionalEvent2>();

            var instructionalEventDto1 = new Dtos.InstructionalEvent2()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" },
                //Status = new Dtos.SectionRegistrationStatus2()
                //{
                //    RegistrationStatus = Dtos.RegistrationStatus2.Registered,
                //    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered
                //},
                //Transcript = new Dtos.SectionRegistrationTranscript()
                //{
                //    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                //    Mode = Dtos.TranscriptMode.Standard
                //},
                //AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            var instructionalEventDto2 = new Dtos.InstructionalEvent2()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" },
                //Status = new Dtos.SectionRegistrationStatus2()
                //{
                //    RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered,
                //    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped
                //},
                //Transcript = new Dtos.SectionRegistrationTranscript()
                //{
                //    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                //    Mode = Dtos.TranscriptMode.Standard
                //},
                //AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            instructionalEventDtos.Add(instructionalEventDto1);
            instructionalEventDtos.Add(instructionalEventDto2);

            return instructionalEventDtos;
        }

        internal static IEnumerable<Dtos.InstructionalEvent3> BuildInstructionalEvents3()
        {
            var instructionalEventDtos = new List<Dtos.InstructionalEvent3>();

            var instructionalEventDto1 = new Dtos.InstructionalEvent3()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" }
            };

            var instructionalEventDto2 = new Dtos.InstructionalEvent3()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" }
            };

            instructionalEventDtos.Add(instructionalEventDto1);
            instructionalEventDtos.Add(instructionalEventDto2);

            return instructionalEventDtos;
        }

        internal static IEnumerable<Dtos.InstructionalEvent4> BuildInstructionalEvents4()
        {
            var instructionalEventDtos = new List<Dtos.InstructionalEvent4>();

            var instructionalEventDto1 = new Dtos.InstructionalEvent4()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" }
            };

            var instructionalEventDto2 = new Dtos.InstructionalEvent4()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                InstructionalMethod = new Dtos.GuidObject2() { Id = "abc123def456" }
            };

            instructionalEventDtos.Add(instructionalEventDto1);
            instructionalEventDtos.Add(instructionalEventDto2);

            return instructionalEventDtos;
        }
    }
}
