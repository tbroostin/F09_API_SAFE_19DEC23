//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    /// <summary>
    /// Test class for AwardLetterConfigurationController
    /// </summary>
    [TestClass]
    public class AwardLetterConfigurationsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        public TestFinancialAidReferenceDataRepository dataRepository;
        public AwardLetterConfigurationsController actualController;

        public FunctionEqualityComparer<AwardLetterConfiguration> comparer;

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

        public AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.AwardLetterConfiguration, Colleague.Dtos.FinancialAid.AwardLetterConfiguration> adapter
        {
            get { return new AwardLetterConfigurationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object); }
        }

        public Task<IEnumerable<AwardLetterConfiguration>> expectedAwardLetterConfigurations
        {
            get { return Task.FromResult(dataRepository.GetAwardLetterConfigurationsAsync().Result.Select(alc => adapter.MapToType(alc))); }
        }

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            dataRepository = new TestFinancialAidReferenceDataRepository();
            comparer = new FunctionEqualityComparer<AwardLetterConfiguration>((s1, s2) => s1.Id == s2.Id, (s) => s.Id.GetHashCode());

            referenceDataRepositoryMock.Setup(r => r.GetAwardLetterConfigurationsAsync()).Returns(() => dataRepository.GetAwardLetterConfigurationsAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.AwardLetterConfiguration, Colleague.Dtos.FinancialAid.AwardLetterConfiguration>())
                    .Returns(new AwardLetterConfigurationEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object));

            actualController = new AwardLetterConfigurationsController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            referenceDataRepositoryMock = null;
            dataRepository = null;
            comparer = null;
            actualController = null;
        }

        /// <summary>
        /// Tests if the actual returned configurations equal the expected ones (compared by config id)
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ActualConfigurations_EqualExpectedTest()
        {
            var actualConfigurations = await actualController.GetAwardLetterConfigurationsAsync();
            CollectionAssert.AreEqual(expectedAwardLetterConfigurations.Result.ToList(), actualConfigurations.ToList(), comparer);
        }

        /// <summary>
        /// Tests if an empty list of award letter configuration dtos is returned if there were no configurations
        /// returned from the repository
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task NoExpectedConfigurations_ActualConfigurationsEmptyListTest()
        {
            dataRepository.awardLetterParameterData = null;
            
            var actualConfigurations = await actualController.GetAwardLetterConfigurationsAsync();
            Assert.IsNotNull(actualConfigurations);
            Assert.IsFalse(actualConfigurations.Any());
        }

        /// <summary>
        /// Tests if a generic exception gets caught and http response exception rethrown along with logging 
        /// an error message
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException (typeof(HttpResponseException))]
        public async Task GenericExceptionIsCalledAndLoggedTest()
        {
            referenceDataRepositoryMock.Setup(r => r.GetAwardLetterConfigurationsAsync()).Throws(new Exception());

            try
            {
                var actualConfigurations = await actualController.GetAwardLetterConfigurationsAsync();
            }
            catch (HttpResponseException)
            {
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                throw;
            }
        }


    }
}
