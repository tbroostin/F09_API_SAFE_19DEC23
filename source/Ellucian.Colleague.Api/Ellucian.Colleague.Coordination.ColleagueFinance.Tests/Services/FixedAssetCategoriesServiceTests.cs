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
    public class FixedAssetCategoriesServiceTests
    {
        private const string fixedAssetCategoriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string fixedAssetCategoriesCode = "AT";
        private ICollection<AssetCategories> _fixedAssetCategoriesCollection;
        private FixedAssetCategoriesService _fixedAssetCategoriesService;
        
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
           

            _fixedAssetCategoriesCollection = new List<AssetCategories>()
                {
                    new AssetCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AssetCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AssetCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAssetCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fixedAssetCategoriesCollection);

            _fixedAssetCategoriesService = new FixedAssetCategoriesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fixedAssetCategoriesService = null;
            _fixedAssetCategoriesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesAsync()
        {
            var results = await _fixedAssetCategoriesService.GetFixedAssetCategoriesAsync(true);
            Assert.IsTrue(results is IEnumerable<FixedAssetCategory>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesAsync_Count()
        {
            var results = await _fixedAssetCategoriesService.GetFixedAssetCategoriesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesAsync_Properties()
        {
            var result =
                (await _fixedAssetCategoriesService.GetFixedAssetCategoriesAsync(true)).FirstOrDefault(x => x.Code == fixedAssetCategoriesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesAsync_Expected()
        {
            var expectedResults = _fixedAssetCategoriesCollection.FirstOrDefault(c => c.Guid == fixedAssetCategoriesGuid);
            var actualResult =
                (await _fixedAssetCategoriesService.GetFixedAssetCategoriesAsync(true)).FirstOrDefault(x => x.Id == fixedAssetCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesByGuidAsync_Empty()
        {
            await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesByGuidAsync_Null()
        {
            await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAssetCategoriesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync("99");
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesByGuidAsync_Expected()
        {
            var expectedResults =
                _fixedAssetCategoriesCollection.First(c => c.Guid == fixedAssetCategoriesGuid);
            var actualResult =
                await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync(fixedAssetCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FixedAssetCategoriesService_GetFixedAssetCategoriesByGuidAsync_Properties()
        {
            var result =
                await _fixedAssetCategoriesService.GetFixedAssetCategoriesByGuidAsync(fixedAssetCategoriesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}