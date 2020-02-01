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
    public class EmergencyContactTypesServiceTests
    {
        private const string emergencyContactTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string emergencyContactTypesCode = "AT";
        private ICollection<IntgPersonEmerTypes> _emergencyContactTypesCollection;
        private EmergencyContactTypesService _emergencyContactTypesService;

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


            _emergencyContactTypesCollection = new List<IntgPersonEmerTypes>()
                {
                    new IntgPersonEmerTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new IntgPersonEmerTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new IntgPersonEmerTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_emergencyContactTypesCollection);

            _emergencyContactTypesService = new EmergencyContactTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _emergencyContactTypesService = null;
            _emergencyContactTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesAsync()
        {
            var results = await _emergencyContactTypesService.GetEmergencyContactTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<EmergencyContactTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesAsync_Count()
        {
            var results = await _emergencyContactTypesService.GetEmergencyContactTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesAsync_Properties()
        {
            var result =
                (await _emergencyContactTypesService.GetEmergencyContactTypesAsync(true)).FirstOrDefault(x => x.Code == emergencyContactTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesAsync_Expected()
        {
            var expectedResults = _emergencyContactTypesCollection.FirstOrDefault(c => c.Guid == emergencyContactTypesGuid);
            var actualResult =
                (await _emergencyContactTypesService.GetEmergencyContactTypesAsync(true)).FirstOrDefault(x => x.Id == emergencyContactTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesByGuidAsync_Empty()
        {
            await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesByGuidAsync_Null()
        {
            await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _emergencyContactTypesCollection.First(c => c.Guid == emergencyContactTypesGuid);
            var actualResult =
                await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync(emergencyContactTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task EmergencyContactTypesService_GetEmergencyContactTypesByGuidAsync_Properties()
        {
            var result =
                await _emergencyContactTypesService.GetEmergencyContactTypesByGuidAsync(emergencyContactTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}