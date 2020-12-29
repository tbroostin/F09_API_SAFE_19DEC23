//Copyright 2020 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CollectionConfigurationSettingsServiceTests
    {
        private const string collectionConfigurationSettingsGuid = "b51d5c02-1010-44de-9fae-671ca7769170";
        private const string collectionConfigurationSettingsCode = "1";
        private IEnumerable<Domain.Base.Entities.CollectionConfigurationSettings> _collectionConfigurationSettingsCollection;
        private CollectionConfigurationSettingsService _collectionConfigurationSettingsService;
        private TestCollectionConfigurationSettingsRepository _testCollectionConfigurationSettingsRepository;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ICollectionConfigurationSettingsRepository> _collectionConfigurationSettingsRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _collectionConfigurationSettingsRepoMock = new Mock<ICollectionConfigurationSettingsRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _testCollectionConfigurationSettingsRepository = new TestCollectionConfigurationSettingsRepository();

            _collectionConfigurationSettingsCollection = _testCollectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsAsync(false);

            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_collectionConfigurationSettingsCollection);

            var collectionConfigurationSettings = _collectionConfigurationSettingsCollection.FirstOrDefault(cs => cs.Code == collectionConfigurationSettingsCode);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(collectionConfigurationSettings);

            var sectionRegistrationStatusCodes = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", false);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("ST.VALCODES", "SECTION.REGISTRATION.STATUSES", It.IsAny<bool>()))
                .ReturnsAsync(sectionRegistrationStatusCodes);

            var officeCodes = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("CORE.VALCODES", "OFFICE.CODES", false);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("CORE.VALCODES", "OFFICE.CODES", It.IsAny<bool>()))
                .ReturnsAsync(officeCodes);

            var hrStatuses = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("HR.VALCODES", "HR.STATUSES", false);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>()))
                .ReturnsAsync(hrStatuses);

            var allRelationTypesCodes = _testCollectionConfigurationSettingsRepository.GetAllRelationTypesCodesAsync(false);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetAllRelationTypesCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allRelationTypesCodes);

            var allBended = _testCollectionConfigurationSettingsRepository.GetAllBendedCodesAsync(false);
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetAllBendedCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allBended);

            _collectionConfigurationSettingsService = new CollectionConfigurationSettingsService(_collectionConfigurationSettingsRepoMock.Object,
                _referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _collectionConfigurationSettingsService = null;
            _collectionConfigurationSettingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsAsync()
        {
            var results = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true);
            Assert.IsTrue(results is IEnumerable<Dtos.CollectionConfigurationSettings>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsAsync_Count()
        {
            var results = await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true);
            Assert.AreEqual(5, results.Count());
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsAsync_Properties()
        {
            var result =
                (await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true)).FirstOrDefault(x => x.Id == collectionConfigurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Description);
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsAsync_Expected()
        {
            var expectedResults = _collectionConfigurationSettingsCollection.FirstOrDefault(c => c.Guid == collectionConfigurationSettingsGuid);
            var actualResult =
                (await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true)).FirstOrDefault(x => x.Id == collectionConfigurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsByGuidAsync_Empty()
        {
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsByGuidAsync_Null()
        {
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsByGuidAsync_InvalidId()
        {
            _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsByGuidAsync_Expected()
        {
            var expectedResults =
                _collectionConfigurationSettingsCollection.First(c => c.Guid == collectionConfigurationSettingsGuid);
            var actualResult =
                await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(collectionConfigurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsByGuidAsync_Properties()
        {
            var result =
                await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsByGuidAsync(collectionConfigurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsOptionsAsync_Properties()
        {
            var resultCollection =
                (await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsAsync(new List<Dtos.DefaultSettingsEthos>(), true));

            foreach (var result in resultCollection)
            {
                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }

        [TestMethod]
        public async Task CollectionConfigurationSettingsService_GetCollectionConfigurationSettingsOptionsByGuidAsync_Properties()
        {
            foreach (var setting in _collectionConfigurationSettingsCollection)
            {
                _collectionConfigurationSettingsRepoMock.Setup(repo => repo.GetCollectionConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_collectionConfigurationSettingsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

                var result =
                    (await _collectionConfigurationSettingsService.GetCollectionConfigurationSettingsOptionsByGuidAsync(setting.Guid, true));

                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }
    }
}