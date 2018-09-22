//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayScalesControllerTests_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IPayScalesService> payScalesServiceMock;
        private Mock<ILogger> loggerMock;
        private PayScalesController payScalesController;

        private PayScales payScale;
        private List<PayScales> payScales;

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
            payScalesServiceMock = new Mock<IPayScalesService>();

            InitializeTestData();

            payScalesServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), bypassCache)).ReturnsAsync(new List<string>());
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ReturnsAsync(payScales);
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ReturnsAsync(payScale);

            payScalesController = new PayScalesController(payScalesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            payScalesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            payScalesServiceMock = null;
            payScalesController = null;
        }

        private void InitializeTestData()
        {
            payScale = new PayScales() { Id = guid };
            payScales = new List<PayScales>() { payScale };
        }

        #endregion

        #region GET & GET BY ID

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_KeyNotFoundException()
        {
            bypassCache = true;
            payScalesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new KeyNotFoundException());

            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_PermissionsException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new PermissionsException());
            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_ArgumentException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new ArgumentException());
            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_RepositoryException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new RepositoryException());
            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_IntegrationApiException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new IntegrationApiException());
            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesAsync_Exception()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesAsync(bypassCache)).ThrowsAsync(new Exception());
            await payScalesController.GetPayScalesAsync();
        }

        [TestMethod]
        public async Task PayScalesController_GetPayScalesAsync()
        {
            var result = await payScalesController.GetPayScalesAsync();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_KeyNotFoundException()
        {
            bypassCache = true;
            payScalesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), false)).ThrowsAsync(new KeyNotFoundException());

            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_PermissionsException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new PermissionsException());
            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_ArgumentException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new ArgumentException());
            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_RepositoryException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new RepositoryException());
            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_IntegrationApiException()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new IntegrationApiException());
            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_Exception()
        {
            payScalesServiceMock.Setup(s => s.GetPayScalesByGuidAsync(It.IsAny<string>(), bypassCache)).ThrowsAsync(new Exception());
            await payScalesController.GetPayScalesByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_GetPayScalesByGuidAsync_Argument_Guid_Null()
        {
            await payScalesController.GetPayScalesByGuidAsync(null);
        }

        [TestMethod]
        public async Task PayScalesController_GetPayScalesByGuidAsync()
        {
            var result = await payScalesController.GetPayScalesByGuidAsync(guid);
            Assert.IsNotNull(result);
        }

        #endregion

        #region UNSUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_PostPayScalesAsync_NotSupported()
        {
            await payScalesController.PostPayScalesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_PutPayScalesAsync_NotSupported()
        {
            await payScalesController.PutPayScalesAsync(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PayScalesController_DeletePayScalesAsync_NotSupported()
        {
            await payScalesController.DeletePayScalesAsync(guid);
        }

        #endregion
    }
}