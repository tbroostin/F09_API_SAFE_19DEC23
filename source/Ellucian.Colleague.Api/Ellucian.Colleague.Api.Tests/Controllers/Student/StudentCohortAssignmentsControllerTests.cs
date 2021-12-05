//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;
using System.Web.Http.Routing;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student;
using System.Web.Http.Controllers;
using System.Collections;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentCohortAssignmentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentCohortAssignmentsService> studentCohortAssignmentsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentCohortAssignmentsController studentCohortAssignmentsController;      
        private List<Dtos.StudentCohortAssignments> studentCohortAssignmentsCollection;
        Dtos.StudentCohortAssignments studentCohortAssignment;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentCohortAssignmentsServiceMock = new Mock<IStudentCohortAssignmentsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();
            BuildMocks();

            studentCohortAssignmentsController = new StudentCohortAssignmentsController(studentCohortAssignmentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentCohortAssignmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }


        private void BuildData()
        {
            studentCohortAssignmentsCollection = new List<Dtos.StudentCohortAssignments>()
            {
                new Dtos.StudentCohortAssignments()
                {
                    Id = "38942157-824a-4e49-a277-613f24852be6",
                    AcademicLevel = new GuidObject2("4a54be2d-51ae-49d6-9886-5d59729c89a3"),
                    Cohort = new GuidObject2("fda4209a-d47c-495c-8659-c04155a7ad4c"),
                    Person = new GuidObject2("70f84351-3493-42e9-bd8f-d71185bf9953"),
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now.AddDays(30)
                },
                new Dtos.StudentCohortAssignments()
                {
                    Id = "18d2e01f-8fa3-49c5-941d-5b37e03204ef",
                    AcademicLevel = new GuidObject2("4a54be2d-51ae-49d6-9886-5d59729c89a3"),
                    Cohort = new GuidObject2("ec4012ea-4365-4667-8e67-b0866ed324de"),
                    Person = new GuidObject2("70f84351-3493-42e9-bd8f-d71185bf9953"),
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now.AddDays(30)
                },
                new Dtos.StudentCohortAssignments()
                {
                    Id = "becaf275-6d8d-4229-a039-b15de9d22cca",
                    AcademicLevel = new GuidObject2("4a54be2d-51ae-49d6-9886-5d59729c89a3"),
                    Cohort = new GuidObject2("0204b878-dc1a-4866-b913-623bfaedb222"),
                    Person = new GuidObject2("70f84351-3493-42e9-bd8f-d71185bf9953"),
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now.AddDays(30)
                },
                new Dtos.StudentCohortAssignments()
                {
                    Id = "348612b7-3795-4153-892f-11094a83095f",
                    AcademicLevel = new GuidObject2("4a54be2d-51ae-49d6-9886-5d59729c89a3"),
                    Cohort = new GuidObject2("f05a6c0f-3a56-4a87-b931-bc2901da5ef9"),
                    Person = new GuidObject2("70f84351-3493-42e9-bd8f-d71185bf9953"),
                    StartOn = DateTime.Now,
                    EndOn = DateTime.Now.AddDays(30)
                }
            };
            
        }
        private void BuildMocks()
        {
            Tuple<IEnumerable<StudentCohortAssignments>, int> tuple = 
                new Tuple<IEnumerable<StudentCohortAssignments>, int>(studentCohortAssignmentsCollection, studentCohortAssignmentsCollection.Count());
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            studentCohortAssignment = studentCohortAssignmentsCollection.FirstOrDefault();
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(studentCohortAssignment);
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentCohortAssignmentsController = null;
            //allStudentAcadLevels = null;
            studentCohortAssignmentsCollection = null;
            loggerMock = null;
            studentCohortAssignmentsServiceMock = null;
        }

        [TestMethod]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_ValidateFields_Nocache()
        {
            studentCohortAssignmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var sourceContexts = await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCohortAssignments>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentCohortAssignments>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = studentCohortAssignmentsCollection.FirstOrDefault(c => c.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Cohort.Id, actual.Cohort.Id);
                Assert.AreEqual(expected.AcademicLevel.Id, actual.AcademicLevel.Id);
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_KeyNotFoundException()
        {
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_PermissionsException()
        {
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_ArgumentException()
        {
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_RepositoryException()
        {
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_IntegrationApiException()
        {
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuidAsync_ValidateFields()
        {
            studentCohortAssignmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = studentCohortAssignmentsCollection.FirstOrDefault();
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignments_Exception()
        {
            StudentCohortAssignments criteria = new StudentCohortAssignments();

            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
              It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
              .Throws<Exception>();
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuidAsync_Exception()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_KeyNotFoundException()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_PermissionsException()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_ArgumentException()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_RepositoryException()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_IntegrationApiException()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuid_Exception()
        {
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_PostStudentCohortAssignmentsAsync_Exception()
        {
            await studentCohortAssignmentsController.PostStudentCohortAssignmentsAsync(studentCohortAssignmentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_PutStudentCohortAssignmentsAsync_Exception()
        {
            var sourceContext = studentCohortAssignmentsCollection.FirstOrDefault();
            await studentCohortAssignmentsController.PutStudentCohortAssignmentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_DeleteStudentCohortAssignmentsAsync_Exception()
        {
            await studentCohortAssignmentsController.DeleteStudentCohortAssignmentsAsync(studentCohortAssignmentsCollection.FirstOrDefault().Id);
        }

        //GET v1.0.0
        //Successful
        //GetStudentCohortAssignmentsAsync

        [TestMethod]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentCohortAssignments" },
                    { "action", "GetStudentCohortAssignmentsAsync" }
                };
            HttpRoute route = new HttpRoute("student-cohort-assignments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCohortAssignmentsController.Request.SetRouteData(data);
            studentCohortAssignmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentCohortAssignments);

            var controllerContext = studentCohortAssignmentsController.ControllerContext;
            var actionDescriptor = studentCohortAssignmentsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
            var tuple = new Tuple<IEnumerable<Dtos.StudentCohortAssignments>, int>(studentCohortAssignmentsCollection, 5);

            studentCohortAssignmentsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var result = await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());

            Object filterObject;
            studentCohortAssignmentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentCohortAssignments));

        }

        //GET v1.0.0
        //Exception
        //GetStudentCohortAssignmentsAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentCohortAssignments" },
                    { "action", "GetStudentCohortAssignmentsAsync" }
                };
            HttpRoute route = new HttpRoute("student-cohort-assignments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCohortAssignmentsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentCohortAssignmentsController.ControllerContext;
            var actionDescriptor = studentCohortAssignmentsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));



                studentCohortAssignmentsServiceMock.Setup(s => s.GetStudentCohortAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(),
                                It.IsAny<StudentCohortAssignments>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                                .Throws<PermissionsException>();
                studentCohortAssignmentsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-cohort-assignments."));
                await studentCohortAssignmentsController.GetStudentCohortAssignmentsAsync(null, It.IsAny<QueryStringFilter>());
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //GET BY ID v1.0.0
        //Successful
        //GetStudentCohortAssignmentsByGuidAsync

        [TestMethod]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuidAsync_Permissions()
        {
            var expected = studentCohortAssignmentsCollection.FirstOrDefault();
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentCohortAssignments" },
                    { "action", "GetStudentCohortAssignmentsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-cohort-assignments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCohortAssignmentsController.Request.SetRouteData(data);
            studentCohortAssignmentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewStudentCohortAssignments);

            var controllerContext = studentCohortAssignmentsController.ControllerContext;
            var actionDescriptor = studentCohortAssignmentsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            studentCohortAssignmentsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expected.Id);

            Object filterObject;
            studentCohortAssignmentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentCohortAssignments));

        }

        //GET BY ID v1.0.0
        //Exception
        //GetStudentCohortAssignmentsByGuidAsync
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCohortAssignmentsController_GetStudentCohortAssignmentsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "StudentCohortAssignments" },
                    { "action", "GetStudentCohortAssignmentsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("student-cohort-assignments", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCohortAssignmentsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentCohortAssignmentsController.ControllerContext;
            var actionDescriptor = studentCohortAssignmentsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                studentCohortAssignmentsServiceMock.Setup(x => x.GetStudentCohortAssignmentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                studentCohortAssignmentsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view student-cohort-assignments."));
                await studentCohortAssignmentsController.GetStudentCohortAssignmentsByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }
}