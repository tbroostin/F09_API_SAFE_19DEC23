// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;


namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ConfigurationControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private IConfigurationService configService;
        private Mock<IConfigurationService> configServiceMock;
        private IProxyService proxyService;
        private Mock<IProxyService> proxyServiceMock;
        private ConfigurationController configurationController;
        private Dtos.Base.ProxyConfiguration configuration;
        private Dtos.Base.PrivacyConfiguration privacyConfiguration;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private ILogger logger;

        private BackupConfiguration FakeBackupConfigurationDataRecord;
        private IEnumerable<BackupConfiguration> FakeBackupConfigurationResultList;
        private BackupConfigurationQueryCriteria FakeBackupConfigurationDataQueryCriteria;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            proxyServiceMock = new Mock<IProxyService>();
            proxyService = proxyServiceMock.Object;

            configServiceMock = new Mock<IConfigurationService>();
            configService = configServiceMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            configuration = BuildProxyConfiguration();
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            FakeBackupConfigurationDataRecord = new BackupConfiguration();
            FakeBackupConfigurationDataRecord.Id = "56c1fb34-9e3e-49a0-b2b0-60751a877855";
            FakeBackupConfigurationDataRecord.Namespace = "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt";
            FakeBackupConfigurationDataRecord.ProductId = "Colleague Web API";
            FakeBackupConfigurationDataRecord.ProductVersion = "1.18.0.0";
            FakeBackupConfigurationDataRecord.CreatedDateTime = new DateTimeOffset(2017, 1, 1, 1, 1, 1, new TimeSpan(0));

            FakeBackupConfigurationResultList = new List<BackupConfiguration>() { FakeBackupConfigurationDataRecord };

            FakeBackupConfigurationDataQueryCriteria = new BackupConfigurationQueryCriteria();
            FakeBackupConfigurationDataQueryCriteria.Namespace = FakeBackupConfigurationDataRecord.Namespace;
            FakeBackupConfigurationDataQueryCriteria.ConfigurationIds = new List<string>() { FakeBackupConfigurationDataRecord.Id };
        }

        [TestCleanup]
        public void Cleanup()
        {
            configurationController = null;
            configService = null;
            configuration = null;
        }

        [TestMethod]
        public async Task ConfigurationController_GetProxyConfigurationAsync_Valid()
        {
            proxyServiceMock.Setup(x => x.GetProxyConfigurationAsync()).Returns(Task.FromResult(configuration));
            configuration = BuildProxyConfiguration();
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetProxyConfigurationAsync();

            Assert.IsNotNull(config);
            Assert.AreEqual(configuration.DisclosureReleaseDocumentId, config.DisclosureReleaseDocumentId);
            Assert.AreEqual(configuration.ProxyEmailDocumentId, config.ProxyEmailDocumentId);
            Assert.AreEqual(configuration.ProxyIsEnabled, config.ProxyIsEnabled);
            Assert.AreEqual(configuration.WorkflowGroups.Count(), config.WorkflowGroups.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetProxyConfigurationAsync_BadRequest()
        {
            proxyServiceMock.Setup(x => x.GetProxyConfigurationAsync()).Throws(new ConfigurationException());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetProxyConfigurationAsync();
        }

        [TestMethod]
        public async Task ConfigurationController_GetPrivacyConfigurationAsync_Valid()
        {
            privacyConfiguration = new PrivacyConfiguration()
            {
                RecordDenialMessage = "Record not accesible due to a privacy request"
            };
            configServiceMock.Setup(x => x.GetPrivacyConfigurationAsync()).ReturnsAsync(privacyConfiguration);
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetPrivacyConfigurationAsync();

            Assert.IsNotNull(config);
            Assert.AreEqual(privacyConfiguration.RecordDenialMessage, config.RecordDenialMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetPrivacyConfigurationAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.GetPrivacyConfigurationAsync()).ThrowsAsync(new ConfigurationException());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);
            var config = await configurationController.GetPrivacyConfigurationAsync();
        }

        [TestMethod]
        public async Task ConfigurationController_GetOrganizationalRelationshipConfigurationAsync_Valid()
        {
            var organizationalRelationshipConfiguration = new OrganizationalRelationshipConfiguration()
            {
                RelationshipTypeCodeMapping = new Dictionary<OrganizationalRelationshipType, List<string>>
                {
                    { OrganizationalRelationshipType.Manager, new List<string> { "MGR" } }
                }
            };
            configServiceMock.Setup(x => x.GetOrganizationalRelationshipConfigurationAsync()).ReturnsAsync(organizationalRelationshipConfiguration);
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetOrganizationalRelationshipConfigurationAsync();

            Assert.IsNotNull(config);
            CollectionAssert.AreEqual(organizationalRelationshipConfiguration.RelationshipTypeCodeMapping[OrganizationalRelationshipType.Manager], config.RelationshipTypeCodeMapping[OrganizationalRelationshipType.Manager]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetOrganizationalRelationshipConfigurationAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.GetOrganizationalRelationshipConfigurationAsync()).ThrowsAsync(new Exception());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);
            var config = await configurationController.GetOrganizationalRelationshipConfigurationAsync();
        }

        private Dtos.Base.ProxyConfiguration BuildProxyConfiguration()
        {
            var config = new Dtos.Base.ProxyConfiguration()
            {
                DisclosureReleaseDocumentId = "PROX.DISC",
                ProxyEmailDocumentId = "PROX.EMAIL",
                ProxyIsEnabled = true,
                WorkflowGroups = new List<Dtos.Base.ProxyWorkflowGroup>()
                {
                    new Dtos.Base.ProxyWorkflowGroup()
                    {
                        Code = "SF",
                        Description = "Student Finance",
                        Workflows = new List<Dtos.Base.ProxyWorkflow>()
                        {
                            new Dtos.Base.ProxyWorkflow() { Code = "SFAA", Description = "Student Finance Account Activity", WorkflowGroupId = "SF", IsEnabled = true },
                            new Dtos.Base.ProxyWorkflow() { Code = "SFMAP", Description = "Student Finance Make a Payment", WorkflowGroupId = "SF", IsEnabled = true }
                        }
                    }
                }
            };

            return config;
        }



        [TestMethod]
        public async Task ConfigurationController_PostConfigBackupDataAsync_Valid()
        {
            configServiceMock.Setup(x => x.WriteBackupConfigurationAsync(FakeBackupConfigurationDataRecord))
                .Returns(Task.FromResult(FakeBackupConfigurationDataRecord));


            var actual = await configurationController.PostConfigBackupDataAsync(FakeBackupConfigurationDataRecord);

            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_PostConfigBackupDataAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.WriteBackupConfigurationAsync(FakeBackupConfigurationDataRecord))
                .Throws(new ConfigurationException());
            var actual = await configurationController.PostConfigBackupDataAsync(FakeBackupConfigurationDataRecord);
        }

        [TestMethod]
        public async Task ConfigurationController_GetConfigBackupDataAsync_Valid()
        {
            configServiceMock.Setup(x => x.ReadBackupConfigurationAsync(It.IsAny<BackupConfigurationQueryCriteria>()))
                .Returns(Task.FromResult(FakeBackupConfigurationResultList));

            var actual = await configurationController.GetConfigBackupDataAsync(FakeBackupConfigurationDataRecord.Id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetConfigBackupDataAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.ReadBackupConfigurationAsync(null))
                .Throws(new ConfigurationException());
            var actual = await configurationController.GetConfigBackupDataAsync(null);
        }

        [TestMethod]
        public async Task ConfigurationController_QueryBackupConfigDataByPostAsync_Valid()
        {
            configServiceMock.Setup(x => x.ReadBackupConfigurationAsync(FakeBackupConfigurationDataQueryCriteria))
                .Returns(Task.FromResult(FakeBackupConfigurationResultList));

            var actualSet = await configurationController.QueryBackupConfigDataByPostAsync(FakeBackupConfigurationDataQueryCriteria);
            var actual = actualSet.FirstOrDefault();

            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationDataRecord.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_QueryBackupConfigDataByPostAsync_BadRequest()
        {
            var actual = await configurationController.QueryBackupConfigDataByPostAsync(null);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_PostBackupApiConfigAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.BackupApiConfigurationAsync())
                .Throws(new ConfigurationException());
            await configurationController.PostBackupApiConfigAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task configurationController_PostRestoreApiConfigAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.BackupApiConfigurationAsync())
                .Throws(new ConfigurationException());
            await configurationController.PostRestoreApiConfigAsync(new DateTimeOffset(2017, 1, 1, 1, 1, 1, new TimeSpan(0)));

        }

        [TestMethod]
        public async Task ConfigurationController_GetSelfServiceConfigurationAsync_Valid()
        {
            var configuration = new SelfServiceConfiguration()
            {
                AlwaysUseClipboardForBulkMailToLinks = true
            };
            configServiceMock.Setup(x => x.GetSelfServiceConfigurationAsync()).ReturnsAsync(configuration);
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetSelfServiceConfigurationAsync();

            Assert.IsNotNull(config);
            Assert.AreEqual(configuration.AlwaysUseClipboardForBulkMailToLinks, config.AlwaysUseClipboardForBulkMailToLinks);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetSelfServiceConfigurationAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.GetSelfServiceConfigurationAsync()).ThrowsAsync(new ApplicationException());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);
            var config = await configurationController.GetSelfServiceConfigurationAsync();
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Error occurred while retrieving Self-Service Configuration."));
        }

        [TestMethod]
        public async Task ConfigurationController_GetRequiredDocumentConfigurationAsync_Valid()
        {
            var configuration = new RequiredDocumentConfiguration()
            {
                SuppressInstance = false,
                PrimarySortField = WebSortField.Status,
                SecondarySortField = WebSortField.OfficeDescription,
                TextForBlankStatus = "",
                TextForBlankDueDate = ""
            };
            configServiceMock.Setup(x => x.GetRequiredDocumentConfigurationAsync()).ReturnsAsync(configuration);
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetRequiredDocumentConfigurationAsync();

            Assert.IsNotNull(config);
            Assert.AreEqual(configuration.SuppressInstance, config.SuppressInstance);
            Assert.AreEqual(configuration.PrimarySortField, config.PrimarySortField);
            Assert.AreEqual(configuration.SecondarySortField, config.SecondarySortField);
            Assert.AreEqual(configuration.TextForBlankStatus, config.TextForBlankStatus);
            Assert.AreEqual(configuration.TextForBlankDueDate, config.TextForBlankDueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetRequiredDocumentConfigurationAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.GetRequiredDocumentConfigurationAsync()).ThrowsAsync(new ApplicationException());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);
            var config = await configurationController.GetRequiredDocumentConfigurationAsync();
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Error occurred while retrieving Required Document Configuration."));
        }

        [TestMethod]
        public async Task ConfigurationController_GetSessionConfigurationAsync_Valid()
        {
            var configuration = new SessionConfiguration()
            {
                PasswordResetEnabled = true,
                UsernameRecoveryEnabled = true
            };
            configServiceMock.Setup(x => x.GetSessionConfigurationAsync()).ReturnsAsync(configuration);
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);

            var config = await configurationController.GetSessionConfigurationAsync();

            Assert.IsNotNull(config);
            Assert.AreEqual(configuration.PasswordResetEnabled, config.PasswordResetEnabled);
            Assert.AreEqual(configuration.UsernameRecoveryEnabled, config.UsernameRecoveryEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationController_GetSessionConfigurationAsync_BadRequest()
        {
            configServiceMock.Setup(x => x.GetSessionConfigurationAsync()).ThrowsAsync(new ApplicationException());
            configurationController = new ConfigurationController(adapterRegistry, configService, proxyService, logger);
            var config = await configurationController.GetSessionConfigurationAsync();
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), "Error occurred while retrieving Session Configuration."));
        }
    }
}
