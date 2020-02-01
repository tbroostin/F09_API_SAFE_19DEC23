//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
    public class RelationshipStatusesServiceTests
    {
        private const string relationshipStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string relationshipStatusesCode = "AT";
        private ICollection<RelationshipStatus> _relationshipStatusesCollection;
        private RelationshipStatusesService _relationshipStatusesService;
        
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
           

            _relationshipStatusesCollection = new List<RelationshipStatus>()
                {
                    new RelationshipStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new RelationshipStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new RelationshipStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_relationshipStatusesCollection);

            _relationshipStatusesService = new RelationshipStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _relationshipStatusesService = null;
            _relationshipStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesAsync()
        {
            var results = await _relationshipStatusesService.GetRelationshipStatusesAsync(true);
            Assert.IsTrue(results is IEnumerable<RelationshipStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesAsync_Count()
        {
            var results = await _relationshipStatusesService.GetRelationshipStatusesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesAsync_Properties()
        {
            var result =
                (await _relationshipStatusesService.GetRelationshipStatusesAsync(true)).FirstOrDefault(x => x.Code == relationshipStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesAsync_Expected()
        {
            var expectedResults = _relationshipStatusesCollection.FirstOrDefault(c => c.Guid == relationshipStatusesGuid);
            var actualResult =
                (await _relationshipStatusesService.GetRelationshipStatusesAsync(true)).FirstOrDefault(x => x.Id == relationshipStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RelationshipStatusesService_GetRelationshipStatusesByGuidAsync_Empty()
        {
            await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RelationshipStatusesService_GetRelationshipStatusesByGuidAsync_Null()
        {
            await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RelationshipStatusesService_GetRelationshipStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationshipStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _relationshipStatusesCollection.First(c => c.Guid == relationshipStatusesGuid);
            var actualResult =
                await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync(relationshipStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task RelationshipStatusesService_GetRelationshipStatusesByGuidAsync_Properties()
        {
            var result =
                await _relationshipStatusesService.GetRelationshipStatusesByGuidAsync(relationshipStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}