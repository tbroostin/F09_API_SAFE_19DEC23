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
    public class LeavePlansControllerTests_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<ILeavePlansService> leavePlansServiceMock;
        private Mock<ILogger> loggerMock;
        private LeavePlansController leavePlansController;

        private LeavePlans leavePlans;
        private Tuple<IEnumerable<LeavePlans>, int> tupleResult;

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
            leavePlansServiceMock = new Mock<ILeavePlansService>();

            InitializeTestData();

            leavePlansServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), bypassCache)).ReturnsAsync(new List<string>());
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ReturnsAsync(leavePlans);
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ReturnsAsync(tupleResult);

            leavePlansController = new LeavePlansController(leavePlansServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            leavePlansController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            leavePlansServiceMock = null;
            leavePlansController = null;
        }

        private void InitializeTestData()
        {
            leavePlans = new LeavePlans()
            {
                Id = guid,
            };

            tupleResult = new Tuple<IEnumerable<LeavePlans>, int>(new List<LeavePlans>() { leavePlans }, 1);
        }

        #endregion

        #region GET & GET BY ID

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_KeyNotFoundException()
        {
            bypassCache = true;
            leavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await leavePlansController.GetLeavePlansAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_PermissionsException()
        {
            leavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await leavePlansController.GetLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_ArgumentException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await leavePlansController.GetLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_RepositoryException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await leavePlansController.GetLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_IntegrationApiException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await leavePlansController.GetLeavePlansAsync(paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansAsync_Exception()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansAsync(It.IsAny<int>(), It.IsAny<int>(), bypassCache)).ThrowsAsync(new Exception());

            await leavePlansController.GetLeavePlansAsync(paging);
        }

        [TestMethod]
        public async Task LeavePlansController_GetLeavePlansAsync()
        {
            var result = await leavePlansController.GetLeavePlansAsync(paging);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsyncc_Guid_As_Null()
        {
            await leavePlansController.GetLeavePlansByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsyncc_KeyNotFoundException()
        {
            leavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsync_PermissionsException()
        {
            bypassCache = false;
            leavePlansController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new PermissionsException());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsyncc_ArgumentException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new ArgumentException());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsync_RepositoryException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new RepositoryException());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsync_IntegrationApiException()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new IntegrationApiException());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_GetLeavePlansByGuidAsync_Exception()
        {
            leavePlansServiceMock.Setup(s => s.GetLeavePlansByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new Exception());

            await leavePlansController.GetLeavePlansByGuidAsync(guid);
        }

        [TestMethod]
        public async Task LeavePlansController_GetLeavePlansByGuidAsync()
        {
            var result = await leavePlansController.GetLeavePlansByGuidAsync(guid);
            Assert.IsNotNull(result);
        }

        #endregion

        #region NOT SUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_PostLeavePlansAsync()
        {
            await leavePlansController.PostLeavePlansAsync(new LeavePlans() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_PutLeavePlansAsync()
        {
            await leavePlansController.PutLeavePlansAsync(guid, new LeavePlans() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LeavePlansController_DeleteLeavePlansAsync()
        {
            await leavePlansController.DeleteLeavePlansAsync(guid);
        }

        #endregion
    }
}