//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
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
using System;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class DefaultSettingsServiceTests
    {
        private const string defaultSettingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string defaultSettingsCode = "1";
        private IEnumerable<Domain.Base.Entities.DefaultSettings> _defaultSettingsCollection;
        private DefaultSettingsService _defaultSettingsService;
        private TestDefaultSettingsRepository _testDefaultSettingsRepository;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<IDefaultSettingsRepository> _defaultSettingsRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _defaultSettingsRepoMock = new Mock<IDefaultSettingsRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _testDefaultSettingsRepository = new TestDefaultSettingsRepository();

            _defaultSettingsCollection = _testDefaultSettingsRepository.GetDefaultSettingsAsync(false);

            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_defaultSettingsCollection);

            var defaultSettings = _defaultSettingsCollection.FirstOrDefault(cs => cs.Code == defaultSettingsCode);
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(defaultSettings);

            var privacyCodes = _testDefaultSettingsRepository.GetAllValcodeItemsAsync("CORE.VALCODES", "PRIVACY.CODES", false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("CORE.VALCODES", "PRIVACY.CODES", It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(privacyCodes);

            var allArCodes = _testDefaultSettingsRepository.GetAllArCodesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllArCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allArCodes);

            var allArTypes = _testDefaultSettingsRepository.GetAllArTypesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllArTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allArTypes);

            var allApprovalAgencyIds = _testDefaultSettingsRepository.GetAllApprovalAgenciesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllApprovalAgenciesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allApprovalAgencyIds);

            var allStaffApprovalIds = _testDefaultSettingsRepository.GetAllStaffApprovalsAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllStaffApprovalsAsync(It.IsAny<bool>()))
                .ReturnsAsync(allStaffApprovalIds);

            var allAcadLevels = _testDefaultSettingsRepository.GetAllAcademicLevelsAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllAcademicLevelsAsync(It.IsAny<bool>()))
                .ReturnsAsync(allAcadLevels);

            var teachingArrangements = _testDefaultSettingsRepository.GetAllValcodeItemsAsync("ST.VALCODES", "TEACHING.ARRANGEMENTS", false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("ST.VALCODES", "TEACHING.ARRANGEMENTS", It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(teachingArrangements);

            var allAssignmentContractTypes = _testDefaultSettingsRepository.GetAllAssignmentContractTypesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllAssignmentContractTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allAssignmentContractTypes);

            var allLoadPeriods = _testDefaultSettingsRepository.GetAllLoadPeriodsAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllLoadPeriodsAsync(It.IsAny<bool>()))
                .ReturnsAsync(allLoadPeriods);

            var allBended = _testDefaultSettingsRepository.GetAllBendedCodesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllBendedCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allBended);

            var courseStatuses = _testDefaultSettingsRepository.GetAllValcodeItemsAsync("ST.VALCODES", "COURSE.STATUSES", false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("ST.VALCODES", "COURSE.STATUSES", It.IsAny<bool>(), It.IsAny<string>()))
                .ReturnsAsync(courseStatuses);

            var allStaff = _testDefaultSettingsRepository.GetAllApplicationStaffAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllApplicationStaffAsync(It.IsAny<bool>()))
                .ReturnsAsync(allStaff);

            var allApplStatuses = _testDefaultSettingsRepository.GetAllApplicationStatusesAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllApplicationStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allApplStatuses);

            var allSponsors = _testDefaultSettingsRepository.GetAllSponsorsAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllSponsorsAsync(It.IsAny<bool>()))
                .ReturnsAsync(allSponsors);

            var allPaymentMethods = _testDefaultSettingsRepository.GetAllPaymentMethodsAsync(false);
            _defaultSettingsRepoMock.Setup(repo => repo.GetAllPaymentMethodsAsync(It.IsAny<bool>()))
                .ReturnsAsync(allPaymentMethods);
            
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsIdFromGuidAsync(It.IsAny<string>()))
                  .ReturnsAsync("1");
            var advancedSearchOptions1 = new Domain.Base.Entities.DefaultSettingsAdvancedSearchOptions("Adj Fac, Sociology", "123", "POSITION");
            var advancedSearchOptions2 = new Domain.Base.Entities.DefaultSettingsAdvancedSearchOptions("Professor, Solciology", "456", "POSITION");
            var advancedSearchOptionsCollection = new List<Domain.Base.Entities.DefaultSettingsAdvancedSearchOptions>();
            advancedSearchOptionsCollection.Add(advancedSearchOptions1);
            advancedSearchOptionsCollection.Add(advancedSearchOptions2);
            var advancedSearch = new Domain.Base.Entities.DefaultSettingsAdvancedSearch("1", advancedSearchOptionsCollection);

            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsAdvancedSearchOptionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(advancedSearch);

            _defaultSettingsService = new DefaultSettingsService(_defaultSettingsRepoMock.Object,
                _referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _defaultSettingsService = null;
            _defaultSettingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsAsync()
        {
            var results = await _defaultSettingsService.GetDefaultSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true);
            Assert.IsTrue(results is IEnumerable<Dtos.DefaultSettings>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsAsync_Count()
        {
            var results = await _defaultSettingsService.GetDefaultSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true);
            Assert.AreEqual(36, results.Count());
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsAsync_Properties()
        {
            var result =
                (await _defaultSettingsService.GetDefaultSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true)).FirstOrDefault(x => x.Id == defaultSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Description);
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsAsync_Expected()
        {
            var expectedResults = _defaultSettingsCollection.FirstOrDefault(c => c.Guid == defaultSettingsGuid);
            var actualResult =
                (await _defaultSettingsService.GetDefaultSettingsAsync(new List<Dtos.DefaultSettingsEthos>(), true)).FirstOrDefault(x => x.Id == defaultSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DefaultSettingsService_GetDefaultSettingsByGuidAsync_Empty()
        {
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _defaultSettingsService.GetDefaultSettingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DefaultSettingsService_GetDefaultSettingsByGuidAsync_Null()
        {
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _defaultSettingsService.GetDefaultSettingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DefaultSettingsService_GetDefaultSettingsByGuidAsync_InvalidId()
        {
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _defaultSettingsService.GetDefaultSettingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsByGuidAsync_Expected()
        {
            var expectedResults =
                _defaultSettingsCollection.First(c => c.Guid == defaultSettingsGuid);
            var actualResult =
                await _defaultSettingsService.GetDefaultSettingsByGuidAsync(defaultSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsByGuidAsync_Properties()
        {
            var result =
                await _defaultSettingsService.GetDefaultSettingsByGuidAsync(defaultSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsOptionsAsync_Properties()
        {
            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsOptionsAsync(new List<Dtos.DefaultSettingsEthos>(), true));

            foreach (var result in resultCollection)
            {
                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsOptionsByGuidAsync_Properties()
        {
            foreach (var setting in _defaultSettingsCollection)
            {
                _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_defaultSettingsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

                var result =
                    (await _defaultSettingsService.GetDefaultSettingsOptionsByGuidAsync(setting.Guid, true));

                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }

        [TestMethod]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_Properties()
        {
            var filter = new Dtos.Filters.DefaultSettingsFilter
            {
                Keyword = "Sociology",
                DefaultSettings = new GuidObject2("1")
            };

            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));
            foreach (var result in resultCollection)
            {
                Assert.IsNotNull(result.Title, "Title");
                Assert.IsNotNull(result.Value, "Value");
                Assert.IsNotNull(result.Origin, "Origin");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_NoKeyword()
        {
            var filter = new Dtos.Filters.DefaultSettingsFilter
            {
                DefaultSettings = new GuidObject2("1")
            };

            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_NoDefaultSetting()
        {
            var filter = new Dtos.Filters.DefaultSettingsFilter
            {
                Keyword = "Sociology"
            };

            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_NoFilter()
        {
            var filter = new Dtos.Filters.DefaultSettingsFilter();

            var resultCollection =                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_NoGuid()
        {
            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsIdFromGuidAsync(It.IsAny<string>()))
                .Throws(new RepositoryException());

            var filter = new Dtos.Filters.DefaultSettingsFilter
            {
                Keyword = "Sociology",
                DefaultSettings = new GuidObject2("1")
            };

            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task DefaultSettingsService_GetDefaultSettingsAdvancedSearchOptionsAsync_CTXException()
        {
            var filter = new Dtos.Filters.DefaultSettingsFilter
            {
                Keyword = "Sociology",
                DefaultSettings = new GuidObject2("1")
            };

            _defaultSettingsRepoMock.Setup(repo => repo.GetDefaultSettingsAdvancedSearchOptionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new RepositoryException());

            var resultCollection =
                (await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(filter, true));

        }
    }
}