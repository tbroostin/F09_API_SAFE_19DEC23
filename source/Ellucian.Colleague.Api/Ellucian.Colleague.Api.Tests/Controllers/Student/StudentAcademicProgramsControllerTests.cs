// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicProgramsControllerTests
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

            private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
            private StudentAcademicProgramsController studentAcademicProgramsController;
            private IStudentAcademicProgramService studentAcademicProgramsService;
            private IAdapterRegistry adapterRegistry = null;
            private IEnumerable<StudentAcademicPrograms> studentAcadProgDtos;
            private ILogger logger = new Mock<ILogger>().Object;
            private Paging page;
            private int limit;
            private int offset;
            private Tuple<IEnumerable<StudentAcademicPrograms>, int> stuAcadProgDtosTuple;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                studentAcademicProgramsServiceMock = new Mock<IStudentAcademicProgramService>();

                studentAcademicProgramsService = studentAcademicProgramsServiceMock.Object;
                studentAcadProgDtos = StudentAcademicProgramsControllerTests.BuildstudentAcademicPrograms();
                string guid = studentAcadProgDtos.ElementAt(0).Id;

                studentAcademicProgramsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                studentAcademicProgramsController = new StudentAcademicProgramsController(adapterRegistry, studentAcademicProgramsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                studentAcademicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                limit = 200;
                offset = 0;
                page = new Paging(limit, offset);
                stuAcadProgDtosTuple = new Tuple<IEnumerable<StudentAcademicPrograms>, int>(studentAcadProgDtos, 3);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicProgramsServiceMock = null;
                studentAcademicProgramsService = null;
                studentAcademicProgramsController = null;
            }

            [TestMethod]
            public async Task ReturnsStudentAcademicProgramssByIdAsync()
            {
                studentAcademicProgramsController.Request.Headers.CacheControl = 
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                string guid = studentAcadProgDtos.ElementAt(0).Id;
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(studentAcadProgDtos.ElementAt(0));
                var studentAcademicPrograms = await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync(guid);
                var expected = studentAcademicPrograms;
                var actual = studentAcadProgDtos.ElementAt(0);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Program, actual.Program);
                Assert.AreEqual(expected.Student, actual.Student);
                Assert.AreEqual(expected.Site, actual.Site);
                Assert.AreEqual(expected.StartTerm, actual.StartTerm);
                Assert.AreEqual(expected.StartDate, actual.StartDate);
                Assert.AreEqual(expected.EndDate, actual.EndDate);
                Assert.AreEqual(expected.EnrollmentStatus.EnrollStatus, actual.EnrollmentStatus.EnrollStatus);
                Assert.AreEqual(expected.EnrollmentStatus.Detail.Id, actual.EnrollmentStatus.Detail.Id);
                var dispCnt = 0;
                foreach (var dis in expected.Disciplines)
                {
                    Assert.AreEqual(dis.Discipline.Id, actual.Disciplines[dispCnt].Discipline.Id);
                    dispCnt++;
                }
                var credCnt = 0;
                foreach (var dis in expected.Credentials)
                {
                    Assert.AreEqual(dis.Id, actual.Credentials[credCnt].Id);
                    credCnt++;
                }
            }

            [TestMethod]
            public async Task ReturnsstudentAcademicProgramssByAsyncCache()
            {
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAcademicProgramsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramsAsync(offset, limit, It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", "")).ReturnsAsync(stuAcadProgDtosTuple);
                var acadProg = await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await acadProg.ExecuteAsync(cancelToken);
                List<Dtos.StudentAcademicPrograms> studentAcademicPrograms = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms>>)httpResponseMessage.Content).Value as List<Dtos.StudentAcademicPrograms>;
                for (var i = 0; i < studentAcademicPrograms.Count; i++)
                {
                    var expected = studentAcadProgDtos.ToList()[i];
                    var actual = studentAcademicPrograms[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Program, actual.Program);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Site, actual.Site);
                    Assert.AreEqual(expected.StartTerm, actual.StartTerm);
                    Assert.AreEqual(expected.StartDate, actual.StartDate);
                    Assert.AreEqual(expected.EndDate, actual.EndDate);
                    Assert.AreEqual(expected.EnrollmentStatus.EnrollStatus, actual.EnrollmentStatus.EnrollStatus);
                    Assert.AreEqual(expected.EnrollmentStatus.Detail.Id, actual.EnrollmentStatus.Detail.Id);
                    var dispCnt = 0;
                    foreach (var dis in expected.Disciplines)
                    {
                        Assert.AreEqual(dis.Discipline.Id, actual.Disciplines[dispCnt].Discipline.Id);
                        dispCnt++;
                    }
                    var credCnt = 0;
                    foreach (var dis in expected.Credentials)
                    {
                        Assert.AreEqual(dis.Id, actual.Credentials[credCnt].Id);
                        credCnt++;
                    }
                }
            }

            [TestMethod]
            public async Task ReturnsstudentAcademicProgramssByAsyncNoCache()
            {
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAcademicProgramsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), true, "", "", "", "", "", "", "", "", "", "", "", "", "")).ReturnsAsync(stuAcadProgDtosTuple);
                var HttpAction = (await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAcademicPrograms> studentAcademicPrograms = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms>>)httpResponseMessage.Content).Value as List<Dtos.StudentAcademicPrograms>;
                for (var i = 0; i < studentAcademicPrograms.Count; i++)
                {
                    var expected = studentAcadProgDtos.ToList()[i];
                    var actual = studentAcademicPrograms[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Program, actual.Program);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Site, actual.Site);
                    Assert.AreEqual(expected.StartTerm, actual.StartTerm);
                    Assert.AreEqual(expected.StartDate, actual.StartDate);
                    Assert.AreEqual(expected.EndDate, actual.EndDate);
                    Assert.AreEqual(expected.EnrollmentStatus.EnrollStatus, actual.EnrollmentStatus.EnrollStatus);
                    Assert.AreEqual(expected.EnrollmentStatus.Detail.Id, actual.EnrollmentStatus.Detail.Id);
                    var dispCnt = 0;
                    foreach (var dis in expected.Disciplines)
                    {
                        Assert.AreEqual(dis.Discipline.Id, actual.Disciplines[dispCnt].Discipline.Id);
                        dispCnt++;
                    }
                    var credCnt = 0;
                    foreach (var dis in expected.Credentials)
                    {
                        Assert.AreEqual(dis.Id, actual.Credentials[credCnt].Id);
                        credCnt++;
                    }
                }
            }

            [TestMethod]
            public async Task ReturnsstudentAcademicProgramssByAsyncNoPaging()
            {
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                studentAcademicProgramsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), true, "", "", "", "", "", "", "", "", "", "", "", "", "")).ReturnsAsync(stuAcadProgDtosTuple);
                var HttpAction = (await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(null));
                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await HttpAction.ExecuteAsync(cancelToken);
                List<Dtos.StudentAcademicPrograms> studentAcademicPrograms = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms>>)httpResponseMessage.Content).Value as List<Dtos.StudentAcademicPrograms>;
                for (var i = 0; i < studentAcademicPrograms.Count; i++)
                {
                    var expected = studentAcadProgDtos.ToList()[i];
                    var actual = studentAcademicPrograms[i];
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Program, actual.Program);
                    Assert.AreEqual(expected.Student, actual.Student);
                    Assert.AreEqual(expected.Site, actual.Site);
                    Assert.AreEqual(expected.StartTerm, actual.StartTerm);
                    Assert.AreEqual(expected.StartDate, actual.StartDate);
                    Assert.AreEqual(expected.EndDate, actual.EndDate);
                    Assert.AreEqual(expected.EnrollmentStatus.EnrollStatus, actual.EnrollmentStatus.EnrollStatus);
                    Assert.AreEqual(expected.EnrollmentStatus.Detail.Id, actual.EnrollmentStatus.Detail.Id);
                    var dispCnt = 0;
                    foreach (var dis in expected.Disciplines)
                    {
                        Assert.AreEqual(dis.Discipline.Id, actual.Disciplines[dispCnt].Discipline.Id);
                        dispCnt++;
                    }
                    var credCnt = 0;
                    foreach (var dis in expected.Credentials)
                    {
                        Assert.AreEqual(dis.Id, actual.Credentials[credCnt].Id);
                        credCnt++;
                    }
                }
            }
            #region Exception Tests

            [TestMethod]
            public async Task GetStudentAcademicProgramsByGuidAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf")).Throws(new PermissionsException());
                try
                {
                    await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsByGuidAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            public async Task GetStudentAcademicProgramsAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new PermissionsException()); try
                {
                    await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_InvalidOperationException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new InvalidOperationException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_GetStudentAcademicProgramsAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            #endregion

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

            private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
            private StudentAcademicProgramsController studentAcademicProgramsController;
            private IStudentAcademicProgramService studentAcademicProgramsService;
            private IAdapterRegistry adapterRegistry = null;
            private List<StudentAcademicPrograms> stuAcadProgDtos;
            private ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                studentAcademicProgramsServiceMock = new Mock<IStudentAcademicProgramService>();

                studentAcademicProgramsService = studentAcademicProgramsServiceMock.Object;
                stuAcadProgDtos = StudentAcademicProgramsControllerTests.BuildstudentAcademicPrograms();

                studentAcademicProgramsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                studentAcademicProgramsController = new StudentAcademicProgramsController(adapterRegistry, studentAcademicProgramsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                studentAcademicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentAcademicProgramsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(stuAcadProgDtos.ElementAt(0)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicProgramsServiceMock = null;
                studentAcademicProgramsService = null;
                studentAcademicProgramsController = null;
            }

            [TestMethod]
            public async Task UpdateStudentAcademicProgramsAsync()
            {
                Dtos.StudentAcademicPrograms stuAcadProgs = stuAcadProgDtos.ElementAt(0);
                string guid = stuAcadProgs.Id;
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(stuAcadProgs);
                studentAcademicProgramsServiceMock.Setup(x => x.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicPrograms>())).ReturnsAsync(stuAcadProgs);
                var result = await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(guid, stuAcadProgs);
                Assert.AreEqual(result.Id, stuAcadProgs.Id);
                Assert.AreEqual(result.Program, stuAcadProgs.Program);
                Assert.AreEqual(result.Student, stuAcadProgs.Student);
                Assert.AreEqual(result.Site, stuAcadProgs.Site);
                Assert.AreEqual(result.StartTerm, stuAcadProgs.StartTerm);
                Assert.AreEqual(result.StartDate, stuAcadProgs.StartDate);
                Assert.AreEqual(result.EndDate, stuAcadProgs.EndDate);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, stuAcadProgs.EnrollmentStatus.EnrollStatus);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, stuAcadProgs.EnrollmentStatus.Detail.Id);
                var dispCnt = 0;
                foreach (var dis in result.Disciplines)
                {
                    Assert.AreEqual(dis.Discipline.Id, stuAcadProgs.Disciplines[dispCnt].Discipline.Id);
                    dispCnt++;
                }
                var credCnt = 0;
                foreach (var dis in result.Credentials)
                {
                    Assert.AreEqual(dis.Id, stuAcadProgs.Credentials[credCnt].Id);
                    credCnt++;
                }
            }

            [TestMethod]
            public async Task UpdateStudentAcademicProgramsAsync_noguid()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                string guid = stuAcadProg.Id;
                stuAcadProg.Id = null;
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(stuAcadProg);
                studentAcademicProgramsServiceMock.Setup(x => x.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicPrograms>())).ReturnsAsync(stuAcadProg);
                var result = await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(guid, stuAcadProg);
                Assert.AreEqual(result.Id, stuAcadProg.Id);
                Assert.AreEqual(result.Program, stuAcadProg.Program);
                Assert.AreEqual(result.Student, stuAcadProg.Student);
                Assert.AreEqual(result.Site, stuAcadProg.Site);
                Assert.AreEqual(result.StartTerm, stuAcadProg.StartTerm);
                Assert.AreEqual(result.StartDate, stuAcadProg.StartDate);
                Assert.AreEqual(result.EndDate, stuAcadProg.EndDate);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, stuAcadProg.EnrollmentStatus.EnrollStatus);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, stuAcadProg.EnrollmentStatus.Detail.Id);
                var dispCnt = 0;
                foreach (var dis in result.Disciplines)
                {
                    Assert.AreEqual(dis.Discipline.Id, stuAcadProg.Disciplines[dispCnt].Discipline.Id);
                    dispCnt++;
                }
                var credCnt = 0;
                foreach (var dis in result.Credentials)
                {
                    Assert.AreEqual(dis.Id, stuAcadProg.Credentials[credCnt].Id);
                    credCnt++;
                }

            }

            #region Exception Tests PUT
            [TestMethod]
            public async Task UpdateStudentAcademicProgramsAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new PermissionsException());
                try
                {
                    await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_PermissionsException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_Update_ArgumentNullException_guidmismatch()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB12345678", stuAcadProg);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_ArgumentNullException_MissingID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(null, stuAcadProg);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_ArgumentNullException_nullDTO()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(null))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("dfdsfh", It.IsAny<Dtos.StudentAcademicPrograms>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_ArgumentNullException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_ArgumentException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new ArgumentException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_KeyNotFoundException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_RepositoryException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_IntegrationApiException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_Exception()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_UpdateStudentAcademicProgramsAsync_EmptyGuid()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = Guid.Empty.ToString();
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(Guid.Empty.ToString(), stuAcadProg);
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

            private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
            private StudentAcademicProgramsController studentAcademicProgramsController;
            private IStudentAcademicProgramService studentAcademicProgramsService;
            private IAdapterRegistry adapterRegistry = null;
            private List<StudentAcademicPrograms> stuAcadProgDtos;
            private ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                studentAcademicProgramsServiceMock = new Mock<IStudentAcademicProgramService>();

                studentAcademicProgramsService = studentAcademicProgramsServiceMock.Object;
                stuAcadProgDtos = StudentAcademicProgramsControllerTests.BuildstudentAcademicPrograms();

                studentAcademicProgramsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
                studentAcademicProgramsController = new StudentAcademicProgramsController(adapterRegistry, studentAcademicProgramsService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                studentAcademicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAcademicProgramsServiceMock = null;
                studentAcademicProgramsService = null;
                studentAcademicProgramsController = null;
            }

            [TestMethod]
            public async Task CreateStudentAcademicProgramAsync()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = Guid.Empty.ToString();
                studentAcademicProgramsServiceMock.Setup(x => x.CreateStudentAcademicProgramAsync(stuAcadProg)).ReturnsAsync(stuAcadProgDtos.ElementAt(0));
                var result = await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
                Assert.AreEqual(result.Id, stuAcadProg.Id);
                Assert.AreEqual(result.Program, stuAcadProg.Program);
                Assert.AreEqual(result.Student, stuAcadProg.Student);
                Assert.AreEqual(result.Site, stuAcadProg.Site);
                Assert.AreEqual(result.StartTerm, stuAcadProg.StartTerm);
                Assert.AreEqual(result.StartDate, stuAcadProg.StartDate);
                Assert.AreEqual(result.EndDate, stuAcadProg.EndDate);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, stuAcadProg.EnrollmentStatus.EnrollStatus);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, stuAcadProg.EnrollmentStatus.Detail.Id);
                var dispCnt = 0;
                foreach (var dis in result.Disciplines)
                {
                    Assert.AreEqual(dis.Discipline.Id, stuAcadProg.Disciplines[dispCnt].Discipline.Id);
                    dispCnt++;
                }
                var credCnt = 0;
                foreach (var dis in result.Credentials)
                {
                    Assert.AreEqual(dis.Id, stuAcadProg.Credentials[credCnt].Id);
                    credCnt++;
                }
            }

            #region Exception Tests POST
            [TestMethod]
            public async Task CreateStudentAcademicProgramAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new PermissionsException());
                try
                {
                    Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                    stuAcadProg.Id = Guid.Empty.ToString();
                    await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new PermissionsException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new ArgumentNullException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullGuid()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullProgramID()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Program = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullPayload()
            {
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullStudent()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Student = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_GradDateBeforeStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.GraduatedOn = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_CredDateBeforeStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.CredentialsDate = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullEnrollmentStatus()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_CompleteEnrollmentStatus()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_InactiveWithoutEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                stuAcadProg.EndDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_CompleteWithoutEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                stuAcadProg.EndDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_ActiveWithEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                stuAcadProg.EndDate = DateTime.Today;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_EndDateBeforeStart()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.EndDate = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NoCredentials()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NoRecognitionsID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullDisciplineID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = null } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullDisciplinesAdministeringInstitutionUnitID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1234" }, AdministeringInstitutionUnit = new GuidObject2() { Id = null } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_NullDiscipline()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_BadDisciplineSubDiscipline()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                var subDisp = new List<GuidObject2>() { new GuidObject2() { Id = "1234" } };
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { SubDisciplines = subDisp, Discipline = new GuidObject2() { Id = "fd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "AIU12134324" } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(It.IsAny<Dtos.StudentAcademicPrograms>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new RepositoryException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new IntegrationApiException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentAcademicProgramssController_CreateStudentAcademicProgramAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>()))
                    .ThrowsAsync(new Exception());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }


            #endregion

        }
        private static List<StudentAcademicPrograms> BuildstudentAcademicPrograms()
        {
            var studentAcademicProgramsDtos = new List<Dtos.StudentAcademicPrograms>();
            var stuAcadProgDto1 = new Dtos.StudentAcademicPrograms()
            {
                Id = "AB1234567890",
                Program = new Dtos.GuidObject2() { Id = "P12345678910" },
                Catalog = new Dtos.GuidObject2() { Id = "C12345678910" },
                Student = new Dtos.GuidObject2() { Id = "S12345678910" },
                Site = new Dtos.GuidObject2() { Id = "L12345678910" },
                AcademicLevel = new Dtos.GuidObject2() { Id = "AL1234567890" },
                ProgramOwner = new Dtos.GuidObject2() { Id = "PO1234567890" },
                StartTerm = new Dtos.GuidObject2() { Id = "S12345678910" },
                Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } },
                Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1df164eb-8178-5678-a9f7-24f27f3991d8" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "ALU12344556778" } } },
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } },
                PerformanceMeasure = "3.0",
                Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = "REC13453545" } },
                ThesisTitle = "thesis title",
                CreditsEarned = 30m,
                StartDate = DateTimeOffset.Now
            };
            var stuAcadProgDto2 = new Dtos.StudentAcademicPrograms()
            {
                Id = "BC1234567890",
                Program = new Dtos.GuidObject2() { Id = "P12345678910" },
                Catalog = new Dtos.GuidObject2() { Id = "C12345678910" },
                Student = new Dtos.GuidObject2() { Id = "S12345678910" },
                Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } },
                Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1df164eb-8178-5678-a9f7-24f27f3991d8" } } },
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Inactive, Detail = new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } }

            };
            var stuAcadProgDto3 = new Dtos.StudentAcademicPrograms()
            {
                Id = "CD1234567890",
                Program = new Dtos.GuidObject2() { Id = "P12345678910" },
                Catalog = new Dtos.GuidObject2() { Id = "C12345678910" },
                Student = new Dtos.GuidObject2() { Id = "S12345678910" },
                Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } },
                Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1df164eb-8178-5678-a9f7-24f27f3991d8" } } },
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Complete, Detail = new GuidObject2() { Id = "1df164eb-8178-4321-a9f7-24f27f3991d8" } }

            };
            studentAcademicProgramsDtos.Add(stuAcadProgDto1);
            studentAcademicProgramsDtos.Add(stuAcadProgDto2);
            studentAcademicProgramsDtos.Add(stuAcadProgDto3);
            return studentAcademicProgramsDtos;
        }

    }

    [TestClass]
    public class StudentAcademicProgramsControllerTests_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicProgramsController studentAcademicProgramController;

        private StudentAcademicPrograms2 studentAcademicProgram;
        private Tuple<IEnumerable<StudentAcademicPrograms2>, int> tupleResult;

        private Paging paging = new Paging(10, 0);

        private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            studentAcademicProgramsServiceMock = new Mock<IStudentAcademicProgramService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            InitializeTestData();

            studentAcademicProgramsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ReturnsAsync(tupleResult);
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ReturnsAsync(studentAcademicProgram);

            studentAcademicProgramController = new StudentAcademicProgramsController(adapterRegistryMock.Object, studentAcademicProgramsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            studentAcademicProgramController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            studentAcademicProgramController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(studentAcademicProgram));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            studentAcademicProgramsServiceMock = null;
            studentAcademicProgramController = null;
        }

        private void InitializeTestData()
        {
            studentAcademicProgram = new StudentAcademicPrograms2()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.Outcome,
                CredentialsDate = DateTime.Today.AddDays(10),
                StartDate = DateTime.Today,
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Recognitions = new List<GuidObject2>() { new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines>()
                {
                    new StudentAcademicProgramDisciplines()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            tupleResult = new Tuple<IEnumerable<StudentAcademicPrograms2>, int>(new List<StudentAcademicPrograms2>() { studentAcademicProgram }, 1);
        }

        #endregion

        #region GET & GET BY ID
        [TestMethod]
        public async Task GetByGuid_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_PermissionsException()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_KeyNotFoundException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_ArgumentNullException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_As_Null_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetByGuid()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_Invalid_Criteria_Key()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetAll_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            //var criteria = @"{'site':{'id':'7a6e9b82-e78b-47db-9f30-5becff004921'}}";
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicPrograms2Async(null, It.IsAny<QueryStringFilter>());
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_PermissionException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new PermissionsException());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_InvalidOperationException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new ArgumentException());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new RepositoryException());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<string>())).ThrowsAsync(new Exception());

            await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_With_Empty_Criteria()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }
        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_As_Null()
        {
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_DtoId_As_Null()
        {
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(new StudentAcademicPrograms2() { Id = null });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_AcademicProgram_As_Null()
        {
            studentAcademicProgram.AcademicProgram = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_AcademicProgramId_As_Null()
        {
            studentAcademicProgram.AcademicProgram.Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_Student_As_Null()
        {
            studentAcademicProgram.Student = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_StudentId_As_Null()
        {
            studentAcademicProgram.Student.Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_CurriculumObjective_As_Notset()
        {
            studentAcademicProgram.CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective.NotSet;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_CredentialsDate_As_Null()
        {
            studentAcademicProgram.CredentialsDate = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_StartDate_As_Null()
        {
            studentAcademicProgram.StartDate = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_EndDate_LesserThan_StartDate()
        {
            studentAcademicProgram.EndDate = DateTime.Today.AddDays(-10);
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_ExpectedGraduationDate_LesserThan_StartDate()
        {
            studentAcademicProgram.ExpectedGraduationDate = DateTime.Today.AddDays(-10);
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_GraduatedOn_LesserThan_StartDate()
        {
            studentAcademicProgram.GraduatedOn = DateTime.Today.AddDays(-10);
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_CredentialsDate_LesserThan_StartDate()
        {
            studentAcademicProgram.CredentialsDate = DateTime.Today.AddDays(-10);
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_EndDate_As_Null_With_InactiveStatus()
        {
            studentAcademicProgram.EndDate = null;
            studentAcademicProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_EnrollmentStatus_As_Complete()
        {
            studentAcademicProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_EndDate_With_ActiveStatus()
        {
            studentAcademicProgram.EndDate = DateTime.Today.AddDays(10);
            studentAcademicProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_CredentialsId_As_Null()
        {
            studentAcademicProgram.Credentials.FirstOrDefault().Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_RecognitionsId_As_Null()
        {
            studentAcademicProgram.Recognitions.FirstOrDefault().Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_Discipline_As_Null()
        {
            studentAcademicProgram.Disciplines.FirstOrDefault().Discipline = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_DisciplineId_As_Null()
        {
            studentAcademicProgram.Disciplines.FirstOrDefault().Discipline.Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_AdministeringInstitutionUnitId_As_Null()
        {
            studentAcademicProgram.Disciplines.FirstOrDefault().AdministeringInstitutionUnit.Id = null;
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Dto_SubDisciplines_Not_Null()
        {
            studentAcademicProgram.Disciplines.FirstOrDefault().SubDisciplines = new List<GuidObject2>() { new GuidObject2(guid) };
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        public async Task Create2_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new PermissionsException());
            try
            {
                studentAcademicProgram.Id = Guid.Empty.ToString();
                await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_PermissionsException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new ArgumentException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_Create()
        {
            studentAcademicProgram.Id = Guid.Empty.ToString();
            var result = await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
            Assert.IsNotNull(result);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_Id_As_Null()
        {
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(null, new StudentAcademicPrograms2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_Dto_As_Null()
        {
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_DtoId_And_Id_AreNotSame()
        {
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(Guid.NewGuid().ToString(), studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_DtoId_As_Empty_Guid()
        {
            studentAcademicProgram.Id = string.Empty;
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(Guid.Empty.ToString(), studentAcademicProgram);
        }

        [TestMethod]
        public async Task Update2_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Unauthorized, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_PermissionException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_ArgumentNullException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new ArgumentNullException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new ArgumentException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_Update()
        {
            var result = await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
            Assert.IsNotNull(result);
        }

        #endregion
    }
}