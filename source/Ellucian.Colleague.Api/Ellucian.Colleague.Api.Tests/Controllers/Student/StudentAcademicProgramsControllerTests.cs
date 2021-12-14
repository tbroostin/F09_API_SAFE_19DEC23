﻿// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicProgramsControllerTests_V6
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
                studentAcadProgDtos = BuildstudentAcademicPrograms();
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
            public async Task StudentAcademicProgramController_ReturnsStudentAcademicProgramssByIdAsync()
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
            public async Task StudentAcademicProgramController_ReturnsstudentAcademicProgramssByAsyncCache()
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
            public async Task StudentAcademicProgramController_ReturnsstudentAcademicProgramssByAsyncNoCache()
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
            public async Task StudentAcademicProgramController_ReturnsstudentAcademicProgramssByAsyncNoPaging()
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
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_PermissionsException_HttpUnauthorized()
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
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsByGuidAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf"))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
            }

            [TestMethod]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_PermissionsException_HttpUnauthorized()
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
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_InvalidOperationException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new InvalidOperationException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_GetStudentAcademicProgramsAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", ""))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
            }

            //Get v6.0.0
            //Successful
            //GetStudentAcademicProgramsAsync

            [TestMethod]
            public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramsAsync(offset, limit, It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", "")).ReturnsAsync(stuAcadProgDtosTuple);

                var acadProg = await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);

                Object filterObject;
                studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

            }

            //Get v6.0.0
            //Exception
            //GetStudentAcademicProgramsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "", "", "", "", "", "", "", "")).ThrowsAsync(new PermissionsException());
                    studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                    await studentAcademicProgramsController.GetStudentAcademicProgramsAsync(page);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Get by id v6.0.0
            //Successful
            //GetStudentAcademicProgramsByGuidAsync

            [TestMethod]
            public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuidAsync_Permissions()
            {
                string guid = studentAcadProgDtos.ElementAt(0).Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {

                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicProgramsServiceMock.Setup(x => x.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(studentAcadProgDtos.ElementAt(0));
                var studentAcademicPrograms = await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync(guid);

                Object filterObject;
                studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

            }

            //Get by id v6.0.0
            //Exception
            //GetStudentAcademicProgramsByGuidAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync("asdf")).ThrowsAsync(new PermissionsException());
                    studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                    await studentAcademicProgramsController.GetStudentAcademicProgramsByGuidAsync("asdf");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
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
                stuAcadProgDtos = BuildstudentAcademicPrograms();

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
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync()
            {
                Dtos.StudentAcademicPrograms stuAcadProgs = stuAcadProgDtos.ElementAt(0);
                string guid = stuAcadProgs.Id;
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(stuAcadProgs);
                studentAcademicProgramsServiceMock.Setup(x => x.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicPrograms>(), It.IsAny<bool>())).ReturnsAsync(stuAcadProgs);
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
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_noguid()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                string guid = stuAcadProg.Id;
                stuAcadProg.Id = null;
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid)).ReturnsAsync(stuAcadProg);
                studentAcademicProgramsServiceMock.Setup(x => x.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicPrograms>(),It.IsAny<bool>())).ReturnsAsync(stuAcadProg);
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
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                try
                {
                    await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
                }
                catch (HttpResponseException e)
                {
                    statusCode = e.Response.StatusCode;
                }
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_PermissionsException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_Update_ArgumentNullException_guidmismatch()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB12345678", stuAcadProg);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_ArgumentNullException_MissingID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(null, stuAcadProg);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_ArgumentNullException_nullDTO()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(null, It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("dfdsfh", It.IsAny<Dtos.StudentAcademicPrograms>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_ArgumentNullException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_ArgumentException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_KeyNotFoundException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_RepositoryException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_IntegrationApiException()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_Exception()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                studentAcademicProgramsServiceMock
                    .Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_UpdateStudentAcademicProgramsAsync_EmptyGuid()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = Guid.Empty.ToString();
                await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(Guid.Empty.ToString(), stuAcadProg);
            }

            //Put v6.0.0
            //Successful
            //UpdateStudentAcademicProgramsAsync

            [TestMethod]
            public async Task StudentAcademicProgramsController_UpdateStudentAcademicProgramsAsync_Permissions()
            {
                Dtos.StudentAcademicPrograms stuAcadProgs = stuAcadProgDtos.ElementAt(0);
                string guid = stuAcadProgs.Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicProgramsServiceMock.Setup(x => x.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicPrograms>(), It.IsAny<bool>())).ReturnsAsync(stuAcadProgs);

                var result = await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync(guid, stuAcadProgs);

                Object filterObject;
                studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

            }

            //Put v6.0.0
            //Exception
            //UpdateStudentAcademicProgramsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramsController_UpdateStudentAcademicProgramsAsync_Invalid_Permissions()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update student-academic-programs."));
                    await studentAcademicProgramsController.UpdateStudentAcademicProgramsAsync("AB1234567890", stuAcadProg);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
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
                stuAcadProgDtos = BuildstudentAcademicPrograms();

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
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = Guid.Empty.ToString();
                studentAcademicProgramsServiceMock.Setup(x => x.CreateStudentAcademicProgramAsync(stuAcadProg, It.IsAny<bool>())).ReturnsAsync(stuAcadProgDtos.ElementAt(0));
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
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_PermissionsException_HttpUnauthorized()
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
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
                Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramssController_CreateStudentAcademicProgramAsync_PermissionsException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_ArgumentNullException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullGuid()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullProgramID()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Program = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullPayload()
            {
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullStudent()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Student = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_GradDateBeforeStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.GraduatedOn = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_CredDateBeforeStartDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.CredentialsDate = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullEnrollmentStatus()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_CompleteEnrollmentStatus()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_InactiveWithoutEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                stuAcadProg.EndDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_CompleteWithoutEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                stuAcadProg.EndDate = null;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_ActiveWithEndDate()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                stuAcadProg.EndDate = DateTime.Today;
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_EndDateBeforeStart()
            {

                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
                stuAcadProg.EndDate = new DateTimeOffset(DateTime.Parse("01/02/2018"));
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NoCredentials()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NoRecognitionsID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullDisciplineID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = null } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullDisciplinesAdministeringInstitutionUnitID()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "1234" }, AdministeringInstitutionUnit = new GuidObject2() { Id = null } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_NullDiscipline()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { Discipline = null } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_BadDisciplineSubDiscipline()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                var subDisp = new List<GuidObject2>() { new GuidObject2() { Id = "1234" } };
                stuAcadProg.Disciplines = new List<StudentAcademicProgramDisciplines>() { new StudentAcademicProgramDisciplines() { SubDisciplines = subDisp, Discipline = new GuidObject2() { Id = "fd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "AIU12134324" } } };
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_KeyNotFoundException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(It.IsAny<Dtos.StudentAcademicPrograms>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_RepositoryException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_IntegrationApiException()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramController_CreateStudentAcademicProgramAsync_Exception()
            {
                studentAcademicProgramsServiceMock
                    .Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
            }

            //Post v11.0.0
            //Successful
            //CreateStudentAcademicProgramsAsync

            [TestMethod]
            public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsAsync_Permissions()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                stuAcadProg.Id = Guid.Empty.ToString();
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);
                studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                studentAcademicProgramsServiceMock.Setup(x => x.CreateStudentAcademicProgramAsync(stuAcadProg, It.IsAny<bool>())).ReturnsAsync(stuAcadProgDtos.ElementAt(0));

                var result = await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);

                Object filterObject;
                studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

            }

            //Post v11.0.0
            //Exception
            //CreateStudentAcademicProgramsAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsAsync_Invalid_Permissions()
            {
                Dtos.StudentAcademicPrograms stuAcadProg = stuAcadProgDtos.ElementAt(0);
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsAsync" }
                };
                HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                studentAcademicProgramsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = studentAcademicProgramsController.ControllerContext;
                var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    
                    studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Dtos.StudentAcademicPrograms>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create student-academic-programs."));
                    await studentAcademicProgramsController.CreateStudentAcademicProgramsAsync(stuAcadProg);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
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
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);

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
        public async Task StudentAcademicProgramController_GetByGuid_PermissionsException_HttpUnauthorized()
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
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
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
        public async Task StudentAcademicProgramController_GetAll_PermissionsException_HttpUnauthorized()
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
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
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

        //Get v11.0.0
        //Successful
        //GetStudentAcademicPrograms2Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms2Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                            It.IsAny<string>())).ReturnsAsync(tupleResult);
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms2Async(paging, It.IsAny<QueryStringFilter>());

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Get v11.0.0
        //Exception
        //GetStudentAcademicPrograms2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                                It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicPrograms2Async(null, It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get by id v11.0.0
        //Successful
        //GetStudentAcademicProgramsByGuid2Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid2Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Get by id v11.0.0
        //Exception
        //GetStudentAcademicProgramsByGuid2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid2Async(guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
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
        public async Task StudentAcademicProgramController_Create2_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            try
            {
                studentAcademicProgram.Id = Guid.Empty.ToString();
                await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_PermissionsException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Create_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_Create()
        {
            studentAcademicProgram.Id = Guid.Empty.ToString();
            var result = await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
            Assert.IsNotNull(result);
        }

        //Post v11.0.0
        //Successful
        //CreateStudentAcademicPrograms2Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicPrograms2Async_Permissions()
        {
            studentAcademicProgram.Id = Guid.Empty.ToString();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);

            var result = await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Post v11.0.0
        //Exception
        //CreateStudentAcademicPrograms2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicPrograms2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create student-academic-programs."));
                await studentAcademicProgramController.CreateStudentAcademicPrograms2Async(studentAcademicProgram);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
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
        public async Task StudentAcademicProgramController_Update2_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_PermissionException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_ArgumentNullException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_Update_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_Update()
        {
            var result = await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
            Assert.IsNotNull(result);
        }

        //Put v11.0.0
        //Successful
        //UpdateStudentAcademicPrograms2Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_UpdateStudentAcademicPrograms2Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);

            var result = await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Put v11.0.0
        //Exception
        //UpdateStudentAcademicPrograms2Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_UpdateStudentAcademicPrograms2Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicPrograms2Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicPrograms2>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-academic-programs."));
                await studentAcademicProgramController.UpdateStudentAcademicPrograms2Async(guid, studentAcademicProgram);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion
    }

    [TestClass]
    public class StudentAcademicProgramsControllerTests_V16_0_0
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicProgramsController studentAcademicProgramController;

        private StudentAcademicPrograms3 studentAcademicProgram;
        private Tuple<IEnumerable<StudentAcademicPrograms3>, int> tupleResult;

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
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ReturnsAsync(tupleResult);
         
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
            studentAcademicProgram = new StudentAcademicPrograms3()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,
                CredentialsDate = DateTime.Today.AddDays(10),
               
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Recognitions = new List<GuidObject2>() { new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            tupleResult = new Tuple<IEnumerable<StudentAcademicPrograms3>, int>(new List<StudentAcademicPrograms3>() { studentAcademicProgram }, 1);
        }

        #endregion

        #region GET & GET BY ID
        [TestMethod]
        public async Task StudentAcademicProgramController_GetByGuid_V16_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_PermissionsException()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_KeyNotFoundException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_ArgumentNullException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V16_As_Null_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetByGuid_V16()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V16_Invalid_Criteria_Key()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V16_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicPrograms3Async(null, It.IsAny<QueryStringFilter>());
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_PermissionException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                 It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_InvalidOperationException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                 It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V16_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(),
                It.IsAny<bool>())).ThrowsAsync(new Exception());

            await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V16()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V16_With_Empty_Criteria()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        //Get v16.0.0
        //Successful
        //GetStudentAcademicPrograms3Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms3Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms3Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(), It.IsAny<bool>())).ReturnsAsync(tupleResult);
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms3Async(paging, It.IsAny<QueryStringFilter>());

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Get v16.0.0
        //Exception
        //GetStudentAcademicPrograms3Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms3Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms3Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms3>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); 
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicPrograms3Async(null, It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get by id v16.0.0
        //Successful
        //GetStudentAcademicProgramsByGuid3Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid3Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid3Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Get by id v16.0.0
        //Exception
        //GetStudentAcademicProgramsByGuid3Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid3Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid3Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid3Async(guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region PUT/POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_CreateStudentAcademicPrograms3Async_Exception()
        {
            await studentAcademicProgramController.CreateStudentAcademicPrograms3Async(studentAcademicProgram);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_UpdateStudentAcademicPrograms3Async_Exception()
        {
            await studentAcademicProgramController.UpdateStudentAcademicPrograms3Async(guid, studentAcademicProgram);
        }

        #endregion

    }

    [TestClass]
    public class StudentAcademicProgramsControllerTests_V17_0_0
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IStudentAcademicProgramService> studentAcademicProgramsServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicProgramsController studentAcademicProgramController;

        private StudentAcademicPrograms4 studentAcademicProgram;
        private Tuple<IEnumerable<StudentAcademicPrograms4>, int> tupleResult;

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
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tupleResult);

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
            studentAcademicProgram = new StudentAcademicPrograms4()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            tupleResult = new Tuple<IEnumerable<StudentAcademicPrograms4>, int>(new List<StudentAcademicPrograms4>() { studentAcademicProgram }, 1);
        }

        #endregion

        #region GET & GET BY ID
        [TestMethod]
        public async Task StudentAcademicProgramController_GetByGuid_V17_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_PermissionsException()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_KeyNotFoundException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_ArgumentNullException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetByGuid_V17_As_Null_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetByGuid_V17()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V17_Invalid_Criteria_Key()
        {
            studentAcademicProgramController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V17_PermissionsException_HttpUnauthorized()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            try
            {
                await studentAcademicProgramController.GetStudentAcademicPrograms4Async(null, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_PermissionException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                 It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_InvalidOperationException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                 It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_ArgumentException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                  It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_RepositoryException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                 It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_IntegrationApiException()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                 It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramController_GetAll_V17_Exception()
        {
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                 It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V17()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task StudentAcademicProgramController_GetAll_V17_With_Empty_Criteria()
        {
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            Assert.IsNotNull(result);
        }
        #endregion

        //Get v17.0.0
        //Successful
        //GetStudentAcademicPrograms4Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms4Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms4Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tupleResult);
            var result = await studentAcademicProgramController.GetStudentAcademicPrograms4Async(paging, It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Get v17.0.0
        //Exception
        //GetStudentAcademicPrograms4Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicPrograms4Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicPrograms4Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicPrograms4Async(null, It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get by id v17.0.0
        //Successful
        //GetStudentAcademicProgramsByGuid4Async

        [TestMethod]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid4Async_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid4Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);
            studentAcademicProgramController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentAcademicProgramConsent, StudentPermissionCodes.CreateStudentAcademicProgramConsent });

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            var result = await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);

            Object filterObject;
            studentAcademicProgramController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));
        }

        //Get by id v17.0.0
        //Exception
        //GetStudentAcademicProgramsByGuid4Async
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_GetStudentAcademicProgramsByGuid4Async_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "GetStudentAcademicProgramsByGuid4Async" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramController.ControllerContext;
            var actionDescriptor = studentAcademicProgramController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                studentAcademicProgramsServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-academic-programs."));
                await studentAcademicProgramController.GetStudentAcademicProgramsByGuid4Async(guid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }



    }


    [TestClass]

    public class StudentAcademicProgramsSubmissionsControllerTests
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IStudentAcademicProgramService> studentAcademicProgramServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicProgramsController studentAcademicProgramsController;

        private StudentAcademicPrograms4 studentAcademicProgram;
        private StudentAcademicProgramsSubmissions studentAcademicProgramSubmissionDto;
        private StudentAcademicProgramsSubmissions post_studentAcademicProgramSubmissionDto;
        private Tuple<IEnumerable<StudentAcademicPrograms4>, int> tupleResult;

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
            studentAcademicProgramServiceMock = new Mock<IStudentAcademicProgramService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            InitializeTestData();

            studentAcademicProgramServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            studentAcademicProgramServiceMock.Setup(s => s.GetStudentAcademicProgramByGuid4Async(It.IsAny<string>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramServiceMock.Setup(s => s.GetStudentAcademicProgramSubmissionByGuidAsync(It.IsAny<string>())).ReturnsAsync(studentAcademicProgramSubmissionDto);
            studentAcademicProgramServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tupleResult);
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);

            studentAcademicProgramsController = new StudentAcademicProgramsController(adapterRegistryMock.Object, studentAcademicProgramServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            studentAcademicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            studentAcademicProgramsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(studentAcademicProgram));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            studentAcademicProgramServiceMock = null;
            studentAcademicProgramsController = null;
        }

        private void InitializeTestData()
        {
            studentAcademicProgram = new StudentAcademicPrograms4()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            studentAcademicProgramSubmissionDto = new StudentAcademicProgramsSubmissions()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            post_studentAcademicProgramSubmissionDto = new StudentAcademicProgramsSubmissions()
            {
                Id = Guid.Empty.ToString(),
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            tupleResult = new Tuple<IEnumerable<StudentAcademicPrograms4>, int>(new List<StudentAcademicPrograms4>() { studentAcademicProgram }, 1);
        }

        #endregion

        #region Exceptions POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_NullRequestBody()
        {
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_Id_Null()
        {
            post_studentAcademicProgramSubmissionDto.Id = "";
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_Not_Nil_Guid()
        {
            post_studentAcademicProgramSubmissionDto.Id = guid;
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_KeyNotFoundException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_PermissionsException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_ArgumentException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_RepositoryException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_IntegrationApiException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_InvalidOperationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_ConfigurationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new ConfigurationException());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_Exception()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var actual = await studentAcademicProgramsController. CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
        }

        //Post v17.0.0
        //Successful
        //CreateStudentAcademicProgramsSubmissionsAsync

        [TestMethod]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsSubmissionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);
            studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);

            Object filterObject;
            studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Post v17.0.0
        //Exception
        //CreateStudentAcademicProgramsSubmissionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsSubmissionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create student-academic-programs-submissions."));
                var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }



        #endregion POST


        #region Exceptions PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_Null_UrlId()
        {
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync("", studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_RequestBody_Null()
        {
            studentAcademicProgramSubmissionDto = new StudentAcademicProgramsSubmissions()
            {
                Id = ""
            };
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_RequestBody_differentId()
        {
            studentAcademicProgramSubmissionDto.Id = "1234";
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_RequestBody_Nil_Id()
        {
            studentAcademicProgramSubmissionDto.Id = Guid.Empty.ToString();
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(Guid.Empty.ToString(), studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_KeyNotFoundException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_PermissionsException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_ArgumentException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_RepositoryException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_IntegrationApiException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_InvalidOperationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_ConfigurationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new ConfigurationException());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync_Exception()
        {
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
        }

        //Put v17.0.0
        //Successful
        //UpdateStudentAcademicProgramsSubmissionsAsync

        [TestMethod]
        public async Task StudentAcademicProgramsController_UpdateStudentAcademicProgramsSubmissionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicProgramsSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);
            studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);

            Object filterObject;
            studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Put v17.0.0
        //Exception
        //UpdateStudentAcademicProgramsSubmissionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_UpdateStudentAcademicProgramsSubmissionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "UpdateStudentAcademicProgramsSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                studentAcademicProgramServiceMock.Setup(s => s.UpdateStudentAcademicProgramSubmissionAsync(It.IsAny<StudentAcademicProgramsSubmissions>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-academic-programs-submissions."));
                var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }



        #endregion

        #region PUT/POST 

        [TestMethod]
        public async Task UpdateStudentAcademicProgramsSubmissionsAsync()
        {
            var actual = await studentAcademicProgramsController.UpdateStudentAcademicProgramsSubmissionsAsync(guid, studentAcademicProgramSubmissionDto);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync()
        {
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsSubmissionsAsync(post_studentAcademicProgramSubmissionDto);
            Assert.IsNotNull(actual);
        }

        #endregion

    }

    [TestClass]

    public class StudentAcademicProgramsReplacementsControllerTests
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IStudentAcademicProgramService> studentAcademicProgramServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicProgramsController studentAcademicProgramsController;

        private StudentAcademicPrograms4 studentAcademicProgram;
        private StudentAcademicProgramReplacements studentAcademicProgramReplacementsDto;
        private StudentAcademicProgramReplacements post_studentAcademicProgramReplacementsDto;
        private Tuple<IEnumerable<StudentAcademicPrograms4>, int> tupleResult;

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
            studentAcademicProgramServiceMock = new Mock<IStudentAcademicProgramService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            InitializeTestData();

            studentAcademicProgramServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            studentAcademicProgramServiceMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentAcademicPrograms4>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tupleResult);
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);

            studentAcademicProgramsController = new StudentAcademicProgramsController(adapterRegistryMock.Object, studentAcademicProgramServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            studentAcademicProgramsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            studentAcademicProgramServiceMock = null;
            studentAcademicProgramsController = null;
        }

        private void InitializeTestData()
        {
            studentAcademicProgram = new StudentAcademicPrograms4()
            {
                Id = guid,
                AcademicProgram = new GuidObject2(guid),
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                {
                    new StudentAcademicProgramDisciplines2()
                    {
                        Discipline = new GuidObject2(guid),
                        AdministeringInstitutionUnit = new GuidObject2(guid)
                    }
                }
            };

            studentAcademicProgramReplacementsDto = new StudentAcademicProgramReplacements()
            {
                Id = guid,
                Student = new GuidObject2(guid),
                ProgramToReplace = new GuidObject2(guid),
                NewProgram = new StudentAcademicProgramsReplacementsNewProgram()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                    Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                    Disciplines = new List<StudentAcademicProgramDisciplines2>()
                    {
                        new StudentAcademicProgramDisciplines2()
                        {
                            Discipline = new GuidObject2(guid),
                            AdministeringInstitutionUnit = new GuidObject2(guid)
                        }
                    }
                }
            };

            post_studentAcademicProgramReplacementsDto = new StudentAcademicProgramReplacements()
            {
                Id = Guid.Empty.ToString(),
                Student = new GuidObject2(guid),
                NewProgram = new StudentAcademicProgramsReplacementsNewProgram()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome,

                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2(guid) },
                    Credentials = new List<GuidObject2>() { new GuidObject2(guid) },
                    Disciplines = new List<StudentAcademicProgramDisciplines2>()
                    {
                        new StudentAcademicProgramDisciplines2()
                        {
                            Discipline = new GuidObject2(guid),
                            AdministeringInstitutionUnit = new GuidObject2(guid)
                        }
                    }
                }
            };

            tupleResult = new Tuple<IEnumerable<StudentAcademicPrograms4>, int>(new List<StudentAcademicPrograms4>() { studentAcademicProgram }, 1);
        }

        #endregion

        #region Exceptions POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_NullRequestBody()
        {
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_Id_Null()
        {
            post_studentAcademicProgramReplacementsDto.Id = "";
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_Not_Nil_Guid()
        {
            post_studentAcademicProgramReplacementsDto.Id = guid;
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_KeyNotFoundException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsSubmissionsAsync_PermissionsException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_ArgumentException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_RepositoryException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_IntegrationApiException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_InvalidOperationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_ConfigurationException()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new ConfigurationException());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CreateStudentAcademicProgramsReplacementsAsync_Exception()
        {
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
        }


        //Post v17.0.0
        //Successful
        //CreateStudentAcademicProgramsReplacementsAsync

        [TestMethod]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsReplacementsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsReplacementsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-credentials", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);
            studentAcademicProgramsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.CreateStudentAcademicProgramConsent);

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgram);
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);

            Object filterObject;
            studentAcademicProgramsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

        }

        //Post v17.0.0
        //Exception
        //CreateStudentAcademicProgramsReplacementsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicProgramsController_CreateStudentAcademicProgramsReplacementsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentAcademicPrograms" },
                    { "action", "CreateStudentAcademicProgramsReplacementsAsync" }
                };
            HttpRoute route = new HttpRoute("student-academic-programs", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentAcademicProgramsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentAcademicProgramsController.ControllerContext;
            var actionDescriptor = studentAcademicProgramsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentAcademicProgramServiceMock.Setup(s => s.CreateStudentAcademicProgramReplacementsAsync(It.IsAny<StudentAcademicProgramReplacements>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentAcademicProgramServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-academic-programs-replacements."));
                var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion POST

        #region POST 

        [TestMethod]
        public async Task CreateStudentAcademicProgramsReplacementsAsync()
        {
            studentAcademicProgramsController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var actual = await studentAcademicProgramsController.CreateStudentAcademicProgramsReplacementsAsync(post_studentAcademicProgramReplacementsDto);
            Assert.IsNotNull(actual);
        }

        #endregion

    }

}