//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class ConfigurationSettingsServiceTests
    {
        private const string configurationSettingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string configurationSettingsCode = "1";
        private ICollection<Domain.Base.Entities.ConfigurationSettings> _configurationSettingsCollection;
        private ConfigurationSettingsService _configurationSettingsService;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<IConfigurationSettingsRepository> _configurationSettingsRepoMock;


        [TestInitialize]
        public void Initialize()
        {
            _configurationSettingsRepoMock = new Mock<IConfigurationSettingsRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            _configurationSettingsCollection = new List<Domain.Base.Entities.ConfigurationSettings>()
                {
                    new Domain.Base.Entities.ConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Person Match Criteria")
                    {
                        EthosResources = new List<string>() { "persons" },
                        SourceTitle = "Integration Person Matching",
                        SourceValue = "INTG.PERSON",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.PERSON.DUPL.CRITERIA"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "Address Match Criteria")
                    {
                        EthosResources = new List<string>() { "addresses" },
                        SourceTitle = "Integration Address Matching",
                        SourceValue = "INTG.ADDRESS",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.ADDR.DUPL.CRITERIA"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "Check Faculty Load")
                    {
                        EthosResources = new List<string>() { "section-instructors" },
                        SourceTitle = "Yes",
                        SourceValue = "Y",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.CHECK.FACULTY.LOAD"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-4664bf5b3fbc", "4", "Registration Users ID")
                    {
                        EthosResources = new List<string>() { "section-registrations" },
                        SourceTitle = "WEBREG",
                        SourceValue = "WEBREG",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "REG.USERS",
                        FieldName = "LDMD.REG.USERS.ID"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa8875f0d", "5", "Student Payment Cashier")
                    {
                        EthosResources = new List<string>() { "student-payments" },
                        SourceTitle = "Steven Magnusson",
                        SourceValue = "0003582",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CASHIERS",
                        FieldName = "LDMD.CASHIER"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d2253ac7-9931-2289-b42f-1fccd43c952e", "6", "Elevate Posting Performed in Colleague")
                    {
                        EthosResources = new List<string>() { "student-charges", "student-payments" },
                        SourceTitle = "No",
                        SourceValue = "N",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.CHECK.POSTING.PERFORMED"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d8836ac7-9931-2289-b42f-1fccd43c952e", "7", "Principal Investigator Contact Role")
                    {
                        EthosResources = new List<string>() { "grants" },
                        SourceTitle = "Interviewer",
                        SourceValue = "INTVR",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CORE.VALCODES",
                        FieldName = "LDMD.PRIN.INVESTIGATOR.ROLE",
                        ValcodeTableName = "CONTACT.ROLES"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d9926ac7-8374-2289-b42f-1fccd43c952e", "8", "Mapping Update Rules")
                    {
                        EthosResources = new List<string>() { "mapping-settings" },
                        SourceTitle = "Update ethos value",
                        SourceValue = "ethos",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.MAPPING.CONTROL"
                    }
                };

            _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_configurationSettingsCollection);

            _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_configurationSettingsCollection.FirstOrDefault(cs => cs.Code == configurationSettingsCode));

            _configurationSettingsRepoMock.Setup(repo => repo.GetAllDuplicateCriteriaAsync(It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<string, string>()
                {
                    {
                    "INTG.PERSON",
                    "Person Integration"
                    },
                    {
                        "INTG.ADDRESS",
                        "Address Integration"
                    }
                }
            );

            _configurationSettingsRepoMock.Setup(repo => repo.GetAllRegUsersAsync(It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<string, string>()
                {
                    {
                    "WEBREG",
                    "WEBREG"
                    }
                }
            );

            _configurationSettingsRepoMock.Setup(repo => repo.GetAllCashierNamesAsync(It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<string, string>()
                {
                    {
                    "0003582",
                    "Steven Magnusson"
                    }
                }
            );

            _configurationSettingsRepoMock.Setup(repo => repo.GetAllValcodeItemsAsync("CORE.VALCODES", "CONTACT.ROLES", It.IsAny<bool>()))
                .ReturnsAsync(new Dictionary<string, string>()
                {
                    {
                    "INTVR",
                    "Interviewer"
                    }
                }
            );

            _configurationSettingsService = new ConfigurationSettingsService(_configurationSettingsRepoMock.Object,
                _referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _configurationSettingsService = null;
            _configurationSettingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync()
        {
            var results = await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true);
            Assert.IsTrue(results is IEnumerable<Dtos.ConfigurationSettings>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Count()
        {
            var results = await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true);
            Assert.AreEqual(8, results.Count());
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Properties()
        {
            var result =
                (await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true)).FirstOrDefault(x => x.Id == configurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Description);
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Expected()
        {
            var expectedResults = _configurationSettingsCollection.FirstOrDefault(c => c.Guid == configurationSettingsGuid);
            var actualResult =
                (await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true)).FirstOrDefault(x => x.Id == configurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Empty()
        {
            _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _configurationSettingsService.GetConfigurationSettingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Null()
        {
            _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_InvalidId()
        {
            _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _configurationSettingsService.GetConfigurationSettingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Expected()
        {
            var expectedResults =
                _configurationSettingsCollection.First(c => c.Guid == configurationSettingsGuid);
            var actualResult =
                await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(configurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Properties()
        {
            var result =
                await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(configurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsOptionsAsync_Properties()
        {
            var resultCollection =
                (await _configurationSettingsService.GetConfigurationSettingsOptionsAsync(new List<string>(), true));

            foreach (var result in resultCollection)
            {
                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetConfigurationSettingsOptionsByGuidAsync_Properties()
        {
            foreach (var setting in _configurationSettingsCollection)
            {
                _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_configurationSettingsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

                var result =
                    (await _configurationSettingsService.GetConfigurationSettingsOptionsByGuidAsync(setting.Guid, true));

                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
                Assert.IsNotNull(result.SourceOptions, "Source Options");
            }
        }
    }
}