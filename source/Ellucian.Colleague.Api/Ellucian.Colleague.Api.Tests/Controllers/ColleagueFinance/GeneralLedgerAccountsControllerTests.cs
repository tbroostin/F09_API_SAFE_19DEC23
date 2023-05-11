//Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class GeneralLedgerAccountsControllerTests
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IGeneralLedgerAccountService> generalLedgerAccountsServiceMock;

        private HttpResponse response;

        private GeneralLedgerAccountsController generalLedgerAccountsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            generalLedgerAccountsServiceMock = new Mock<IGeneralLedgerAccountService>();


            response = new HttpResponse(new StringWriter());

            generalLedgerAccountsController = new GeneralLedgerAccountsController(generalLedgerAccountsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            generalLedgerAccountsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            generalLedgerAccountsServiceMock = null;
            generalLedgerAccountsController = null;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetGlAccountValidationAsync_ExpiredSessionException()
        {
            try
            {
                generalLedgerAccountsServiceMock.Setup(x => x.ValidateGlAccountAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await generalLedgerAccountsController.GetGlAccountValidationAsync(It.IsAny<string>(), It.IsAny<string>());
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("timeout", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetGlAccountValidationAsync_Exception()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.ValidateGlAccountAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());
            await generalLedgerAccountsController.GetGlAccountValidationAsync(It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetGlAccountValidationAsync_ConfigurationException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.ValidateGlAccountAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
            await generalLedgerAccountsController.GetGlAccountValidationAsync(It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetGlAccountValidationAsync_ArgumentNullException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.ValidateGlAccountAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await generalLedgerAccountsController.GetGlAccountValidationAsync(It.IsAny<string>(), It.IsAny<string>());
        }

        [TestMethod]
        public async Task GeneralLedgerAccountsController_GetUserGeneralLedgerAccountsAsync_Success()
        {
            var expected = new List<GlAccount>();
            generalLedgerAccountsServiceMock.Setup(x => x.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>())).ReturnsAsync(expected);
            loggerMock.SetupGet(x => x.IsInfoEnabled).Returns(true);
            var actual = await generalLedgerAccountsController.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>());
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetUserGeneralLedgerAccountsAsync_Exception()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await generalLedgerAccountsController.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetUserGeneralLedgerAccountsAsync_ArgumentNullException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await generalLedgerAccountsController.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetUserGeneralLedgerAccountsAsync_ConfigurationException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
            await generalLedgerAccountsController.GetUserGeneralLedgerAccountsAsync(It.IsAny<string>());
        }

        [TestMethod]
        public async Task GeneralLedgerAccountsController_GetAsync_Success()
        {
            var expected = new GeneralLedgerAccount();
            generalLedgerAccountsServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var actual = await generalLedgerAccountsController.GetAsync(It.IsAny<string>());
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetAsync_ConfigurationException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
            await generalLedgerAccountsController.GetAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetAsync_ArgumentNullException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await generalLedgerAccountsController.GetAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetAsync_ColleagueSessionExpiredException()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
            await generalLedgerAccountsController.GetAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerAccountsController_GetAsync_Exception()
        {
            generalLedgerAccountsServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await generalLedgerAccountsController.GetAsync(It.IsAny<string>());
        }
    }
}
