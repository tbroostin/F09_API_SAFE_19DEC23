//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public class FixedAssetTypesServiceTests
    {
        private const string fixedAssetTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string fixedAssetTypesCode = "AT";
        private ICollection<AssetTypes> _fixedAssetTypesCollection;
        private FixedAssetTypesService _fixedAssetTypesService;
        
        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _fixedAssetTypesCollection = new List<AssetTypes>()
                {
                    new AssetTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AssetTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AssetTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAssetTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fixedAssetTypesCollection);

            _fixedAssetTypesService = new FixedAssetTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fixedAssetTypesService = null;
            _fixedAssetTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesAsync()
        {
            var results = await _fixedAssetTypesService.GetFixedAssetTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<FixedAssetType>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesAsync_Count()
        {
            var results = await _fixedAssetTypesService.GetFixedAssetTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesAsync_Properties()
        {
            var result =
                (await _fixedAssetTypesService.GetFixedAssetTypesAsync(true)).FirstOrDefault(x => x.Code == fixedAssetTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesAsync_Expected()
        {
            var expectedResults = _fixedAssetTypesCollection.FirstOrDefault(c => c.Guid == fixedAssetTypesGuid);
            var actualResult =
                (await _fixedAssetTypesService.GetFixedAssetTypesAsync(true)).FirstOrDefault(x => x.Id == fixedAssetTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetTypesService_GetFixedAssetTypesByGuidAsync_Empty()
        {
            await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetTypesService_GetFixedAssetTypesByGuidAsync_Null()
        {
            await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetTypesService_GetFixedAssetTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAssetTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _fixedAssetTypesCollection.First(c => c.Guid == fixedAssetTypesGuid);
            var actualResult =
                await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync(fixedAssetTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FixedAssetTypesService_GetFixedAssetTypesByGuidAsync_Properties()
        {
            var result =
                await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync(fixedAssetTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}