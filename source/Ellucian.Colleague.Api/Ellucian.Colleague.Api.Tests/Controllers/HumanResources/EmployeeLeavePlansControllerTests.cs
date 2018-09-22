//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmployeeEmployeeLeavePlansControllerTests_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IEmployeeLeavePlansService> empLeavePlansServiceMock;
        private Mock<ILogger> loggerMock;
        private EmployeeLeavePlansController empLeavePlansController;

        private EmployeeLeavePlans empLeavePlans;
        private Tuple<IEnumerable<EmployeeLeavePlans>, int> tupleResult;

        private Paging paging = new Paging(10, 0);

        private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";

        private bool bypassCache = false;

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            empLeavePlansServiceMock = new Mock<IEmployeeLeavePlansService>();

            InitializeTestData();

            empLeavePlansServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), bypassCache)).ReturnsAsync(new List<string>());
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ReturnsAsync(empLeavePlans);
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ReturnsAsync(tupleResult);

            empLeavePlansController = new EmployeeLeavePlansController(empLeavePlansServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            empLeavePlansController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            empLeavePlansServiceMock = null;
            empLeavePlansController = null;
        }

        private void InitializeTestData()
        {
            empLeavePlans = new EmployeeLeavePlans()
            {
                Id = guid,
            };

            tupleResult = new Tuple<IEnumerable<EmployeeLeavePlans>, int>(new List<EmployeeLeavePlans>() { empLeavePlans }, 1);
        }

        #endregion

        #region GET & GET BY ID

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_KeyNotFoundException()
        {
            bypassCache = true;
            empLeavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_PermissionsException()
        {
            empLeavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_ArgumentException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_RepositoryException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_IntegrationApiException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync_Exception()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new Exception());

            await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);
        }

        [TestMethod]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansAsync()
        {
            var result = await empLeavePlansController.GetEmployeeLeavePlansAsync(paging);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsyncc_Guid_As_Null()
        {
            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsyncc_KeyNotFoundException()
        {
            empLeavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsync_PermissionsException()
        {
            bypassCache = false;
            empLeavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsyncc_ArgumentException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsync_RepositoryException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsync_IntegrationApiException()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsync_Exception()
        {
            empLeavePlansServiceMock.Setup(s => s.GetEmployeeLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new Exception());

            await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        public async Task EmployeeLeavePlansController_GetEmployeeLeavePlansByGuidAsync()
        {
            var result = await empLeavePlansController.GetEmployeeLeavePlansByGuidAsync(guid);
            Assert.IsNotNull(result);
        }

        #endregion

        #region NOT SUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_PostEmployeeLeavePlansAsync()
        {
            await empLeavePlansController.PostEmployeeLeavePlansAsync(new EmployeeLeavePlans() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_PutEmployeeLeavePlansAsync()
        {
            await empLeavePlansController.PutEmployeeLeavePlansAsync(guid, new EmployeeLeavePlans() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeLeavePlansController_DeleteEmployeeLeavePlansAsync()
        {
            await empLeavePlansController.DeleteEmployeeLeavePlansAsync(guid);
        }

        #endregion
    }

    [TestClass]
    public class EmployeeLeavePlansControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IEmployeeLeavePlansService> employeeLeavePlanServiceMock;

        public EmployeeLeavePlansController controllerUnderTest;

        public void InitializeEmployeeLeavePlansControllerTests()
        {
            loggerMock = new Mock<ILogger>();
            employeeLeavePlanServiceMock = new Mock<IEmployeeLeavePlansService>();
            controllerUnderTest = new EmployeeLeavePlansController(employeeLeavePlanServiceMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetEmployeeLeavePlansV2Tests : EmployeeLeavePlansControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                InitializeEmployeeLeavePlansControllerTests();
                employeeLeavePlanServiceMock.Setup(s => s.GetEmployeeLeavePlansV2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<Dtos.HumanResources.EmployeeLeavePlan>());
                employeeLeavePlanServiceMock.Setup(s => s.GetEmployeeLeavePlansV2Async(null, false)).ReturnsAsync(new List<Dtos.HumanResources.EmployeeLeavePlan>());

            }

            [TestMethod]
            public async Task MethodExecutesNoErrors()
            {
                var result = await controllerUnderTest.GetEmployeeLeavePlansV2Async();
                Assert.IsInstanceOfType(result, typeof(IEnumerable<Dtos.HumanResources.EmployeeLeavePlan>));
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                employeeLeavePlanServiceMock.Setup(s => s.GetEmployeeLeavePlansV2Async(null, false)).ThrowsAsync(new PermissionsException());
                var result = await controllerUnderTest.GetEmployeeLeavePlansV2Async();
            }

            [TestMethod, ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                employeeLeavePlanServiceMock.Setup(s => s.GetEmployeeLeavePlansV2Async(null, false)).ThrowsAsync(new Exception());
                var result = await controllerUnderTest.GetEmployeeLeavePlansV2Async();
            }
        }
    }
}