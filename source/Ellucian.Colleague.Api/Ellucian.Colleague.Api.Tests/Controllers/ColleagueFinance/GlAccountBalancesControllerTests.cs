// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class GlAccountBalancesControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IGlAccountBalancesService> glAccountBalancesServiceMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;

        private GlAccountBalancesController glAccountBalancesController;
        private Dtos.ColleagueFinance.GlAccountBalancesQueryCriteria criteria;

        private Dtos.ColleagueFinance.GlAccountBalances resultDto;
        private List<Dtos.ColleagueFinance.GlAccountBalances> resultDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            glAccountBalancesServiceMock = new Mock<IGlAccountBalancesService>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();

            BuildData();
            criteria = new Dtos.ColleagueFinance.GlAccountBalancesQueryCriteria()
            {
                GlAccounts = new List<string>() { "11-00-00-00-00000-50000" },
                FiscalYear = "2021"
            };
            glAccountBalancesServiceMock.Setup(s => s.QueryGlAccountBalancesAsync(criteria.GlAccounts, criteria.FiscalYear)).ReturnsAsync(resultDtos);
            glAccountBalancesController = new GlAccountBalancesController(glAccountBalancesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            resultDtos = new List<Dtos.ColleagueFinance.GlAccountBalances>();
            resultDto = new Ellucian.Colleague.Dtos.ColleagueFinance.GlAccountBalances
            {
                GlAccountNumber = "11_00_00_00_00000_50000",
                FormattedGlAccount = "11-00-00-00-00000-50000",
                BudgetAmount = 100m,
                ActualAmount = 10m,
                EncumbranceAmount = 10m,
                RequisitionAmount = 10m,
                GlAccountDescription = "Test Desc",
                ErrorMessage = string.Empty,
                IsPooleeAccount = false,
                UmbrellaGlAccount = string.Empty
            };
            resultDtos.Add(resultDto);
        }

        [TestCleanup]
        public void Cleanup()
        {
            glAccountBalancesController = null;
            _loggerMock = null;
            glAccountBalancesServiceMock = null;
            resultDto = null;
        }

        [TestMethod]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_Success()
        {
            var actualDtos = await glAccountBalancesController.QueryGlAccountBalancesAsync(criteria);
            Assert.IsNotNull(actualDtos);
            Assert.IsTrue(actualDtos.ToList().Count() == 1);
            var actualDto = actualDtos.ToList().First();
            Assert.AreEqual(resultDto.GlAccountNumber, actualDto.GlAccountNumber);
            Assert.AreEqual(resultDto.FormattedGlAccount, actualDto.FormattedGlAccount);
            Assert.AreEqual(resultDto.GlAccountDescription, actualDto.GlAccountDescription);
            Assert.AreEqual(resultDto.BudgetAmount, actualDto.BudgetAmount);
            Assert.AreEqual(resultDto.ActualAmount, actualDto.ActualAmount);
            Assert.AreEqual(resultDto.EncumbranceAmount, actualDto.EncumbranceAmount);
            Assert.AreEqual(resultDto.RequisitionAmount, actualDto.RequisitionAmount);
            Assert.AreEqual(resultDto.IsPooleeAccount, actualDto.IsPooleeAccount);
            Assert.AreEqual(resultDto.UmbrellaGlAccount, actualDto.UmbrellaGlAccount);
            Assert.AreEqual(resultDto.ErrorMessage, actualDto.ErrorMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_NullCriteria()
        {
            var actualDtos = await glAccountBalancesController.QueryGlAccountBalancesAsync(null);
            Assert.IsNull(actualDtos);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_EmptyGlAccountsCriteria()
        {
            criteria = new Dtos.ColleagueFinance.GlAccountBalancesQueryCriteria()
            {
                GlAccounts = new List<string>(),
                FiscalYear = "2021"
            };
            var actualDto = await glAccountBalancesController.QueryGlAccountBalancesAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_EmptyFiscalYearCriteria()
        {
            criteria = new Dtos.ColleagueFinance.GlAccountBalancesQueryCriteria()
            {
                GlAccounts = new List<string>() { "11-00-00-00-00000-50000" },
                FiscalYear = string.Empty
            };
            var actualDto = await glAccountBalancesController.QueryGlAccountBalancesAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_ArgumentNullException()
        {
            glAccountBalancesServiceMock.Setup(s => s.QueryGlAccountBalancesAsync(null, null)).Throws(new ArgumentNullException());
            glAccountBalancesController = new GlAccountBalancesController(glAccountBalancesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await glAccountBalancesController.QueryGlAccountBalancesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_Exception()
        {
            glAccountBalancesServiceMock.Setup(s => s.QueryGlAccountBalancesAsync(It.IsAny<List<string>>(), It.IsAny<string>())).Throws(new Exception());
            glAccountBalancesController = new GlAccountBalancesController(glAccountBalancesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await glAccountBalancesController.QueryGlAccountBalancesAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GlAccountBalancesController_QueryGlAccountBalancesAsync_PermissionsException()
        {
            glAccountBalancesServiceMock.Setup(s => s.QueryGlAccountBalancesAsync(It.IsAny<List<string>>(), It.IsAny<string>())).Throws(new PermissionsException());
            glAccountBalancesController = new GlAccountBalancesController(glAccountBalancesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await glAccountBalancesController.QueryGlAccountBalancesAsync(criteria);
        }
    }
}
