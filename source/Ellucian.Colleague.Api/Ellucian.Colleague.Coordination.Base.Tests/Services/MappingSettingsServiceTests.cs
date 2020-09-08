//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class MappingSettingsServiceTests
    {
        private const string mappingSettingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string mappingSettingsCode = "1";
        private IEnumerable<Domain.Base.Entities.MappingSettings> _mappingSettingsCollection;
        private IEnumerable<Domain.Base.Entities.MappingSettingsOptions> _mappingSettingsOptionsCollection;
        private const string mappingSettingsOptionsGuid = "8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private MappingSettingsService _mappingSettingsService;
        private TestMappingSettingsRepository _testMappingSettingsRepository;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<IMappingSettingsRepository> _mappingSettingsRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _mappingSettingsRepoMock = new Mock<IMappingSettingsRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _testMappingSettingsRepository = new TestMappingSettingsRepository();
            
                       
            _mappingSettingsCollection = _testMappingSettingsRepository.GetMappingSettingsAsync(false);
            var tuple = new Tuple<IEnumerable<Domain.Base.Entities.MappingSettings>, int>(_mappingSettingsCollection, _mappingSettingsCollection.Count());

            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var mappingSettings = _mappingSettingsCollection.FirstOrDefault();
            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(mappingSettings);

            _mappingSettingsOptionsCollection = _testMappingSettingsRepository.GetMappingSettingsOptionsAsync(false);
            var tuple2 = new Tuple<IEnumerable<Domain.Base.Entities.MappingSettingsOptions>, int>(_mappingSettingsOptionsCollection, _mappingSettingsOptionsCollection.Count());

            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple2);

            var mappingSettingsOptions = _mappingSettingsOptionsCollection.FirstOrDefault();
            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(mappingSettingsOptions);

            _mappingSettingsService = new MappingSettingsService(_mappingSettingsRepoMock.Object,
                _referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mappingSettingsService = null;
            _mappingSettingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsAsync()
        {
            var results = await _mappingSettingsService.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.MappingSettings>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsAsync_Count()
        {
            var results = await _mappingSettingsService.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.MappingSettings>(), It.IsAny<bool>());
            Assert.AreEqual(2, results.Item2);
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsAsync_Properties()
        {
            var result = (await _mappingSettingsService.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(),
                new Dtos.MappingSettings(), true)).Item1.FirstOrDefault(x => x.Id == mappingSettingsGuid);

            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsAsync_Expected()
        {
            var expectedResults = _mappingSettingsCollection.FirstOrDefault(c => c.Guid == mappingSettingsGuid);
            var actualResult = (await _mappingSettingsService.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), 
                new Dtos.MappingSettings(), true)).Item1.FirstOrDefault(x => x.Id == mappingSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MappingSettingsService_GetMappingSettingsByGuidAsync_Empty()
        {
            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mappingSettingsService.GetMappingSettingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MappingSettingsService_GetMappingSettingsByGuidAsync_Null()
        {
            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mappingSettingsService.GetMappingSettingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MappingSettingsService_GetMappingSettingsByGuidAsync_InvalidId()
        {
            _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _mappingSettingsService.GetMappingSettingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsByGuidAsync_Expected()
        {
            var expectedResults =
                _mappingSettingsCollection.First(c => c.Guid == mappingSettingsGuid);
            var actualResult =
                await _mappingSettingsService.GetMappingSettingsByGuidAsync(mappingSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsByGuidAsync_Properties()
        {
            var result =
                await _mappingSettingsService.GetMappingSettingsByGuidAsync(mappingSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsOptionsAsync_Properties()
        {
            var results =
                (await _mappingSettingsService.GetMappingSettingsOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), new Dtos.MappingSettingsOptions(), true));
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Item2);

            var actuals = results.Item1.ToList();
            for (int i = 0; i < results.Item1.Count(); i++)
            {
                var actual = actuals[i];
                var expected = _mappingSettingsOptionsCollection.FirstOrDefault(c => c.Guid == actual.Id);
                Assert.AreEqual(expected.Guid, actual.Id, "Guid");
                Assert.AreEqual(expected.Enumerations.Count(), actual.Ethos.Enumerations.Count(), "Enumerations");
            }
        }

        [TestMethod]
        public async Task MappingSettingsService_GetMappingSettingsOptionsByGuidAsync_Properties()
        {
            foreach (var setting in _mappingSettingsOptionsCollection)
            {
                _mappingSettingsRepoMock.Setup(repo => repo.GetMappingSettingsOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_mappingSettingsOptionsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

                var result =
                    (await _mappingSettingsService.GetMappingSettingsOptionsByGuidAsync(setting.Guid, true));

                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos.Resources, "Ethos Resources");
                Assert.IsNotNull(result.Ethos.Enumerations, "Ethos Enumerations");
            }
        }
    }
}