//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class DeductionCategoriesServiceTests
    {
        private const string deductionCategoriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string deductionCategoriesCode = "AT";
        private ICollection<DeductionCategory> _deductionCategoriesCollection;
        private DeductionCategoriesService _deductionCategoriesService;
        
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _deductionCategoriesCollection = new List<DeductionCategory>()
                {
                    new DeductionCategory("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new DeductionCategory("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new DeductionCategory("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetDeductionCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_deductionCategoriesCollection);

            _deductionCategoriesService = new DeductionCategoriesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _deductionCategoriesService = null;
            _deductionCategoriesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesAsync()
        {
            var results = await _deductionCategoriesService.GetDeductionCategoriesAsync(true);
            Assert.IsTrue(results is IEnumerable<DeductionCategories>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesAsync_Count()
        {
            var results = await _deductionCategoriesService.GetDeductionCategoriesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesAsync_Properties()
        {
            var result =
                (await _deductionCategoriesService.GetDeductionCategoriesAsync(true)).FirstOrDefault(x => x.Code == deductionCategoriesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesAsync_Expected()
        {
            var expectedResults = _deductionCategoriesCollection.FirstOrDefault(c => c.Guid == deductionCategoriesGuid);
            var actualResult =
                (await _deductionCategoriesService.GetDeductionCategoriesAsync(true)).FirstOrDefault(x => x.Id == deductionCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task DeductionCategoriesService_GetDeductionCategoriesByGuidAsync_Empty()
        {
            await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task DeductionCategoriesService_GetDeductionCategoriesByGuidAsync_Null()
        {
            await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task DeductionCategoriesService_GetDeductionCategoriesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetDeductionCategoriesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync("99");
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesByGuidAsync_Expected()
        {
            var expectedResults =
                _deductionCategoriesCollection.First(c => c.Guid == deductionCategoriesGuid);
            var actualResult =
                await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync(deductionCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task DeductionCategoriesService_GetDeductionCategoriesByGuidAsync_Properties()
        {
            var result =
                await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync(deductionCategoriesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}