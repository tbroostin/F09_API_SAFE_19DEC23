//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AlternativeCredentialTypesServiceTests
    {
        private const string alternativeCredentialTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string alternativeCredentialTypesCode = "AT";
        private ICollection<AltIdTypes> _alternativeCredentialTypesCollection;
        private AlternativeCredentialTypesService _alternativeCredentialTypesService;
        
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _alternativeCredentialTypesCollection = new List<AltIdTypes>()
                {
                    new AltIdTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AltIdTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AltIdTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_alternativeCredentialTypesCollection);

            _alternativeCredentialTypesService = new AlternativeCredentialTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _alternativeCredentialTypesService = null;
            _alternativeCredentialTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesAsync()
        {
            var results = await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<AlternativeCredentialTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesAsync_Count()
        {
            var results = await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesAsync_Properties()
        {
            var result =
                (await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(true)).FirstOrDefault(x => x.Code == alternativeCredentialTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesAsync_Expected()
        {
            var expectedResults = _alternativeCredentialTypesCollection.FirstOrDefault(c => c.Guid == alternativeCredentialTypesGuid);
            var actualResult =
                (await _alternativeCredentialTypesService.GetAlternativeCredentialTypesAsync(true)).FirstOrDefault(x => x.Id == alternativeCredentialTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesByGuidAsync_Empty()
        {
            await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesByGuidAsync_Null()
        {
            await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _alternativeCredentialTypesCollection.First(c => c.Guid == alternativeCredentialTypesGuid);
            var actualResult =
                await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync(alternativeCredentialTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesService_GetAlternativeCredentialTypesByGuidAsync_Properties()
        {
            var result =
                await _alternativeCredentialTypesService.GetAlternativeCredentialTypesByGuidAsync(alternativeCredentialTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);            
        }
    }
}