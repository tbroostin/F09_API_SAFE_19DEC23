//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentCourseTransfersControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentCourseTransferService> studentCourseTransfersServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentCourseTransferController studentCourseTransfersController;
        private List<AcademicCredit> allStcs;
        private List<Dtos.StudentCourseTransfer> studentCourseTransfersCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private int offset;
        private int limit;
        private Paging page;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentCourseTransfersServiceMock = new Mock<IStudentCourseTransferService>();
            loggerMock = new Mock<ILogger>();
            studentCourseTransfersCollection = new List<Dtos.StudentCourseTransfer>();
            page = new Paging(100, 0);


            allStcs = new List<AcademicCredit>()
                {
                    new AcademicCredit("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3faa"),
                    new AcademicCredit("849e6a7c-6cd4-4f98-8a73-ab0aa3627fbb"),
                    new AcademicCredit("d2253ac7-9931-4560-b42f-1fccd43c95cc")
                };

            foreach (var source in allStcs)
            {
                var studentCourseTransfers = new Dtos.StudentCourseTransfer()
                { 
                    Id = source.Id
                };
                studentCourseTransfersCollection.Add(studentCourseTransfers);
            }

            studentCourseTransfersController = new StudentCourseTransferController(studentCourseTransfersServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentCourseTransfersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            studentCourseTransfersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

        }

        [TestCleanup]
        public void Cleanup()
        {
            studentCourseTransfersController = null;
            allStcs = null;
            studentCourseTransfersCollection = null;
            loggerMock = null;
            studentCourseTransfersServiceMock = null;
        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_ValidateFields_Cache()
        {
            studentCourseTransfersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), true))
                .ReturnsAsync(new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 3));

            var results = await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
            Assert.IsNotNull(results);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            List<Dtos.StudentCourseTransfer> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCourseTransfer>>)httpResponseMessage.Content).Value as List<Dtos.StudentCourseTransfer>;
            Assert.AreEqual(studentCourseTransfersCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentCourseTransfersCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id);
            }

        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentCourseTransfersController" },
                { "action", "GetStudentCourseTransfers2" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCourseTransfersController.Request.SetRouteData(data);
            studentCourseTransfersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers });

            var controllerContext = studentCourseTransfersController.ControllerContext;
            var actionDescriptor = studentCourseTransfersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 5);
            studentCourseTransfersServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            studentCourseTransfersServiceMock.Setup(s => s.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await studentCourseTransfersController.GetStudentCourseTransfersAsync(new Paging(10, 0));

            Object filterObject;
            studentCourseTransfersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentCourseTransfers));


        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentCourseTransfersController" },
                { "action", "GetStudentCourseTransfers2" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCourseTransfersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentCourseTransfersController.ControllerContext;
            var actionDescriptor = studentCourseTransfersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 5);

                studentCourseTransfersServiceMock.Setup(s => s.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
                studentCourseTransfersServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                     .Throws(new PermissionsException("User is not authorized to view person-holds."));
                var resp = await studentCourseTransfersController.GetStudentCourseTransfersAsync(new Paging(10, 0));
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_KeyNotFoundException()
        {
            //
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_PermissionsException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_ArgumentException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_RepositoryException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_IntegrationApiException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransferByGuidAsync_ValidateFields()
        {
            studentCourseTransfersController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = studentCourseTransfersCollection.FirstOrDefault();
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfersAsync(offset, limit, false)).Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransfersAsync(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransferByGuidAsync_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false)).Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_KeyNotFoundException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<KeyNotFoundException>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_PermissionsException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<PermissionsException>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_ArgumentException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<ArgumentException>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_RepositoryException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<RepositoryException>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_IntegrationApiException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<IntegrationApiException>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfersByGuid_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransferByGuidAsync(It.IsAny<string>(), false))
                .Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransferByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_PostStudentCourseTransferAsync_Exception()
        {
            await studentCourseTransfersController.PostStudentCourseTransferAsync(studentCourseTransfersCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_PutStudentCourseTransfersAsync_Exception()
        {
            var sourceContext = studentCourseTransfersCollection.FirstOrDefault();
            await studentCourseTransfersController.PutStudentCourseTransferAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_DeleteStudentCourseTransfersAsync_Exception()
        {
            await studentCourseTransfersController.DeleteStudentCourseTransferAsync(studentCourseTransfersCollection.FirstOrDefault().Id);
        }
    }

    [TestClass]
    public class StudentCourseTransfersControllerTests_V13
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentCourseTransferService> studentCourseTransfersServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentCourseTransferController studentCourseTransfersController;
        private List<AcademicCredit> allStcs;
        private List<Dtos.StudentCourseTransfer> studentCourseTransfersCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private int offset;
        private int limit;
        private Paging page;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentCourseTransfersServiceMock = new Mock<IStudentCourseTransferService>();
            loggerMock = new Mock<ILogger>();
            studentCourseTransfersCollection = new List<Dtos.StudentCourseTransfer>();
            page = new Paging(100, 0);


            allStcs = new List<AcademicCredit>()
                {
                    new AcademicCredit("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3faa"),
                    new AcademicCredit("849e6a7c-6cd4-4f98-8a73-ab0aa3627fbb"),
                    new AcademicCredit("d2253ac7-9931-4560-b42f-1fccd43c95cc")
                };

            foreach (var source in allStcs)
            {
                var studentCourseTransfers = new Dtos.StudentCourseTransfer()
                {
                    Id = source.Id
                };
                studentCourseTransfersCollection.Add(studentCourseTransfers);
            }

            studentCourseTransfersController = new StudentCourseTransferController(studentCourseTransfersServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentCourseTransfersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            studentCourseTransfersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

        }

        [TestCleanup]
        public void Cleanup()
        {
            studentCourseTransfersController = null;
            allStcs = null;
            studentCourseTransfersCollection = null;
            loggerMock = null;
            studentCourseTransfersServiceMock = null;
        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_ValidateFields_Cache()
        {
            studentCourseTransfersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), true))
                .ReturnsAsync(new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 3));

            var results = await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
            Assert.IsNotNull(results);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            List<Dtos.StudentCourseTransfer> StudentMealPlansAssessments = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentCourseTransfer>>)httpResponseMessage.Content).Value as List<Dtos.StudentCourseTransfer>;
            Assert.AreEqual(studentCourseTransfersCollection.Count, StudentMealPlansAssessments.Count);
            for (var i = 0; i < StudentMealPlansAssessments.Count; i++)
            {
                var expected = studentCourseTransfersCollection[i];
                var actual = StudentMealPlansAssessments[i];
                Assert.AreEqual(expected.Id, actual.Id);
            }

        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentCourseTransfersController" },
                { "action", "GetStudentCourseTransfers2" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCourseTransfersController.Request.SetRouteData(data);
            studentCourseTransfersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers });

            var controllerContext = studentCourseTransfersController.ControllerContext;
            var actionDescriptor = studentCourseTransfersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 5);
            studentCourseTransfersServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                   .Returns(true);
            studentCourseTransfersServiceMock.Setup(s => s.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await studentCourseTransfersController.GetStudentCourseTransfers2Async(new Paging(10, 0));

            Object filterObject;
            studentCourseTransfersController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewStudentCourseTransfers));
            

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
            {
                { "controller", "StudentCourseTransfersController" },
                { "action", "GetStudentCourseTransfers2" }
            };
            HttpRoute route = new HttpRoute("person-holds", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            studentCourseTransfersController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = studentCourseTransfersController.ControllerContext;
            var actionDescriptor = studentCourseTransfersController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.StudentCourseTransfer>, int>(studentCourseTransfersCollection, 5);

               studentCourseTransfersServiceMock.Setup(s => s.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
               studentCourseTransfersServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User is not authorized to view person-holds."));
                var resp = await studentCourseTransfersController.GetStudentCourseTransfers2Async(new Paging(10, 0));
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_KeyNotFoundException()
        {
            //
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_PermissionsException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_ArgumentException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_RepositoryException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_IntegrationApiException()
        {

            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
        }

        [TestMethod]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfer2ByGuidAsync_ValidateFields()
        {
            studentCourseTransfersController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = studentCourseTransfersCollection.FirstOrDefault();
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfers2Async(offset, limit, false)).Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransfers2Async(page);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfer2ByGuidAsync_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false)).Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_KeyNotFoundException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<KeyNotFoundException>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_PermissionsException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<PermissionsException>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_ArgumentException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<ArgumentException>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_RepositoryException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<RepositoryException>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_IntegrationApiException()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<IntegrationApiException>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentCourseTransfersController_GetStudentCourseTransfers2ByGuid_Exception()
        {
            studentCourseTransfersServiceMock.Setup(x => x.GetStudentCourseTransfer2ByGuidAsync(It.IsAny<string>(), false))
                .Throws<Exception>();
            await studentCourseTransfersController.GetStudentCourseTransfer2ByGuidAsync(expectedGuid);
        }

    }
}