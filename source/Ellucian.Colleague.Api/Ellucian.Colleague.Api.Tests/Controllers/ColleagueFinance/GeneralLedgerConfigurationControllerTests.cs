//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class GeneralLedgerConfigurationControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IGeneralLedgerConfigurationService> glConfigurationServiceMock;
        private Mock<ILogger> loggerMock;
        private GeneralLedgerConfigurationController controller;

        GlFiscalYearConfiguration glFiscalYearConfiguration;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            glConfigurationServiceMock = new Mock<IGeneralLedgerConfigurationService>();
            loggerMock = new Mock<ILogger>();


            glFiscalYearConfiguration = new GlFiscalYearConfiguration();
            glConfigurationServiceMock.Setup(m => m.GetGlFiscalYearConfigurationAsync()).ReturnsAsync(glFiscalYearConfiguration);


            controller = new GeneralLedgerConfigurationController(glConfigurationServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            glConfigurationServiceMock = null;
            loggerMock = null;
        }

        #region GetGlFiscalYearConfigurationAsync
        [TestMethod]
        public async Task GeneralLedgerConfigurationController_GetGlFiscalYearConfigurationAsync()
        {
            var glFiscalYearConfig = await controller.GetGlFiscalYearConfigurationAsync();
            Assert.IsNotNull(glFiscalYearConfig);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerConfigurationController_GetGlFiscalYearConfigurationAsync_ConfigurationException()
        {
            glConfigurationServiceMock.Setup(m => m.GetGlFiscalYearConfigurationAsync()).Throws<ConfigurationException>();
            await controller.GetGlFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerConfigurationController_GetGlFiscalYearConfigurationAsync_ArgumentNullException()
        {
            glConfigurationServiceMock.Setup(m => m.GetGlFiscalYearConfigurationAsync()).Throws<ArgumentNullException>();
            await controller.GetGlFiscalYearConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerConfigurationController_GetGlFiscalYearConfigurationAsync_Exception()
        {
            glConfigurationServiceMock.Setup(m => m.GetGlFiscalYearConfigurationAsync()).Throws<Exception>();
            await controller.GetGlFiscalYearConfigurationAsync();
        }

        #endregion  
    }
}

