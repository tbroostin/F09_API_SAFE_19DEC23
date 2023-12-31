﻿//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentGradePointAveragesControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IStudentGradePointAveragesService> _studentGradePointAveragesServiceMock;
        private Mock<ILogger> _loggerMock;

        private StudentGradePointAveragesController _studentGradePointAveragesController;

        private List<Dtos.StudentGradePointAverages> _studentGradePointAveragesCollection;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string StudentGradePointAveragesGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _studentGradePointAveragesServiceMock = new Mock<IStudentGradePointAveragesService>();
            _loggerMock = new Mock<ILogger>();

            _studentGradePointAveragesCollection = new List<Dtos.StudentGradePointAverages>();

            var studentGradePointAverages = new Ellucian.Colleague.Dtos.StudentGradePointAverages
            {
                Id = StudentGradePointAveragesGuid,
                Student = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438911"),
                //SectionRegistration = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438922"),
            };

            _studentGradePointAveragesCollection.Add(studentGradePointAverages);

            _studentGradePointAveragesController = new StudentGradePointAveragesController(_studentGradePointAveragesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentGradePointAveragesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentGradePointAveragesController = null;
            _studentGradePointAveragesCollection = null;
            _loggerMock = null;
            _studentGradePointAveragesServiceMock = null;
        }

        #region GET ALL

        [TestMethod]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync()
        {
            _studentGradePointAveragesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentGradePointAveragesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var tuple = new Tuple<IEnumerable<Dtos.StudentGradePointAverages>, int>(_studentGradePointAveragesCollection, 1);

            _studentGradePointAveragesServiceMock
                .Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var studentGradePointAverages = await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(new Paging(10, 0), new QueryStringFilter("criteria", "{'student':{'id':'123'}}"), new QueryStringFilter("gradeDate", "{'gradeDate':{'id':'123'}}"));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await studentGradePointAverages.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentGradePointAverages>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentGradePointAverages>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentGradePointAveragesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                //Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_PermissionsException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_KeyNotFoundException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_ArgumentNullException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_ArgumentException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_RepositoryException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_Exception()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
        }

        //GET v1.0.0
        //Successful
        //GetStudentGradePointAveragesAsync
        [TestMethod]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_Permissions()
        {

            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentGradePointAverages" },
                    { "action", "GetStudentGradePointAveragesAsync" }
                };
            HttpRoute route = new HttpRoute("student-grade-point-averages", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentGradePointAveragesController.Request.SetRouteData(data);
            _studentGradePointAveragesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentGradePointAverages);

            var controllerContext = _studentGradePointAveragesController.ControllerContext;
            var actionDescriptor = _studentGradePointAveragesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.StudentGradePointAverages>, int>(_studentGradePointAveragesCollection, 1);

            _studentGradePointAveragesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var studentGradePointAverages = await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(new Paging(10, 0), new QueryStringFilter("criteria", "{'student':{'id':'123'}}"), new QueryStringFilter("gradeDate", "{'gradeDate':{'id':'123'}}"));

            Object filterObject;
            _studentGradePointAveragesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentGradePointAverages));


        }

        //GET v1.0.0
        //Exception
        //GetStudentGradePointAveragesAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_GetStudentGradePointAveragesAsync_Invalid_Permissions()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentGradePointAverages" },
                    { "action", "GetStudentGradePointAveragesAsync" }
                };
            HttpRoute route = new HttpRoute("student-grade-point-averages", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentGradePointAveragesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _studentGradePointAveragesController.ControllerContext;
            var actionDescriptor = _studentGradePointAveragesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                _studentGradePointAveragesServiceMock.Setup(x => x.GetStudentGradePointAveragesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentGradePointAverages>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                _studentGradePointAveragesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-grade-point-averages."));
                await _studentGradePointAveragesController.GetStudentGradePointAveragesAsync(paging, queryStringFilter, queryStringFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region GETBYID

        [TestMethod]
        public async Task StudentGradePointAveragesController_GetByIdStudentGradePointAverages()
        {
            _studentGradePointAveragesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentGradePointAveragesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = _studentGradePointAveragesCollection.FirstOrDefault();
            _studentGradePointAveragesServiceMock.Setup(rec => rec.GetStudentGradePointAveragesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var result = await _studentGradePointAveragesController.GetStudentGradePointAveragesByGuidAsync(expected.Id);
            Assert.IsNotNull(result);
        }

        //GET by id v1.0.0
        //Successful
        //GetStudentGradePointAveragesByGuidAsync
        [TestMethod]
        public async Task StudentMealPlansController_GetStudentGradePointAveragesByGuidAsync_Permissions()
        {
            var expected = _studentGradePointAveragesCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetStudentGradePointAveragesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentGradePointAveragesController.Request.SetRouteData(data);
            _studentGradePointAveragesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentGradePointAverages);

            var controllerContext = _studentGradePointAveragesController.ControllerContext;
            var actionDescriptor = _studentGradePointAveragesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _studentGradePointAveragesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _studentGradePointAveragesServiceMock.Setup(rec => rec.GetStudentGradePointAveragesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var result = await _studentGradePointAveragesController.GetStudentGradePointAveragesByGuidAsync(expected.Id);

            Object filterObject;
            _studentGradePointAveragesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentGradePointAverages));


        }

        //GET by id v1.0.0
        //Exception
        //GetStudentGradePointAveragesByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonalRelationshipsController_GetStudentGradePointAveragesByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "PersonalRelationships" },
                    { "action", "GetStudentGradePointAveragesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("personal-relationships", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentGradePointAveragesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _studentGradePointAveragesController.ControllerContext;
            var actionDescriptor = _studentGradePointAveragesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _studentGradePointAveragesServiceMock.Setup(i => i.GetStudentGradePointAveragesByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new PermissionsException());
                _studentGradePointAveragesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view personal-relationships."));
                var result = await _studentGradePointAveragesController.GetStudentGradePointAveragesByGuidAsync(It.IsAny<string>());
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
        public async Task StudentGradePointAveragesController_PutStudentGradePointAverages_Exception()
        {
            var expected = _studentGradePointAveragesCollection.FirstOrDefault();
            await _studentGradePointAveragesController.PutStudentGradePointAveragesAsync(expected.Id, expected);
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_PostStudentGradePointAverages_Exception()
        {
            var expected = _studentGradePointAveragesCollection.FirstOrDefault();
            await _studentGradePointAveragesController.PostStudentGradePointAveragesAsync(expected);
        }

        #endregion

        #region DELETE

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentGradePointAveragesController_DeleteStudentGradePointAverages_Exception()
        {
            var expected = _studentGradePointAveragesCollection.FirstOrDefault();
            await _studentGradePointAveragesController.DeleteStudentGradePointAveragesAsync(expected.Id);
        }

        #endregion
    }
}
