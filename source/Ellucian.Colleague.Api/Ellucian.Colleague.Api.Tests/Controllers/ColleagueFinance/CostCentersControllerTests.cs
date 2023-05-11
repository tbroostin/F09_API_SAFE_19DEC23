// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class CostCentersControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICostCenterService> costCenterServiceMock;
        private Mock<ILogger> loggerMock;
        private CostCentersController controller;
        private IEnumerable<CostCenter> costCenterDtos;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            costCenterServiceMock = new Mock<ICostCenterService>();
            loggerMock = new Mock<ILogger>();

            costCenterDtos = new List<CostCenter>();
            costCenterServiceMock.Setup(m => m.QueryCostCentersAsync(It.IsAny<CostCenterQueryCriteria>())).ReturnsAsync(costCenterDtos);

            controller = new CostCentersController(costCenterServiceMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            costCenterServiceMock = null;
            loggerMock = null;
            costCenterDtos = null;
        }

        [TestMethod]
        public async Task CostCentersController_QueryCostCentersByPostAsync()
        {
            var criteria = new CostCenterQueryCriteria();

            var actual = await controller.QueryCostCentersByPostAsync(criteria);

            Assert.AreEqual(costCenterDtos, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCentersController_QueryCostCentersByPostAsync_NullCriteria()
        {
            await controller.QueryCostCentersByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCentersController_QueryCostCentersByPostAsync_TooManyCriteriaIds()
        {
            var criteria = new CostCenterQueryCriteria()
            {
                Ids = new List<string>()
                {
                    "1",
                    "2"
                }
            };

            await controller.QueryCostCentersByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CostCentersController_QueryCostCentersByPostAsync_ColleagueSessionExpiredException()
        {
            var criteria = new CostCenterQueryCriteria()
            {
                Ids = new List<string>()
                {
                    "test"
                }
            };
            costCenterServiceMock.Setup(m => m.QueryCostCentersAsync(It.IsAny<CostCenterQueryCriteria>())).Throws(new ColleagueSessionExpiredException("session expired"));

            await controller.QueryCostCentersByPostAsync(criteria);
        }
    }
}
