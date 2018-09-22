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
    public class BeneficiaryPreferenceTypesServiceTests
    {
        private const string beneficiaryPreferenceTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string beneficiaryPreferenceTypesCode = "AT";
        private ICollection<BeneficiaryTypes> _beneficiaryPreferenceTypesCollection;
        private BeneficiaryPreferenceTypesService _beneficiaryPreferenceTypesService;
        
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
           

            _beneficiaryPreferenceTypesCollection = new List<BeneficiaryTypes>()
                {
                    new BeneficiaryTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new BeneficiaryTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new BeneficiaryTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetBeneficiaryTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_beneficiaryPreferenceTypesCollection);

            _beneficiaryPreferenceTypesService = new BeneficiaryPreferenceTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _beneficiaryPreferenceTypesService = null;
            _beneficiaryPreferenceTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesAsync()
        {
            var results = await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<BeneficiaryPreferenceTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesAsync_Count()
        {
            var results = await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesAsync_Properties()
        {
            var result =
                (await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(true)).FirstOrDefault(x => x.Code == beneficiaryPreferenceTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesAsync_Expected()
        {
            var expectedResults = _beneficiaryPreferenceTypesCollection.FirstOrDefault(c => c.Guid == beneficiaryPreferenceTypesGuid);
            var actualResult =
                (await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(true)).FirstOrDefault(x => x.Id == beneficiaryPreferenceTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesByGuidAsync_Empty()
        {
            await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesByGuidAsync_Null()
        {
            await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetBeneficiaryTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _beneficiaryPreferenceTypesCollection.First(c => c.Guid == beneficiaryPreferenceTypesGuid);
            var actualResult =
                await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync(beneficiaryPreferenceTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesService_GetBeneficiaryPreferenceTypesByGuidAsync_Properties()
        {
            var result =
                await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync(beneficiaryPreferenceTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}