// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Resource;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Dtos.Base;
using System.Linq;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    /// <summary>
    /// This class tests that the service returns the various configurations.
    /// </summary>
    [TestClass]
    public class ConfigurationServiceTests : GenericUserFactory
    {
        #region Initialize and Cleanup
        private ConfigurationService configurationService = null;
        private ConfigurationService backupConfigurationService = null;
        private ConfigurationService backupConfigurationServiceNoPermission = null;
        private ConfigurationService configurationServiceToReturnNull = null;
        private TestConfigurationRepository testConfigurationRepository = null;
        private ICurrentUserFactory currentUserFactory;
        private ICurrentUserFactory backupConfigCurrentUserFactory;

        // Mock/fake objects to construct ConfigurationService
        private Mock<IConfigurationRepository> configRepoMock;
        private IConfigurationRepository configRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ICurrentUserFactory currentUserFactoryFake;

        private IApiSettingsRepository ApiSettingsRepository;
        private ISettingsRepository XmlSettingsRepository;
        private IResourceRepository ResourceRepository;
        private IReferenceDataRepository ReferenceDataRepository;

        private Domain.Base.Entities.BackupConfiguration FakeBackupConfigurationEntity;
        private Dtos.Base.BackupConfiguration FakeBackupConfigurationDto;
        private ApiSettings FakeApiSettings;
        private Settings FakeSettings;
        private ColleagueSettings FakeColleagueSettings;
        private DmiSettings FakeDmiSettings;
        private OauthSettings FakeOauthSettings;
        private ApiBackupConfigData FakeApiBackupConfigData;

        private Mock<IApiSettingsRepository> ApiSettingsRepositoryMock;
        private Mock<ISettingsRepository> XmlSettingsRepositoryMock;
        private Mock<IResourceRepository> ResourceRepositoryMock;
        private Mock<IReferenceDataRepository> ReferenceDataRepositoryMock;

        private BackupConfigurationQueryCriteria fakeBackupConfigQueryCriteriaWithIds;
        private BackupConfigurationQueryCriteria fakeBackupConfigQueryCriteriaWithNamespace;


        private const string CreateApiBackupConfigPermission = "CREATE.API.BACKUP.CONFIGURATION";
        private const string RestoreApiConfigPermission = "RESTORE.API.CONFIGURATION";
        private const string CreateBackupConfigPermission = "CREATE.BACKUP.CONFIGURATION";
        private const string ViewBackupConfigPermission = "VIEW.BACKUP.CONFIGURATION";

        [TestInitialize]
        public void Initialize()
        {
            // Instantiate mock and fake objects used to construct the service
            configRepoMock = new Mock<IConfigurationRepository>();
            configRepo = configRepoMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            roleRepoMock = new Mock<IRoleRepository>();
            currentUserFactoryFake = new Person001UserFactory();

            // set up backup/restore config permissions for related test methods
            Domain.Entities.Role backupConfigAdminRole = new Domain.Entities.Role(105, "Backupconfig Admin");
            backupConfigAdminRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(CreateApiBackupConfigPermission));
            backupConfigAdminRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(RestoreApiConfigPermission));
            backupConfigAdminRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(CreateBackupConfigPermission));
            backupConfigAdminRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ViewBackupConfigPermission));
            roleRepoMock.SetupGet(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { backupConfigAdminRole });
            roleRepo = roleRepoMock.Object;

            // Mock the adapter registry to use the automappers between the EmergencyInformation domain entity and dto. 
            var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>(); // An empty mock adapter registry to instantiate AutoMapperAdapter


            ApiSettingsRepositoryMock = new Mock<IApiSettingsRepository>();
            XmlSettingsRepositoryMock = new Mock<ISettingsRepository>();
            ResourceRepositoryMock = new Mock<IResourceRepository>();
            ReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            ApiSettingsRepository = ApiSettingsRepositoryMock.Object;
            XmlSettingsRepository = XmlSettingsRepositoryMock.Object;
            ResourceRepository = ResourceRepositoryMock.Object;
            ReferenceDataRepository = ReferenceDataRepositoryMock.Object;


            FakeApiSettings = new ApiSettings(1, "fakeName", 1);
            FakeApiSettings.BulkReadSize = 111;

            FakeDmiSettings = new DmiSettings();
            FakeDmiSettings.AccountName = "test_rt";
            FakeDmiSettings.IpAddress = "1.2.3.4";
            FakeDmiSettings.SharedSecret = "secret";
            FakeOauthSettings = new OauthSettings();
            FakeOauthSettings.OauthIssuerUrl = "https://oauth.prod.10005.elluciancloud.com";
            FakeOauthSettings.OauthProxyLogin = "oauth_proxy";
            FakeOauthSettings.OauthProxyPassword = "password";
            FakeColleagueSettings = new ColleagueSettings();
            FakeColleagueSettings.DmiSettings = FakeDmiSettings;
            FakeSettings = new Settings(FakeColleagueSettings, FakeOauthSettings, Serilog.Events.LogEventLevel.Information);

            FakeApiBackupConfigData = new ApiBackupConfigData();
            FakeApiBackupConfigData.ApiSettings = FakeApiSettings;
            FakeApiBackupConfigData.ResourceFileChangeLogContent = "changelogstuff";
            FakeApiBackupConfigData.WebConfigAppSettingsMaxQueryAttributeLimit = "222";

            FakeBackupConfigurationEntity = new Domain.Base.Entities.BackupConfiguration();
            FakeBackupConfigurationEntity.Id = "56c1fb34-9e3e-49a0-b2b0-60751a877855";
            FakeBackupConfigurationEntity.Namespace = "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt";
            FakeBackupConfigurationEntity.ProductId = "Colleague Web API";
            FakeBackupConfigurationEntity.ProductVersion = "1.18.0.0";
            FakeBackupConfigurationEntity.CreatedDateTime = new DateTimeOffset(2017, 1, 1, 1, 1, 1, new TimeSpan(0));
            FakeBackupConfigurationEntity.ConfigVersion = "1";
            FakeBackupConfigurationEntity.ConfigData = "testdata";


            FakeBackupConfigurationDto = new Dtos.Base.BackupConfiguration();
            FakeBackupConfigurationDto.Id = "56c1fb34-9e3e-49a0-b2b0-60751a877855";
            FakeBackupConfigurationDto.Namespace = "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt";
            FakeBackupConfigurationDto.ProductId = "Colleague Web API";
            FakeBackupConfigurationDto.ProductVersion = "1.18.0.0";
            FakeBackupConfigurationDto.CreatedDateTime = new DateTimeOffset(2017, 1, 1, 1, 1, 1, new TimeSpan(0));
            FakeBackupConfigurationDto.ConfigVersion = "1";
            FakeBackupConfigurationDto.ConfigData = "testdata";

            fakeBackupConfigQueryCriteriaWithIds = new BackupConfigurationQueryCriteria()
            {
                ConfigurationIds = new List<string>() { FakeBackupConfigurationDto.Id }
            };
            fakeBackupConfigQueryCriteriaWithNamespace = new BackupConfigurationQueryCriteria()
            {
                Namespace = FakeBackupConfigurationDto.Namespace
            };

            testConfigurationRepository = new TestConfigurationRepository();
            // do all the arranges/mocks here so they don't have to be repeated in the test methods.

            XmlSettingsRepositoryMock.Setup(
                repo => repo.Get())
                .Returns(FakeSettings);

            configRepoMock.Setup(
                repo => repo.AddBackupConfigurationAsync(It.IsAny<Domain.Base.Entities.BackupConfiguration>()))
                .ReturnsAsync(FakeBackupConfigurationEntity);

            configRepoMock.Setup(
                repo => repo.GetBackupConfigurationByIdsAsync(new List<string>() { FakeBackupConfigurationEntity.Id }))
                .ReturnsAsync(new List<Domain.Base.Entities.BackupConfiguration>() { FakeBackupConfigurationEntity });

            // Instantiate the service
            configurationService = new ConfigurationService(configRepoMock.Object, adapterRegistry, currentUserFactoryFake, roleRepo,
                FakeApiSettings, XmlSettingsRepository, ApiSettingsRepository, ResourceRepository, ReferenceDataRepository, logger);

            BuildConfigurationService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset the services and repository variables.
            configRepo = null;
            adapterRegistry = null;
            logger = null;
            roleRepo = null;
            currentUserFactoryFake = null;
            configurationService = null;
            testConfigurationRepository = null;
            currentUserFactory = null;
            backupConfigCurrentUserFactory = null;
        }
        #endregion

        [TestMethod]
        public async Task GetUserProfileConfigurationAsync_Success()
        {
            var upcDto = await this.configurationService.GetUserProfileConfigurationAsync();
            var upcEntity = await testConfigurationRepository.GetUserProfileConfigurationAsync();

            Assert.AreEqual(upcEntity.AddressesAreUpdatable, upcDto.AddressesAreUpdatable);
            Assert.AreEqual(upcEntity.AllAddressTypesAreViewable, upcDto.AllAddressTypesAreViewable);
            Assert.AreEqual(upcEntity.AllEmailTypesAreUpdatable, upcDto.AllEmailTypesAreUpdatable);
            Assert.AreEqual(upcEntity.AllEmailTypesAreViewable, upcDto.AllEmailTypesAreViewable);
            Assert.AreEqual(upcEntity.AllPhoneTypesAreViewable, upcDto.AllPhoneTypesAreViewable);
            Assert.AreEqual(upcEntity.CanUpdateAddressWithoutPermission, upcDto.CanUpdateAddressWithoutPermission);
            Assert.AreEqual(upcEntity.CanUpdateEmailWithoutPermission, upcDto.CanUpdateEmailWithoutPermission);
            Assert.AreEqual(upcEntity.CanUpdatePhoneWithoutPermission, upcDto.CanUpdatePhoneWithoutPermission);
            Assert.AreEqual(upcEntity.Text, upcDto.Text);
            CollectionAssert.AreEqual(upcEntity.UpdatableAddressTypes, upcDto.UpdatableAddressTypes);
            CollectionAssert.AreEqual(upcEntity.UpdatableEmailTypes, upcDto.UpdatableEmailTypes);
            CollectionAssert.AreEqual(upcEntity.UpdatablePhoneTypes, upcDto.UpdatablePhoneTypes);
            CollectionAssert.AreEqual(upcEntity.ViewableAddressTypes, upcDto.ViewableAddressTypes);
            CollectionAssert.AreEqual(upcEntity.ViewableEmailTypes, upcDto.ViewableEmailTypes);
            CollectionAssert.AreEqual(upcEntity.ViewablePhoneTypes, upcDto.ViewablePhoneTypes);
        }

        [TestMethod]
        public async Task GetUserProfileConfiguration2Async_Success()
        {
            var upcDto = await this.configurationService.GetUserProfileConfiguration2Async();
            var upcEntity = await testConfigurationRepository.GetUserProfileConfiguration2Async();

            Assert.AreEqual(upcEntity.AllAddressTypesAreViewable, upcDto.AllAddressTypesAreViewable);
            Assert.AreEqual(upcEntity.AllEmailTypesAreUpdatable, upcDto.AllEmailTypesAreUpdatable);
            Assert.AreEqual(upcEntity.AllEmailTypesAreViewable, upcDto.AllEmailTypesAreViewable);
            Assert.AreEqual(upcEntity.AllPhoneTypesAreViewable, upcDto.AllPhoneTypesAreViewable);
            Assert.AreEqual(upcEntity.CanViewPhonesWithoutTypes, upcDto.CanViewPhonesWithoutTypes);
            Assert.AreEqual(upcEntity.CanUpdateAddressWithoutPermission, upcDto.CanUpdateAddressWithoutPermission);
            Assert.AreEqual(upcEntity.CanUpdateEmailWithoutPermission, upcDto.CanUpdateEmailWithoutPermission);
            Assert.AreEqual(upcEntity.CanUpdatePhoneWithoutPermission, upcDto.CanUpdatePhoneWithoutPermission);
            Assert.AreEqual(upcEntity.Text, upcDto.Text);
            Assert.AreEqual(upcEntity.AuthorizePhonesForText, upcDto.AuthorizePhonesForText);
            CollectionAssert.AreEqual(upcEntity.UpdatableAddressTypes, upcDto.UpdatableAddressTypes);
            CollectionAssert.AreEqual(upcEntity.UpdatableEmailTypes, upcDto.UpdatableEmailTypes);
            CollectionAssert.AreEqual(upcEntity.UpdatablePhoneTypes, upcDto.UpdatablePhoneTypes);
            CollectionAssert.AreEqual(upcEntity.ViewableAddressTypes, upcDto.ViewableAddressTypes);
            CollectionAssert.AreEqual(upcEntity.ViewableEmailTypes, upcDto.ViewableEmailTypes);
            CollectionAssert.AreEqual(upcEntity.ViewablePhoneTypes, upcDto.ViewablePhoneTypes);
        }

        [TestMethod]
        public async Task GetEmergencyInformationConfigurationAsync()
        {
            var eicDto = await this.configurationService.GetEmergencyInformationConfigurationAsync();
            var eicEntity = await testConfigurationRepository.GetEmergencyInformationConfigurationAsync();

            Assert.AreEqual(eicEntity.AllowOptOut, eicDto.AllowOptOut);
            Assert.AreEqual(eicEntity.HideHealthConditions, eicDto.HideHealthConditions);
            Assert.AreEqual(eicEntity.RequireContact, eicDto.RequireContactToConfirm);
        }

        [TestMethod]
        public async Task GetEmergencyInformationConfiguration2Async()
        {
            var eicDto = await this.configurationService.GetEmergencyInformationConfiguration2Async();
            var eicEntity = await testConfigurationRepository.GetEmergencyInformationConfigurationAsync();

            Assert.AreEqual(eicEntity.AllowOptOut, eicDto.AllowOptOut);
            Assert.AreEqual(eicEntity.HideHealthConditions, eicDto.HideHealthConditions);
            Assert.AreEqual(eicEntity.HideOtherInformation, eicDto.HideOtherInformation);
            Assert.AreEqual(eicEntity.RequireContact, eicDto.RequireContact);
        }

        [TestMethod]
        public async Task GetRestrictionConfigurationAsync()
        {
            var rcDto = await this.configurationService.GetRestrictionConfigurationAsync();
            var rcEntity = await testConfigurationRepository.GetRestrictionConfigurationAsync();

            Assert.AreEqual(rcEntity.Mapping.Count, rcDto.Mapping.Count);
            Assert.AreEqual(rcEntity.Mapping[0].SeverityStart, rcDto.Mapping[0].SeverityStart);
            Assert.AreEqual(rcEntity.Mapping[0].SeverityEnd, rcDto.Mapping[0].SeverityEnd);
            Assert.AreEqual(rcEntity.Mapping[0].Style.ToString(), rcDto.Mapping[0].Style.ToString());
        }

        [TestMethod]
        public async Task ConfigurationService_GetPrivacyConfigurationAsync()
        {
            var pcDto = await this.configurationService.GetPrivacyConfigurationAsync();
            var pcEntity = await testConfigurationRepository.GetPrivacyConfigurationAsync();

            Assert.AreEqual(pcEntity.RecordDenialMessage, pcDto.RecordDenialMessage);
        }

        [TestMethod]
        public async Task ConfigurationService_GetOrganizationalRelationshipConfigurationAsync_Success()
        {
            var expectedConfigCount = 1;
            var expectedConfigCode = "MGR";
            var configuration = await configurationService.GetOrganizationalRelationshipConfigurationAsync();
            Assert.AreEqual(expectedConfigCount, configuration.RelationshipTypeCodeMapping.Keys.Count);
            Assert.AreEqual(expectedConfigCode, configuration.RelationshipTypeCodeMapping[Dtos.Base.OrganizationalRelationshipType.Manager][0]);

        }

        [TestMethod]
        public async Task ConfigurationService_GetSelfServiceConfigurationAsync()
        {
            var dto = await this.configurationService.GetSelfServiceConfigurationAsync();
            var expected = await testConfigurationRepository.GetSelfServiceConfigurationAsync();

            Assert.AreEqual(expected.AlwaysUseClipboardForBulkMailToLinks, dto.AlwaysUseClipboardForBulkMailToLinks);
        }

        [TestMethod]
        public async Task GetRequiredDocumentConfigurationAsync_Success()
        {
            var rdcDto = await this.configurationService.GetRequiredDocumentConfigurationAsync();
            var rdcEntity = await testConfigurationRepository.GetRequiredDocumentConfigurationAsync();

            Assert.AreEqual(rdcEntity.SuppressInstance, rdcDto.SuppressInstance);
            Assert.ReferenceEquals(rdcEntity.PrimarySortField, rdcDto.PrimarySortField);
            Assert.ReferenceEquals(rdcEntity.SecondarySortField, rdcDto.SecondarySortField);
            Assert.AreEqual(rdcEntity.TextForBlankStatus, rdcDto.TextForBlankStatus);
            Assert.AreEqual(rdcEntity.TextForBlankDueDate, rdcDto.TextForBlankDueDate);
        }

        [TestMethod]
        public async Task GetSessionConfigurationAsync_Success()
        {
            var sessionConfigurationDto = await this.configurationService.GetSessionConfigurationAsync();
            var sessionConfigurationEntity = await testConfigurationRepository.GetSessionConfigurationAsync();

            Assert.AreEqual(sessionConfigurationEntity.PasswordResetEnabled, sessionConfigurationDto.PasswordResetEnabled);
            Assert.AreEqual(sessionConfigurationEntity.UsernameRecoveryEnabled, sessionConfigurationDto.UsernameRecoveryEnabled);
        }


        /// <summary>
        /// Fake an ICurrentUserFactory implementation to construct ConfigurationService
        /// </summary>
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = "0001234",   /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        /// <summary>
        /// Builds a configuration service object.
        /// </summary>
        private void BuildConfigurationService()
        {
            testConfigurationRepository = new TestConfigurationRepository();

            // Set up current user
            currentUserFactory = new GenericUserFactory.UserFactory();
            backupConfigCurrentUserFactory = new BackupConfigUserFactory();

            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();

            var taxFormConfigurationDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormConfiguration, Dtos.Base.TaxFormConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormConfiguration, Dtos.Base.TaxFormConfiguration>()).Returns(taxFormConfigurationDtoAdapter);

            var taxFormConfiguration2DtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormConfiguration2, Dtos.Base.TaxFormConfiguration2>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormConfiguration2, Dtos.Base.TaxFormConfiguration2>()).Returns(taxFormConfiguration2DtoAdapter);

            var userProfileConfigurationAdapter = new AutoMapperAdapter<Domain.Base.Entities.UserProfileConfiguration, Dtos.Base.UserProfileConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.UserProfileConfiguration, Dtos.Base.UserProfileConfiguration>()).Returns(userProfileConfigurationAdapter);

            var userProfileConfiguration2Adapter = new AutoMapperAdapter<Domain.Base.Entities.UserProfileConfiguration2, Dtos.Base.UserProfileConfiguration2>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.UserProfileConfiguration2, Dtos.Base.UserProfileConfiguration2>()).Returns(userProfileConfiguration2Adapter);

            var emergencyInformationConfigurationAdapter = new AutoMapperAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration>()).Returns(emergencyInformationConfigurationAdapter);

            var emergencyInformationConfiguration2Adapter = new AutoMapperAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration2>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration2>()).Returns(emergencyInformationConfiguration2Adapter);

            var restrictionConfigurationAdapter = new AutoMapperAdapter<Domain.Base.Entities.RestrictionConfiguration, Dtos.Base.RestrictionConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.RestrictionConfiguration, Dtos.Base.RestrictionConfiguration>()).Returns(restrictionConfigurationAdapter);

            var severityStyleMappingAdapter = new AutoMapperAdapter<Domain.Base.Entities.SeverityStyleMapping, Dtos.Base.SeverityStyleMapping>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.SeverityStyleMapping, Dtos.Base.SeverityStyleMapping>()).Returns(severityStyleMappingAdapter);

            var privacyConfigurationMappingAdapter = new AutoMapperAdapter<Domain.Base.Entities.PrivacyConfiguration, Dtos.Base.PrivacyConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.PrivacyConfiguration, Dtos.Base.PrivacyConfiguration>()).Returns(privacyConfigurationMappingAdapter);

            var organizationalRelationshipConfigurationMappingAdapter = new AutoMapperAdapter<Domain.Base.Entities.OrganizationalRelationshipConfiguration, Dtos.Base.OrganizationalRelationshipConfiguration>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.OrganizationalRelationshipConfiguration, Dtos.Base.OrganizationalRelationshipConfiguration>()).Returns(organizationalRelationshipConfigurationMappingAdapter);

            var backupConfigurationDtoEntityAdapter = new AutoMapperAdapter<Dtos.Base.BackupConfiguration, Domain.Base.Entities.BackupConfiguration>(adapterRegistry.Object, logger);
            adapterRegistry.Setup(reg => reg.GetAdapter<Dtos.Base.BackupConfiguration, Domain.Base.Entities.BackupConfiguration>()).Returns(backupConfigurationDtoEntityAdapter);

            var backupConfigurationEntityDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.BackupConfiguration, Dtos.Base.BackupConfiguration>(adapterRegistry.Object, logger);
            adapterRegistry.Setup(reg => reg.GetAdapter<Domain.Base.Entities.BackupConfiguration, Dtos.Base.BackupConfiguration>()).Returns(backupConfigurationEntityDtoAdapter);

            var selfServiceConfigurationDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.SelfServiceConfiguration, Dtos.Base.SelfServiceConfiguration>(adapterRegistry.Object, logger);
            adapterRegistry.Setup(reg => reg.GetAdapter<Domain.Base.Entities.SelfServiceConfiguration, Dtos.Base.SelfServiceConfiguration>()).Returns(selfServiceConfigurationDtoAdapter);

            var requiredDocumentConfigurationAdapter = new AutoMapperAdapter<Domain.Base.Entities.RequiredDocumentConfiguration, Dtos.Base.RequiredDocumentConfiguration>(adapterRegistry.Object, logger);
            adapterRegistry.Setup(reg => reg.GetAdapter<Domain.Base.Entities.RequiredDocumentConfiguration, Dtos.Base.RequiredDocumentConfiguration>()).Returns(requiredDocumentConfigurationAdapter);

            var sessionConfigurationAdapter = new AutoMapperAdapter<Domain.Base.Entities.SessionConfiguration, Dtos.Base.SessionConfiguration>(adapterRegistry.Object, logger);
            adapterRegistry.Setup(reg => reg.GetAdapter<Domain.Base.Entities.SessionConfiguration, Dtos.Base.SessionConfiguration>()).Returns(sessionConfigurationAdapter);

            configurationService = new ConfigurationService(testConfigurationRepository, adapterRegistry.Object, currentUserFactory, roleRepository,
                FakeApiSettings, XmlSettingsRepository, ApiSettingsRepository, ResourceRepository, ReferenceDataRepository, loggerObject);

            backupConfigurationService = new ConfigurationService(configRepo, adapterRegistry.Object, backupConfigCurrentUserFactory, roleRepo,
                FakeApiSettings, XmlSettingsRepository, ApiSettingsRepository, ResourceRepository, ReferenceDataRepository, loggerObject);

            backupConfigurationServiceNoPermission = new ConfigurationService(configRepo, adapterRegistry.Object, currentUserFactory, roleRepo,
                FakeApiSettings, XmlSettingsRepository, ApiSettingsRepository, ResourceRepository, ReferenceDataRepository, loggerObject);

            // Set up the mock statement to make the configuration repository method return null.
            Mock<IConfigurationRepository> testConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            Domain.Base.Entities.TaxFormConfiguration configuration = new Domain.Base.Entities.TaxFormConfiguration(Domain.Base.Entities.TaxForms.Form1095C);
            Domain.Base.Entities.TaxFormConfiguration2 configuration2 = new Domain.Base.Entities.TaxFormConfiguration2(Domain.Base.TaxFormTypes.Form1095C);
            configuration = null;
            testConfigurationRepositoryMock.Setup<Task<Domain.Base.Entities.TaxFormConfiguration>>(x => x.GetTaxFormConsentConfigurationAsync(It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
            {
                return Task.FromResult(configuration);
            });
            testConfigurationRepositoryMock.Setup<Task<Domain.Base.Entities.TaxFormConfiguration2>>(x => x.GetTaxFormConsentConfiguration2Async(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(configuration2);
            });
            testConfigurationRepositoryMock.Setup<Task<Domain.Base.Entities.PrivacyConfiguration>>(x => x.GetPrivacyConfigurationAsync()).Returns(
                async () => await testConfigurationRepository.GetPrivacyConfigurationAsync());

            configRepoMock.Setup(repo => repo.GetSelfServiceConfigurationAsync()).Returns(testConfigurationRepository.GetSelfServiceConfigurationAsync());
           
            configurationServiceToReturnNull = new ConfigurationService(testConfigurationRepositoryMock.Object, adapterRegistry.Object, currentUserFactory, roleRepository,
                FakeApiSettings, XmlSettingsRepository, ApiSettingsRepository, ResourceRepository, ReferenceDataRepository, loggerObject);
        }

        #region backup configuration

        [TestMethod]
        public async Task ConfigurationService_WriteBackupConfigurationAsync()
        {
            // act
            var actual = await backupConfigurationService.WriteBackupConfigurationAsync(FakeBackupConfigurationDto);

            // assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationDto.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationDto.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationDto.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationDto.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationDto.ConfigData, actual.ConfigData);
            Assert.AreEqual(FakeBackupConfigurationDto.ConfigVersion, actual.ConfigVersion);
            Assert.AreEqual(FakeBackupConfigurationDto.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConfigurationService_WriteBackupConfigurationAsync_Nullargs()
        {
            // act
            var actual = await backupConfigurationService.WriteBackupConfigurationAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ConfigurationService_WriteBackupConfigurationAsync_BadPermission()
        {
            // act
            var actual = await backupConfigurationServiceNoPermission.WriteBackupConfigurationAsync(FakeBackupConfigurationDto);
        }


        [TestMethod]
        public async Task ConfigurationService_ReadBackupConfigurationByIdsAsync()
        {
            // act
            var actualSet = await backupConfigurationService.ReadBackupConfigurationAsync(fakeBackupConfigQueryCriteriaWithIds);
            var actual = actualSet.FirstOrDefault();

            // assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationDto.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationDto.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationDto.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationDto.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationDto.ConfigData, actual.ConfigData);
            Assert.AreEqual(FakeBackupConfigurationDto.ConfigVersion, actual.ConfigVersion);
            Assert.AreEqual(FakeBackupConfigurationDto.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConfigurationService_ReadBackupConfigurationAsync_Nullargs()
        {
            // act
            var actual = await backupConfigurationService.ReadBackupConfigurationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ConfigurationService_ReadBackupConfigurationAsync_Invalidargs()
        {
            // arrange
            var emptyCriteria = new BackupConfigurationQueryCriteria();
            // act
            var actual = await backupConfigurationService.ReadBackupConfigurationAsync(emptyCriteria);
        }


        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ConfigurationService_ReadBackupConfigurationAsync_BadPermissionException()
        {
            // act
            var actual = await backupConfigurationServiceNoPermission.ReadBackupConfigurationAsync(fakeBackupConfigQueryCriteriaWithIds);
        }


        [TestMethod]
        public async Task ConfigurationService_BackupApiConfigurationAsync()
        {
            // act
            await backupConfigurationService.BackupApiConfigurationAsync();
            // test passes if no exception thrown.
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ConfigurationService_BackupApiConfigurationAsync_BadPermission()
        {
            // act
            await backupConfigurationServiceNoPermission.BackupApiConfigurationAsync();
        }

        // todo: ConfigurationService_RestoreApiBackupConfigurationAsync
        // currently the restore logic can't be mocked.

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ConfigurationService_RestoreApiBackupConfigurationAsync_BadPermission()
        {
            // act
            await backupConfigurationServiceNoPermission.RestoreApiBackupConfigurationAsync();
        }

        #endregion

        #region GetTaxFormConsentConfigurationAsync tests

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_W2_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormW2);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormW2);

            Assert.AreEqual(Domain.Base.TaxFormTypes.FormW2, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_1095_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1095C);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1095C);

            Assert.AreEqual(Domain.Base.TaxFormTypes.Form1095C, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_1098_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1098);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1098);

            Assert.AreEqual(Domain.Base.TaxFormTypes.Form1098, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_T4_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT4);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT4);

            Assert.AreEqual(Domain.Base.TaxFormTypes.FormT4, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.HideConsent, configurationDto.HideConsent);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_T4A_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT4A);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT4A);

            Assert.AreEqual(Domain.Base.TaxFormTypes.FormT4A, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.HideConsent, configurationDto.HideConsent);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_T2202A_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT2202A);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.FormT2202A);

            Assert.AreEqual(Domain.Base.TaxFormTypes.FormT2202A, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.HideConsent, configurationDto.HideConsent);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_1099MI_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1099MI);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1099MI);

            Assert.AreEqual(Domain.Base.TaxFormTypes.Form1099MI, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_1099NEC_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1099NEC);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1099NEC);

            Assert.AreEqual(Domain.Base.TaxFormTypes.Form1099NEC, configurationDto.TaxForm);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_NonExistingForm()
        {
            var expectedParam = "taxForm";
            var actualParam = "";
            try
            {
                await this.configurationServiceToReturnNull.GetTaxFormConsentConfiguration2Async("FormXYZ");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetTaxFormConsentConfiguration2Async_NullForm()
        {
            var expectedParam = "taxForm";
            var actualParam = "";
            try
            {
                await this.configurationServiceToReturnNull.GetTaxFormConsentConfiguration2Async(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        //[TestMethod]
        //public async Task GetTaxFormConsentConfiguration2Async_1098_NullConfigurationReturned()
        //{
        //    var expectedParam = "configuration";
        //    var actualParam = "";
        //    try
        //    {
        //        await this.configurationServiceToReturnNull.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1095C);
        //    }
        //    catch (ArgumentNullException anex)
        //    {
        //        actualParam = anex.ParamName;
        //    }

        //    Assert.AreEqual(expectedParam, actualParam);
        //}
        //[TestMethod]
        //public async Task GetTaxFormConsentConfiguration2Async_1099MI_NullConfigurationReturned()
        //{
        //    var expectedParam = "configuration";
        //    var actualParam = "";
        //    try
        //    {
        //        await this.configurationServiceToReturnNull.GetTaxFormConsentConfiguration2Async(Domain.Base.TaxFormTypes.Form1099MI);
        //    }
        //    catch (ArgumentNullException anex)
        //    {
        //        actualParam = anex.ParamName;
        //    }

        //    Assert.AreEqual(expectedParam, actualParam);
        //}

        #region Obsolete

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_W2_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.FormW2);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.FormW2);

            Assert.AreEqual(Dtos.Base.TaxForms.FormW2, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_1095_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.Form1095C);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.Form1095C);

            Assert.AreEqual(Dtos.Base.TaxForms.Form1095C, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_1098_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.Form1098);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.Form1098);

            Assert.AreEqual(Dtos.Base.TaxForms.Form1098, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_T4_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.FormT4);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.FormT4);

            Assert.AreEqual(Dtos.Base.TaxForms.FormT4, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_T4A_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.FormT4A);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.FormT4A);

            Assert.AreEqual(Dtos.Base.TaxForms.FormT4A, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_T2202A_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.FormT2202A);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.FormT2202A);

            Assert.AreEqual(Dtos.Base.TaxForms.FormT2202A, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_1099MI_Success()
        {
            var configurationDto = await this.configurationService.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.Form1099MI);
            var configurationDomainEntity = await testConfigurationRepository.GetTaxFormConsentConfigurationAsync(Domain.Base.Entities.TaxForms.Form1099MI);

            Assert.AreEqual(Dtos.Base.TaxForms.Form1099MI, configurationDto.TaxFormId);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentText, configurationDto.ConsentText);
            Assert.AreEqual(configurationDomainEntity.ConsentParagraphs.ConsentWithheldText, configurationDto.ConsentWithheldText);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_1098_NullConfigurationReturned()
        {
            var expectedParam = "configuration";
            var actualParam = "";
            try
            {
                await this.configurationServiceToReturnNull.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.Form1095C);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        // Obsolete
        public async Task GetTaxFormConsentConfigurationAsync_1099MI_NullConfigurationReturned()
        {
            var expectedParam = "configuration";
            var actualParam = "";
            try
            {
                await this.configurationServiceToReturnNull.GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms.Form1099MI);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #endregion
    }

    public class BackupConfigUserFactory : ICurrentUserFactory
    {
        public ICurrentUser CurrentUser
        {
            get
            {
                return new CurrentUser(new Claims()
                {
                    PersonId = "0000002",
                    ControlId = "123",
                    Name = "Jimmy Backupconfig",
                    SecurityToken = "321",
                    SessionTimeout = 30,
                    UserName = "backupadmin",
                    Roles = new List<string>() { "Backupconfig Admin" },
                    SessionFixationId = "abc123"
                });
            }
        }
    }
}
