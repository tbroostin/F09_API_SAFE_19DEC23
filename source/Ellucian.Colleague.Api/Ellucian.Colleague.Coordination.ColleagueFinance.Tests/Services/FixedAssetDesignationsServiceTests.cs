//Copyright 2020 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FixedAssetDesignationsServiceTests
    {
        private const string fixedAssetDesignationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string fixedAssetDesignationsCode = "AT";
        private ICollection<FxaTransferFlags> _fixedAssetDesignationsCollection;
        private FixedAssetDesignationsService _fixedAssetDesignationsService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _fixedAssetDesignationsCollection = new List<FxaTransferFlags>()
                {
                    new FxaTransferFlags("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FxaTransferFlags("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FxaTransferFlags("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetFxaTransferFlagsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fixedAssetDesignationsCollection);

            _fixedAssetDesignationsService = new FixedAssetDesignationsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fixedAssetDesignationsService = null;
            _fixedAssetDesignationsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsAsync()
        {
            var results = await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(true);
            Assert.IsTrue(results is IEnumerable<FixedAssetDesignations>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsAsync_Count()
        {
            var results = await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsAsync_Properties()
        {
            var result =
                (await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(true)).FirstOrDefault(x => x.Code == fixedAssetDesignationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsAsync_Expected()
        {
            var expectedResults = _fixedAssetDesignationsCollection.FirstOrDefault(c => c.Guid == fixedAssetDesignationsGuid);
            var actualResult =
                (await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(true)).FirstOrDefault(x => x.Id == fixedAssetDesignationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsByGuidAsync_Empty()
        {
            await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsByGuidAsync_Null()
        {
            await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFxaTransferFlagsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsByGuidAsync_Expected()
        {
            var expectedResults =
                _fixedAssetDesignationsCollection.First(c => c.Guid == fixedAssetDesignationsGuid);
            var actualResult =
                await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync(fixedAssetDesignationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task FixedAssetDesignationsService_GetFixedAssetDesignationsByGuidAsync_Properties()
        {
            var result =
                await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync(fixedAssetDesignationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}