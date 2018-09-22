//Copyright 2017 Ellucian Company L.P. and its affiliates.


using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FinancialAidFundClassificationsServiceTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private const string financialAidFundClassificationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string financialAidFundClassificationsCode = "AT";
        private ICollection<FinancialAidFundClassification> _financialAidFundClassificationsCollection;
        private FinancialAidFundClassificationsService _financialAidFundClassificationsService;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _financialAidFundClassificationsCollection = new List<FinancialAidFundClassification>()
                {
                    new FinancialAidFundClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FinancialAidFundClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FinancialAidFundClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundClassificationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_financialAidFundClassificationsCollection);

            _financialAidFundClassificationsService = new FinancialAidFundClassificationsService(_referenceRepositoryMock.Object, baseConfigurationRepository, adapterRegistry, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _financialAidFundClassificationsService = null;
            _financialAidFundClassificationsCollection = null;
            _referenceRepositoryMock = null;
            logger = null;
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsAsync()
        {
            var results = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(true);
            Assert.IsTrue(results is IEnumerable<FinancialAidFundClassifications>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsAsync_Count()
        {
            var results = await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsAsync_Properties()
        {
            var result =
                (await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(true)).FirstOrDefault(x => x.Code == financialAidFundClassificationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsAsync_Expected()
        {
            var expectedResults = _financialAidFundClassificationsCollection.FirstOrDefault(c => c.Guid == financialAidFundClassificationsGuid);
            var actualResult =
                (await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsAsync(true)).FirstOrDefault(x => x.Id == financialAidFundClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsByGuidAsync_Empty()
        {
            await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsByGuidAsync_Null()
        {
            await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundClassificationsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsByGuidAsync_Expected()
        {
            var expectedResults =
                _financialAidFundClassificationsCollection.First(c => c.Guid == financialAidFundClassificationsGuid);
            var actualResult =
                await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync(financialAidFundClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FinancialAidFundClassificationsService_GetFinancialAidFundClassificationsByGuidAsync_Properties()
        {
            var result =
                await _financialAidFundClassificationsService.GetFinancialAidFundClassificationsByGuidAsync(financialAidFundClassificationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}