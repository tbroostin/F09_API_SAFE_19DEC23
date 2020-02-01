//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class RelationshipTypesServiceTests
    {
        private const string relationshipTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string relationshipTypesCode = "C";
        private ICollection<Domain.Base.Entities.RelationType> _relationshipTypesCollection;
        private RelationshipTypesService _relationshipTypesService;

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


            _relationshipTypesCollection = new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "C", "Child", "", "P"),
                    new Domain.Base.Entities.RelationType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CO", "Cousin", "", "CO"),
                    new Domain.Base.Entities.RelationType("d2253ac7-9931-4560-b42f-1fccd43c952e", "GC", "Grandchild", "N", "GP"),
                    new Domain.Base.Entities.RelationType("f2253ac7-9931-4560-b42f-1fccd43c952e", "GP", "Grandparent", "N", "GC")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypes2Async(It.IsAny<bool>()))
                .ReturnsAsync(_relationshipTypesCollection);

            _relationshipTypesService = new RelationshipTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _relationshipTypesService = null;
            _relationshipTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesAsync()
        {
            var results = await _relationshipTypesService.GetRelationshipTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<RelationshipTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesAsync_Count()
        {
            var results = await _relationshipTypesService.GetRelationshipTypesAsync(true);
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesAsync_Properties()
        {
            var result =
                (await _relationshipTypesService.GetRelationshipTypesAsync(true)).FirstOrDefault(x => x.Code == relationshipTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesAsync_Expected()
        {
            var expectedResults = _relationshipTypesCollection.FirstOrDefault(c => c.Guid == relationshipTypesGuid);
            var actualResult =
                (await _relationshipTypesService.GetRelationshipTypesAsync(true)).FirstOrDefault(x => x.Id == relationshipTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RelationshipTypesService_GetRelationshipTypesByGuidAsync_Empty()
        {
            await _relationshipTypesService.GetRelationshipTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RelationshipTypesService_GetRelationshipTypesByGuidAsync_Null()
        {
            await _relationshipTypesService.GetRelationshipTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RelationshipTypesService_GetRelationshipTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRelationTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _relationshipTypesService.GetRelationshipTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _relationshipTypesCollection.First(c => c.Guid == relationshipTypesGuid);
            var actualResult =
                await _relationshipTypesService.GetRelationshipTypesByGuidAsync(relationshipTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task RelationshipTypesService_GetRelationshipTypesByGuidAsync_Properties()
        {
            var result =
                await _relationshipTypesService.GetRelationshipTypesByGuidAsync(relationshipTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}