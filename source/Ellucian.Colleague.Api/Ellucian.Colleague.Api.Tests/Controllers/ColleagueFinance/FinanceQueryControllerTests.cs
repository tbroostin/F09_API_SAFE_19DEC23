//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FinanceQueryControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinanceQueryService> financeQueryServiceMock;
        private FinanceQueryCriteria criteria;
        private IEnumerable<FinanceQuery> financeQueryList;
        private IEnumerable<FinanceQueryActivityDetail> financeQueryDetailList;
        private Mock<ILogger> loggerMock;
        private FinanceQueryController controller;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            var componentCriteria = new List<CostCenterComponentQueryCriteria>();
            var sortComponentCriteria = new List<FinanceQueryComponentSortCriteria>();
            criteria = new FinanceQueryCriteria();
            criteria.FiscalYear = "2018";
            criteria.IncludeActiveAccountsWithNoActivity = false;
            criteria.ComponentCriteria = componentCriteria;
            criteria.ComponentSortCriteria = sortComponentCriteria;

            financeQueryServiceMock = new Mock<IFinanceQueryService>();
            loggerMock = new Mock<ILogger>();


            financeQueryList = new List<FinanceQuery>();
            financeQueryDetailList = new List<FinanceQueryActivityDetail>();
            financeQueryServiceMock.Setup(m => m.QueryFinanceQuerySelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).ReturnsAsync(financeQueryList);
            financeQueryServiceMock.Setup(m => m.QueryFinanceQueryDetailSelectionByPostAsync(It.IsAny<FinanceQueryCriteria>())).ReturnsAsync(financeQueryDetailList);

            controller = new FinanceQueryController(financeQueryServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            financeQueryServiceMock = null;
            loggerMock = null;
        }

        #region Finance Query Summary

        [TestMethod]
        public async Task FinanceQueryController_QueryFinanceQuerySelectionByPostAsync_Success()
        {
            var actual = await controller.QueryFinanceQuerySelectionByPostAsync(criteria);
            Assert.AreEqual(financeQueryList, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQuerySelectionByPostAsync_Exception()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQuerySelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<Exception>();
            await controller.QueryFinanceQuerySelectionByPostAsync(criteria);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQuerySelectionByPostAsync_NullCriteriaException()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQuerySelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<ApplicationException>();
            await controller.QueryFinanceQuerySelectionByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQuerySelectionByPostAsync_ArgumentNullException()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQuerySelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<ArgumentNullException>();
            await controller.QueryFinanceQuerySelectionByPostAsync(criteria);
        }

        #endregion

        #region Finance Query Detail (CSV)

        [TestMethod]
        public async Task FinanceQueryController_QueryFinanceQueryDetailSelectionByPostAsync_Success()
        {
            var actual = await controller.QueryFinanceQueryDetailSelectionByPostAsync(criteria);
            Assert.AreEqual(financeQueryDetailList, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQueryDetailSelectionByPostAsync_Exception()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQueryDetailSelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<Exception>();
            await controller.QueryFinanceQueryDetailSelectionByPostAsync(criteria);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQueryDetailSelectionByPostAsync_NullCriteriaException()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQueryDetailSelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<ApplicationException>();
            await controller.QueryFinanceQueryDetailSelectionByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinanceQueryController_QueryFinanceQueryDetailSelectionByPostAsync_ArgumentNullException()
        {
            financeQueryServiceMock.Setup(m => m.QueryFinanceQueryDetailSelectionByPostAsync(It.IsAny<Dtos.ColleagueFinance.FinanceQueryCriteria>())).Throws<ArgumentNullException>();
            await controller.QueryFinanceQueryDetailSelectionByPostAsync(criteria);
        }

        #endregion
    }
}
