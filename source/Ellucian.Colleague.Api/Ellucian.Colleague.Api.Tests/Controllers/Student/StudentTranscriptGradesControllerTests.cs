//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.Web.Http.Routing;
using Ellucian.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Collections;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentTranscriptGradesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentTranscriptGradesService> studentTranscriptGradesServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentTranscriptGradesController studentTranscriptGradesController;
        private IEnumerable<Domain.Student.Entities.StudentTranscriptGrades> allStudentAcademicCredit;
        private List<Dtos.StudentTranscriptGrades> studentTranscriptGradesCollection;
        private List<Dtos.StudentTranscriptGradesAdjustments> studentTranscriptGradesAdjustmentsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentTranscriptGradesServiceMock = new Mock<IStudentTranscriptGradesService>();
            loggerMock = new Mock<ILogger>();
            studentTranscriptGradesCollection = new List<Dtos.StudentTranscriptGrades>();
            studentTranscriptGradesAdjustmentsCollection = new List<Dtos.StudentTranscriptGradesAdjustments>();

            allStudentAcademicCredit = new List<Domain.Student.Entities.StudentTranscriptGrades>()
                {
                    new Domain.Student.Entities.StudentTranscriptGrades("1", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.Student.Entities.StudentTranscriptGrades("2", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.Student.Entities.StudentTranscriptGrades("3", "d2253ac7-9931-4560-b42f-1fccd43c952e")
                };

            foreach (var source in allStudentAcademicCredit)
            {
                var studentTranscriptGrades = new Ellucian.Colleague.Dtos.StudentTranscriptGrades()
                {
                    Id = source.Id
                  
                };
                studentTranscriptGradesCollection.Add(studentTranscriptGrades);
                var studentTranscriptGradesAdjustments = new Ellucian.Colleague.Dtos.StudentTranscriptGradesAdjustments()
                {
                    Id = source.Id,
                    Detail = new StudentTranscriptGradesAdjustmentsDetail()
                    {
                        Grade = new GuidObject2(Guid.NewGuid().ToString())
                    }
                };
                studentTranscriptGradesAdjustmentsCollection.Add(studentTranscriptGradesAdjustments);
            }

            studentTranscriptGradesController = new StudentTranscriptGradesController(studentTranscriptGradesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentTranscriptGradesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentTranscriptGradesController = null;
            allStudentAcademicCredit = null;
            studentTranscriptGradesCollection = null;
            loggerMock = null;
            studentTranscriptGradesServiceMock = null;
        }

        #region student-transcript-grades

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuidAsync_Exception()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_KeyNotFoundException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_PermissionsException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_ArgumentException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_RepositoryException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_IntegrationApiException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_Exception()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_PostStudentTranscriptGradesAsync_Exception()
        {
            await studentTranscriptGradesController.PostStudentTranscriptGradesAsync(studentTranscriptGradesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_PutStudentTranscriptGradesAsync_Exception()
        {
            var sourceContext = studentTranscriptGradesCollection.FirstOrDefault();
            await studentTranscriptGradesController.PutStudentTranscriptGradesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_DeleteStudentTranscriptGradesAsync_Exception()
        {
            await studentTranscriptGradesController.DeleteStudentTranscriptGradesAsync(studentTranscriptGradesCollection.FirstOrDefault().Id);
        }

        //GET by id v1.0.0
        //Successful
        //GetStudentTranscriptGradesByGuidAsync
        [TestMethod]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuidAsync_Permissions()
        {
            var studentTranscriptGrade = studentTranscriptGradesCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGrades" },
                    { "action", "GetStudentTranscriptGradesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);
            studentTranscriptGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments });

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentTranscriptGrade);
            studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);

            Object filterObject;
            studentTranscriptGradesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentTranscriptGrades));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments));


        }

        //GET by id v1.0.0
        //Exception
        //GetStudentTranscriptGradesByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGrades" },
                    { "action", "GetStudentTranscriptGradesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-transcript-grades."));
                await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET v1.0.0
        //Successful
        //GetStudentTranscriptGradesAsync
        [TestMethod]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesAsync_Permissions()
        {
            var studentTranscriptGrade = studentTranscriptGradesCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGrades" },
                    { "action", "GetStudentTranscriptGradesAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);
            studentTranscriptGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments });

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentTranscriptGrade);
            studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);

            Object filterObject;
            studentTranscriptGradesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentTranscriptGrades));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments));


        }

        //GET v1.0.0
        //Exception
        //GetStudentTranscriptGradesAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGrades" },
                    { "action", "GetStudentTranscriptGradesAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentTranscriptGradesServiceMock.Setup(x => x.GetStudentTranscriptGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-transcript-grades."));
                await studentTranscriptGradesController.GetStudentTranscriptGradesByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion

        #region student-transcript-grades-adjustments

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesAdjustmentsByGuid_Exception()
        {
            await studentTranscriptGradesController.GetStudentTranscriptGradesAdjustmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_PostStudentTranscriptGradesAdjustmentsAsync_Exception()
        {
            await studentTranscriptGradesController.PostStudentTranscriptGradesAdjustmentsAsync(studentTranscriptGradesAdjustmentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_PutStudentTranscriptGradesAdjustments_PermissionsException()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.UpdateStudentTranscriptGradesAdjustmentsAsync(studentTranscriptGradesAdjustmentsCollection.FirstOrDefault(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentTranscriptGradesController.PutStudentTranscriptGradesAdjustmentsAsync(expectedGuid, studentTranscriptGradesAdjustmentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesController_PutStudentTranscriptGradesAdjustmentsAsync_Exception()
        {
            studentTranscriptGradesServiceMock.Setup(x => x.UpdateStudentTranscriptGradesAdjustmentsAsync(studentTranscriptGradesAdjustmentsCollection.FirstOrDefault(), It.IsAny<bool>()))
                .Throws<Exception>();
            var sourceContext = studentTranscriptGradesAdjustmentsCollection.FirstOrDefault();
            await studentTranscriptGradesController.PutStudentTranscriptGradesAdjustmentsAsync(sourceContext.Id, sourceContext);
        }

        //PUT v1.0.0
        //Successful
        //PutStudentTranscriptGradesAdjustmentsAsync
        [TestMethod]
        public async Task StudentTranscriptGradesAdjustmentsController_PutStudentTranscriptGradesAdjustmentsAsync_Permissions()
        {
            var sourceContext = studentTranscriptGradesAdjustmentsCollection.FirstOrDefault();

            var studentTranscriptGrade = studentTranscriptGradesCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGradesAdjustments" },
                    { "action", "PutStudentTranscriptGradesAdjustmentsAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades-adjustments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);
            studentTranscriptGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments);

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentTranscriptGradesServiceMock.Setup(x => x.UpdateStudentTranscriptGradesAdjustmentsAsync(It.IsAny<Dtos.StudentTranscriptGradesAdjustments>(), It.IsAny<bool>())).ReturnsAsync(studentTranscriptGrade);
            //var studentGradePointAverages = await studentTranscriptGradesController.PutStudentTranscriptGradesAdjustmentsAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentTranscriptGradesAdjustments>());
            await studentTranscriptGradesController.PutStudentTranscriptGradesAdjustmentsAsync(sourceContext.Id, sourceContext);

            Object filterObject;
            studentTranscriptGradesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments));


        }

        //PUT v1.0.0
        //Exception
        //PutStudentTranscriptGradesAdjustmentsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesAdjustmentsController_PutStudentTranscriptGradesAdjustmentsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentTranscriptGradesAdjustments" },
                    { "action", "PutStudentTranscriptGradesAdjustmentsAsync" }
                };
            HttpRoute route = new HttpRoute("student-transcript-grades-adjustments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentTranscriptGradesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentTranscriptGradesController.ControllerContext;
            var actionDescriptor = studentTranscriptGradesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentTranscriptGradesServiceMock.Setup(x => x.UpdateStudentTranscriptGradesAdjustmentsAsync(studentTranscriptGradesAdjustmentsCollection.FirstOrDefault(), It.IsAny<bool>())).Throws<PermissionsException>();
                studentTranscriptGradesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to update student-transcript-grades-adjustments."));
                await studentTranscriptGradesController.PutStudentTranscriptGradesAdjustmentsAsync(expectedGuid, studentTranscriptGradesAdjustmentsCollection.FirstOrDefault());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}