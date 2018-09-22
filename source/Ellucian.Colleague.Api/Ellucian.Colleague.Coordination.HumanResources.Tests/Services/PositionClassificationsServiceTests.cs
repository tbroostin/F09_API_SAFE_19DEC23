//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResoures.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PositionClassificationsServiceTests
    {
        private const string positionClassificationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string positionClassificationsCode = "AT";
        private ICollection<Domain.HumanResources.Entities.EmploymentClassification> _positionClassificationsCollection;
        private PositionClassificationsService _positionClassificationsService;

        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _positionClassificationsCollection = new List<Domain.HumanResources.Entities.EmploymentClassification>()
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", EmploymentClassificationType.Position)
                };


            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentClassificationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_positionClassificationsCollection);

            _positionClassificationsService = new PositionClassificationsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _positionClassificationsService = null;
            _positionClassificationsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsAsync()
        {
            var results = await _positionClassificationsService.GetPositionClassificationsAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.PositionClassification>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsAsync_Count()
        {
            var results = await _positionClassificationsService.GetPositionClassificationsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsAsync_Properties()
        {
            var result =
                (await _positionClassificationsService.GetPositionClassificationsAsync(true)).FirstOrDefault(x => x.Code == positionClassificationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsAsync_Expected()
        {
            var expectedResults = _positionClassificationsCollection.FirstOrDefault(c => c.Guid == positionClassificationsGuid);
            var actualResult =
                (await _positionClassificationsService.GetPositionClassificationsAsync(true)).FirstOrDefault(x => x.Id == positionClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PositionClassificationsService_GetPositionClassificationsByGuidAsync_Empty()
        {
            await _positionClassificationsService.GetPositionClassificationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PositionClassificationsService_GetPositionClassificationsByGuidAsync_Null()
        {
            await _positionClassificationsService.GetPositionClassificationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PositionClassificationsService_GetPositionClassificationsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentClassificationsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _positionClassificationsService.GetPositionClassificationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsByGuidAsync_Expected()
        {
            var expectedResults =
                _positionClassificationsCollection.First(c => c.Guid == positionClassificationsGuid);
            var actualResult =
                await _positionClassificationsService.GetPositionClassificationsByGuidAsync(positionClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PositionClassificationsService_GetPositionClassificationsByGuidAsync_Properties()
        {
            var result =
                await _positionClassificationsService.GetPositionClassificationsByGuidAsync(positionClassificationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}