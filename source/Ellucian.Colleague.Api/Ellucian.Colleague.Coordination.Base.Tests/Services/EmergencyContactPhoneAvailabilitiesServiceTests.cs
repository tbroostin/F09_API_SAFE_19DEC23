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
    public class EmergencyContactPhoneAvailabilitiesServiceTests
    {
        private const string emergencyContactPhoneAvailabilitiesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string emergencyContactPhoneAvailabilitiesCode = "AT";
        private ICollection<IntgPersonEmerPhoneTypes> _emergencyContactPhoneAvailabilitiesCollection;
        private EmergencyContactPhoneAvailabilitiesService _emergencyContactPhoneAvailabilitiesService;

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


            _emergencyContactPhoneAvailabilitiesCollection = new List<IntgPersonEmerPhoneTypes>()
                {
                    new IntgPersonEmerPhoneTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new IntgPersonEmerPhoneTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new IntgPersonEmerPhoneTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_emergencyContactPhoneAvailabilitiesCollection);

            _emergencyContactPhoneAvailabilitiesService = new EmergencyContactPhoneAvailabilitiesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _emergencyContactPhoneAvailabilitiesService = null;
            _emergencyContactPhoneAvailabilitiesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesAsync()
        {
            var results = await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(true);
            Assert.IsTrue(results is IEnumerable<EmergencyContactPhoneAvailabilities>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesAsync_Count()
        {
            var results = await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesAsync_Properties()
        {
            var result =
                (await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(true)).FirstOrDefault(x => x.Code == emergencyContactPhoneAvailabilitiesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesAsync_Expected()
        {
            var expectedResults = _emergencyContactPhoneAvailabilitiesCollection.FirstOrDefault(c => c.Guid == emergencyContactPhoneAvailabilitiesGuid);
            var actualResult =
                (await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesAsync(true)).FirstOrDefault(x => x.Id == emergencyContactPhoneAvailabilitiesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_Empty()
        {
            await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_Null()
        {
            await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync("99");
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_Expected()
        {
            var expectedResults =
                _emergencyContactPhoneAvailabilitiesCollection.First(c => c.Guid == emergencyContactPhoneAvailabilitiesGuid);
            var actualResult =
                await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(emergencyContactPhoneAvailabilitiesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesService_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_Properties()
        {
            var result =
                await _emergencyContactPhoneAvailabilitiesService.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(emergencyContactPhoneAvailabilitiesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}