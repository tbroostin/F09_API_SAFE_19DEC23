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
    public class EarningTypesServiceTests
    {
        private const string earningTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string earningTypesCode = "AT";
        private ICollection<EarningType2> _earningTypesCollection;
        private EarningTypesService _earningTypesService;
        
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
           

            _earningTypesCollection = new List<EarningType2>()
                {
                    new EarningType2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EarningType2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EarningType2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetEarningTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_earningTypesCollection);

            _earningTypesService = new EarningTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _earningTypesService = null;
            _earningTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesAsync()
        {
            var results = await _earningTypesService.GetEarningTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<EarningTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesAsync_Count()
        {
            var results = await _earningTypesService.GetEarningTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesAsync_Properties()
        {
            var result =
                (await _earningTypesService.GetEarningTypesAsync(true)).FirstOrDefault(x => x.Code == earningTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesAsync_Expected()
        {
            var expectedResults = _earningTypesCollection.FirstOrDefault(c => c.Guid == earningTypesGuid);
            var actualResult =
                (await _earningTypesService.GetEarningTypesAsync(true)).FirstOrDefault(x => x.Id == earningTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EarningTypesService_GetEarningTypesByGuidAsync_Empty()
        {
            await _earningTypesService.GetEarningTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EarningTypesService_GetEarningTypesByGuidAsync_Null()
        {
            await _earningTypesService.GetEarningTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EarningTypesService_GetEarningTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEarningTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _earningTypesService.GetEarningTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _earningTypesCollection.First(c => c.Guid == earningTypesGuid);
            var actualResult =
                await _earningTypesService.GetEarningTypesByGuidAsync(earningTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EarningTypesService_GetEarningTypesByGuidAsync_Properties()
        {
            var result =
                await _earningTypesService.GetEarningTypesByGuidAsync(earningTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}