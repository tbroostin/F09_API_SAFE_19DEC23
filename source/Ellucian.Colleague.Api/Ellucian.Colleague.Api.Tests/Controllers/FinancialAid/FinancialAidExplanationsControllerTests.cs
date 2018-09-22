//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidExplanationsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        public TestFinancialAidReferenceDataRepository dataRepository;
        public FinancialAidExplanationsController actualController;

        public AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.FinancialAidExplanation, Colleague.Dtos.FinancialAid.FinancialAidExplanation> adapter
        {
            get { return new AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidExplanation, Dtos.FinancialAid.FinancialAidExplanation>(adapterRegistryMock.Object, loggerMock.Object); }
        }

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

        [TestInitialize]
        public void TestInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            dataRepository = new TestFinancialAidReferenceDataRepository();            

            referenceDataRepositoryMock.Setup(r => r.GetFinancialAidExplanationsAsync()).Returns(dataRepository.GetFinancialAidExplanationsAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.FinancialAidExplanation, Colleague.Dtos.FinancialAid.FinancialAidExplanation>())
                .Returns(() => new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.FinancialAidExplanation, Colleague.Dtos.FinancialAid.FinancialAidExplanation>(adapterRegistryMock.Object, loggerMock.Object));


            actualController = new FinancialAidExplanationsController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            referenceDataRepositoryMock = null;
            dataRepository = null;
            actualController = null;
        }

        private async Task<List<Dtos.FinancialAid.FinancialAidExplanation>> GetExpectedExplanationDtos()
        {
            var explanationEntities = await dataRepository.GetFinancialAidExplanationsAsync();
            var explanationDtos = new List<Dtos.FinancialAid.FinancialAidExplanation>();
            foreach (var entity in explanationEntities)
            {
                explanationDtos.Add(adapter.MapToType(entity));
            }
            return explanationDtos;
        }

        [TestMethod]
        public async Task GetFinancialAidExplanations_ReturnsNonEmptyExplanationsListTest()
        {
            var explanations = await actualController.GetFinancialAidExplanationsAsync();
            Assert.IsNotNull(explanations);
            Assert.IsTrue(explanations.Any());
        }

        [TestMethod]
        public async Task GetFinancialAidExplanations_ReturnsExpectedNumberOfItemsTest()
        {

            var explanationDtos = await GetExpectedExplanationDtos();
            var actualExplanations = await actualController.GetFinancialAidExplanationsAsync();
            Assert.AreEqual(explanationDtos.Count, actualExplanations.Count());
        }        

        [TestMethod]
        public async Task GetFinancialAidExplanations_ReturnsExpectedListTest()
        {
            var explanationDtos = await GetExpectedExplanationDtos();
            var actualExplanations = (await actualController.GetFinancialAidExplanationsAsync()).ToList();

            for(var i = 0; i < actualExplanations.Count(); i++)
            {
                Assert.AreEqual(explanationDtos[i].ExplanationText, actualExplanations[i].ExplanationText);
                Assert.AreEqual(explanationDtos[i].ExplanationType, actualExplanations[i].ExplanationType);
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetFinancialAidExplanations_RethrowsExceptionTest()
        {
            referenceDataRepositoryMock.Setup(r => r.GetFinancialAidExplanationsAsync()).Throws(new System.Exception());
            actualController = new FinancialAidExplanationsController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
            await actualController.GetFinancialAidExplanationsAsync();
        }
    }
}
