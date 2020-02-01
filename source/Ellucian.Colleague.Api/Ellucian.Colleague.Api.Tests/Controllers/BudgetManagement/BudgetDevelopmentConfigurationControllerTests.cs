/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Controllers.BudgetManagement;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Coordination.BudgetManagement.Adapters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.BudgetManagement
{
    [TestClass]
    public class BudgetDevelopmentConfigurationControllerTests
    {
        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #endregion

        public Mock<IBudgetDevelopmentConfigurationService> buDevConfigServiceMock;
        public IBudgetDevelopmentConfigurationService buDevConfigService;
        public TestBudgetDevelopmentConfigurationRepository buDevConfigRepository;
        public BudgetDevelopmentConfigurationController actualController;

        public Mock<ILogger> loggerMock;
        public Mock<IAdapterRegistry> adapterRegistry;

        public ITypeAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration> adapter;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistry = new Mock<IAdapterRegistry>();

            loggerMock = new Mock<ILogger>();

            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>())
                .Returns(() => new BudgetConfigurationDtoAdapter(adapterRegistry.Object, loggerMock.Object));
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfigurationComparable, Dtos.BudgetManagement.BudgetConfigurationComparable>())
               .Returns(() => new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetConfigurationComparable, Dtos.BudgetManagement.BudgetConfigurationComparable>(adapterRegistry.Object, loggerMock.Object));

            buDevConfigServiceMock = new Mock<IBudgetDevelopmentConfigurationService>();
            buDevConfigRepository = new TestBudgetDevelopmentConfigurationRepository();

            adapter = adapterRegistry.Object.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>();
            buDevConfigServiceMock.Setup(s => s.GetBudgetDevelopmentConfigurationAsync())
                .ReturnsAsync(adapter.MapToType((buDevConfigRepository.GetBudgetDevelopmentConfigurationAsync()).Result));

            actualController = new BudgetDevelopmentConfigurationController(buDevConfigServiceMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistry = null;
            loggerMock = null;
            buDevConfigServiceMock = null;
            actualController = null;
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentConfigurationAsync_ReturnsExpectedResultTest()
        {
            var expectedDto = adapter.MapToType((buDevConfigRepository.GetBudgetDevelopmentConfigurationAsync()).Result);
            var actualDto = await actualController.GetBudgetDevelopmentConfigurationAsync();
            Assert.AreEqual(expectedDto.BudgetId, actualDto.BudgetId);
            Assert.AreEqual(expectedDto.BudgetTitle, actualDto.BudgetTitle);
            Assert.AreEqual(expectedDto.BudgetYear, actualDto.BudgetYear);
            Assert.AreEqual(expectedDto.BudgetStatus, actualDto.BudgetStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_ConfigurationExceptionTest()
        {
            buDevConfigServiceMock.Setup(s => s.GetBudgetDevelopmentConfigurationAsync())
                .Throws(new ConfigurationException());
            actualController = new BudgetDevelopmentConfigurationController(buDevConfigServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.GetBudgetDevelopmentConfigurationAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetBudgetDevelopmentConfigurationAsync_ExceptionTest()
        {
            buDevConfigServiceMock.Setup(s => s.GetBudgetDevelopmentConfigurationAsync())
                .Throws(new Exception());
            actualController = new BudgetDevelopmentConfigurationController(buDevConfigServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.GetBudgetDevelopmentConfigurationAsync();
        }
    }
}
