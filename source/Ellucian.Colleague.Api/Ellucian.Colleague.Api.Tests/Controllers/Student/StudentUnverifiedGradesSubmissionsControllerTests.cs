//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

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
using Newtonsoft.Json.Linq;
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
    public class StudentUnverifiedGradeSubmissionsControllerTests
    {
        private const string StudentUnverifiedGradesGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

        public TestContext TestContext { get; set; }
        private Mock<IStudentUnverifiedGradesService> _studentUnverifiedGradesServiceMock;
        private Mock<ILogger> _loggerMock;
        private StudentUnverifiedGradesSubmissionsController _studentUnverifiedGradesController;
        private Dtos.StudentUnverifiedGradesSubmissions _studentUnverifiedGradesSubmissions;
        private Dtos.StudentUnverifiedGrades _studentUnverifiedGrades;
        private readonly DateTime _currentDate = DateTime.Now;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _studentUnverifiedGradesServiceMock = new Mock<IStudentUnverifiedGradesService>();
            _loggerMock = new Mock<ILogger>();

            _studentUnverifiedGrades = new Ellucian.Colleague.Dtos.StudentUnverifiedGrades
            {
                Id = StudentUnverifiedGradesGuid,
                Student = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438911"),
                SectionRegistration = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438922"),
            };

            _studentUnverifiedGradesSubmissions = new Dtos.StudentUnverifiedGradesSubmissions()
            {
                Id = StudentUnverifiedGradesGuid,

                SectionRegistration = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438922"),
                Grade = new Dtos.StudentUnverifiedGradesGradeDtoProperty()
                {
                    Grade = new Dtos.GuidObject2("b830e686-7692-4012-8da5-b1b5d4438922"),
                    Type = new Dtos.GuidObject2("c830e686-7692-4012-8da5-b1b5d4438922"),
                    IncompleteGrade = new Dtos.StudentUnverifiedGradesIncompleteGradeDtoProperty()
                    {
                        ExtensionDate = _currentDate,
                        FinalGrade = new Dtos.GuidObject2("d830e686-7692-4012-8da5-b1b5d4438922")
                    }
                },
                LastAttendance = new Dtos.StudentUnverifiedGradesLastAttendanceDtoProperty()
                {
                    Date = _currentDate,
                    Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet
                },
                SubmittedBy = new Dtos.GuidObject2("e830e686-7692-4012-8da5-b1b5d4438922")
            };

            _studentUnverifiedGradesServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            _studentUnverifiedGradesController = new StudentUnverifiedGradesSubmissionsController(_studentUnverifiedGradesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentUnverifiedGradesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _studentUnverifiedGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            _studentUnverifiedGradesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(_studentUnverifiedGrades));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentUnverifiedGradesController = null;
            _studentUnverifiedGrades = null;
            _loggerMock = null;
            _studentUnverifiedGradesServiceMock = null;
        }

        #region GET 
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesSubmissionsController_Get_Exception()
        {
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesSubmissionsAsync(new Paging(1, 1));
        }

       
        #endregion

        #region PUT

        [TestMethod]
        public async Task StudentUnverifiedGradesController_PutStudentUnverifiedGradesSubmissionsAsync()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesSubmissionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_studentUnverifiedGradesSubmissions);

            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                .ReturnsAsync(_studentUnverifiedGrades);

            var studentUnverifiedGrades = await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);

            Assert.IsNotNull(studentUnverifiedGrades);

            Assert.AreEqual(_studentUnverifiedGrades.Id, studentUnverifiedGrades.Id);
            Assert.AreEqual(_studentUnverifiedGrades.Student.Id, studentUnverifiedGrades.Student.Id);
            Assert.AreEqual(_studentUnverifiedGrades.SectionRegistration.Id, studentUnverifiedGrades.SectionRegistration.Id);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_PermissionsException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                      .Throws<PermissionsException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_KeyNotFoundException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                 .Throws<KeyNotFoundException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_ArgumentNullException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                .Throws<ArgumentNullException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_ArgumentException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
               .Throws<ArgumentException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_IntegrationApiException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
             .Throws<IntegrationApiException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_RepositoryException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
             .Throws<RepositoryException>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Put_Exception()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
              .Throws<Exception>();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
        }

        //PUT v1.0.0
        //Successful
        //PutStudentUnverifiedGradesSubmissionsAsync
        [TestMethod]
        public async Task StudentUnverifiedGradesController_PutStudentUnverifiedGradesSubmissionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentUnverifiedGrades" },
                    { "action", "PutStudentUnverifiedGradesSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-unverified-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentUnverifiedGradesController.Request.SetRouteData(data);
            _studentUnverifiedGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions);

            var controllerContext = _studentUnverifiedGradesController.ControllerContext;
            var actionDescriptor = _studentUnverifiedGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _studentUnverifiedGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>())).ReturnsAsync(_studentUnverifiedGrades);
            var studentUnverifiedGrades = await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);

            Object filterObject;
            _studentUnverifiedGradesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));


        }

        //PUT v1.0.0
        //Exception
        //PutStudentUnverifiedGradesSubmissionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_PutStudentUnverifiedGradesSubmissionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentUnverifiedGrades" },
                    { "action", "PutStudentUnverifiedGradesSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-unverified-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentUnverifiedGradesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _studentUnverifiedGradesController.ControllerContext;
            var actionDescriptor = _studentUnverifiedGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _studentUnverifiedGradesServiceMock.Setup(x => x.UpdateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>())).Throws<PermissionsException>();
                _studentUnverifiedGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-unverified-grades."));
                await _studentUnverifiedGradesController.PutStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid, _studentUnverifiedGradesSubmissions);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region POST

        [TestMethod]
        public async Task StudentUnverifiedGradesController_CreateStudentUnverifiedGradesSubmissionsAsync()
        {

            _studentUnverifiedGradesSubmissions.Id = Guid.Empty.ToString();

            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesSubmissionsByGuidAsync(StudentUnverifiedGradesGuid, It.IsAny<bool>())).ReturnsAsync(_studentUnverifiedGradesSubmissions);

            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                .ReturnsAsync(_studentUnverifiedGrades);

            // var response = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissions);

            var studentUnverifiedGrades = await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);

            Assert.IsNotNull(studentUnverifiedGrades);

            Assert.AreEqual(_studentUnverifiedGrades.Id, studentUnverifiedGrades.Id);
            Assert.AreEqual(_studentUnverifiedGrades.Student.Id, studentUnverifiedGrades.Student.Id);
            Assert.AreEqual(_studentUnverifiedGrades.SectionRegistration.Id, studentUnverifiedGrades.SectionRegistration.Id);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_PermissionsException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                      .Throws<PermissionsException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_KeyNotFoundException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                 .Throws<KeyNotFoundException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_ArgumentNullException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
                .Throws<ArgumentNullException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_ArgumentException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
               .Throws<ArgumentException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_IntegrationApiException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
             .Throws<IntegrationApiException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_RepositoryException()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
             .Throws<RepositoryException>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_Post_Exception()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>()))
              .Throws<Exception>();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
        }

        //POST v1.0.0
        //Successful
        //PostStudentUnverifiedGradesSubmissionsAsync
        [TestMethod]
        public async Task StudentUnverifiedGradesController_PostStudentUnverifiedGradesSubmissionsAsync_Permissions()
        {
            _studentUnverifiedGradesSubmissions.Id = Guid.Empty.ToString();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentUnverifiedGrades" },
                    { "action", "PostStudentUnverifiedGradesSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-unverified-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentUnverifiedGradesController.Request.SetRouteData(data);
            _studentUnverifiedGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions);

            var controllerContext = _studentUnverifiedGradesController.ControllerContext;
            var actionDescriptor = _studentUnverifiedGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            _studentUnverifiedGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>())).ReturnsAsync(_studentUnverifiedGrades);
            var studentUnverifiedGrades = await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);

            Object filterObject;
            _studentUnverifiedGradesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions));


        }

        //POST v1.0.0
        //Exception
        //PostStudentUnverifiedGradesSubmissionsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_PostStudentUnverifiedGradesSubmissionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentUnverifiedGrades" },
                    { "action", "PostStudentUnverifiedGradesSubmissionsAsync" }
                };
            HttpRoute route = new HttpRoute("student-unverified-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            _studentUnverifiedGradesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = _studentUnverifiedGradesController.ControllerContext;
            var actionDescriptor = _studentUnverifiedGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                _studentUnverifiedGradesServiceMock.Setup(x => x.CreateStudentUnverifiedGradesSubmissionsAsync(It.IsAny<Dtos.StudentUnverifiedGradesSubmissions>())).Throws<PermissionsException>();
                _studentUnverifiedGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create student-unverified-grades."));
                await _studentUnverifiedGradesController.PostStudentUnverifiedGradesSubmissionsAsync(_studentUnverifiedGradesSubmissions);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


        #endregion

        #region DELETE

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_DeleteStudentUnverifiedGrades_Exception()
        {
            await _studentUnverifiedGradesController.DeleteStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGradesGuid);
        }

        #endregion
    }
}
