//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ColleagueFinanceWebConfigurationsControllerTests
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IColleagueFinanceWebConfigurationsService> colleagueFinanceWebConfigurationsServiceMock;
        private ColleagueFinanceWebConfiguration colleagueFinanceWebConfigurations;
        private Mock<ILogger> loggerMock;
        private ColleagueFinanceWebConfigurationsController controller;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            colleagueFinanceWebConfigurationsServiceMock = new Mock<IColleagueFinanceWebConfigurationsService>();
            loggerMock = new Mock<ILogger>();
            colleagueFinanceWebConfigurations = new ColleagueFinanceWebConfiguration();

            colleagueFinanceWebConfigurationsServiceMock.Setup(m => m.GetColleagueFinanceWebConfigurationsAsync()).Returns(Task.FromResult(colleagueFinanceWebConfigurations));


            controller = new ColleagueFinanceWebConfigurationsController(colleagueFinanceWebConfigurationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            colleagueFinanceWebConfigurationsServiceMock = null;
            loggerMock = null;
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ColleagueFinanceWebConfigurationsController_GetColleagueFinanceWebConfigurationsAsync_Exception()
        {
            colleagueFinanceWebConfigurationsServiceMock.Setup(m => m.GetColleagueFinanceWebConfigurationsAsync()).Throws<Exception>();
            await controller.GetColleagueFinanceWebConfigurationsAsync();
        }
    }
}
