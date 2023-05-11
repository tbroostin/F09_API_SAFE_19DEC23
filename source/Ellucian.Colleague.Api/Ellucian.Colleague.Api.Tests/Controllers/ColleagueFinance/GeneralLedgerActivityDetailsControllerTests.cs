// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Security;
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
    public class GeneralLedgerActivityDetailsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IGeneralLedgerActivityDetailService> generalLedgerActivityDetailServiceMock;
        private Mock<ILogger> loggerMock;
        private GeneralLedgerActivityDetailsController controller;
        private GlAccountActivityDetail glAccountActivityDetailDto;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            generalLedgerActivityDetailServiceMock = new Mock<IGeneralLedgerActivityDetailService>();
            loggerMock = new Mock<ILogger>();

            glAccountActivityDetailDto = new GlAccountActivityDetail();
            generalLedgerActivityDetailServiceMock.Setup(m => m.QueryGlAccountActivityDetailAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(glAccountActivityDetailDto);

            controller = new GeneralLedgerActivityDetailsController(generalLedgerActivityDetailServiceMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            generalLedgerActivityDetailServiceMock = null;
            loggerMock = null;
            glAccountActivityDetailDto = null;
        }

        [TestMethod]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = "2020"
            };

            var actual = await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);

            Assert.AreEqual(glAccountActivityDetailDto, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_NullCriteria()
        {
            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_MissingFiscalYear()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = ""
            };

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_MissingGlAccount()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "",
                FiscalYear = "2020"
            };

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_PermissionsException()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = "2020"
            };
            
            generalLedgerActivityDetailServiceMock.Setup(m => m.QueryGlAccountActivityDetailAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_ConfigurationException()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = "2020"
            };

            generalLedgerActivityDetailServiceMock.Setup(m => m.QueryGlAccountActivityDetailAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ConfigurationException());

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_ColleagueSessionExpiredException()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = "2020"
            };

            generalLedgerActivityDetailServiceMock.Setup(m => m.QueryGlAccountActivityDetailAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ColleagueSessionExpiredException("session expired"));

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerActivityDetailsController_QueryGeneralLedgerActivityDetailsByPostAsync_Exception()
        {
            var criteria = new GlActivityDetailQueryCriteria()
            {
                GlAccount = "1234567890",
                FiscalYear = "2020"
            };

            generalLedgerActivityDetailServiceMock.Setup(m => m.QueryGlAccountActivityDetailAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            await controller.QueryGeneralLedgerActivityDetailsByPostAsync(criteria);
        }
    }
}
