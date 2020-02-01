//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ConfigurationSettingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IConfigurationSettingsService> configurationSettingsServiceMock;
        private Mock<ILogger> loggerMock;
        private ConfigurationSettingsController configurationSettingsController;
        private IEnumerable<Domain.Base.Entities.ConfigurationSettings> allConfigurationSettings;
        private List<ConfigurationSettings> configurationSettingsCollection;
        private readonly string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            configurationSettingsServiceMock = new Mock<IConfigurationSettingsService>();
            loggerMock = new Mock<ILogger>();
            configurationSettingsCollection = new List<ConfigurationSettings>();

            allConfigurationSettings = new List<Domain.Base.Entities.ConfigurationSettings>()
                {
                    new Domain.Base.Entities.ConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Person Match Criteria")
                    {
                        EthosResources = new List<string>() { "persons" },
                        SourceTitle = "Integration Person Matching",
                        SourceValue = "INTG.PERSON",
                        FieldHelp = "Long Description for field help."
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "Address Match Criteria")
                    {
                        EthosResources = new List<string>() { "addresses" },
                        SourceTitle = "Integration Address Matching",
                        SourceValue = "INTG.ADDRESS",
                        FieldHelp = "Long Description for field help."
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "Check Faculty Load")
                    {
                        EthosResources = new List<string>() { "section-instructors" },
                        SourceTitle = "No",
                        SourceValue = "N",
                        FieldHelp = "Long Description for field help."
                    }
                };

            foreach (var source in allConfigurationSettings)
            {
                var configurationSettings = new ConfigurationSettings
                {
                    Id = source.Guid,
                    Title = source.Description,
                    Description = source.FieldHelp,
                    Ethos = new ConfigurationSettingsEthos() { Resources = source.EthosResources },
                    Source = new ConfigurationSettingsSource() { Title = source.SourceTitle, Value = source.SourceValue }
                };
                configurationSettingsCollection.Add(configurationSettings);
            }

            configurationSettingsController = new ConfigurationSettingsController(configurationSettingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            configurationSettingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            configurationSettingsController = null;
            allConfigurationSettings = null;
            configurationSettingsCollection = null;
            loggerMock = null;
            configurationSettingsServiceMock = null;
        }

        [TestMethod]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_ValidateFields_Nocache()
        {
            configurationSettingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false)).ReturnsAsync(configurationSettingsCollection);

            var sourceContexts = (await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(configurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = configurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_ValidateFields_Cache()
        {
            configurationSettingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), true)).ReturnsAsync(configurationSettingsCollection);

            var sourceContexts = (await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter)).ToList();
            Assert.AreEqual(configurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = configurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_KeyNotFoundException()
        {
            //
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false))
                .Throws<KeyNotFoundException>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_PermissionsException()
        {

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false))
                .Throws<PermissionsException>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_ArgumentException()
        {

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false))
                .Throws<ArgumentException>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_RepositoryException()
        {

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false))
                .Throws<RepositoryException>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_IntegrationApiException()
        {

            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false))
                .Throws<IntegrationApiException>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuidAsync_ValidateFields()
        {
            var expected = configurationSettingsCollection.FirstOrDefault();
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettings_Exception()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsAsync(It.IsAny<List<string>>(), false)).Throws<Exception>();
            await configurationSettingsController.GetConfigurationSettingsAsync(criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuidAsync_Exception()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_KeyNotFoundException()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_PermissionsException()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_ArgumentException()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_RepositoryException()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_IntegrationApiException()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_GetConfigurationSettingsByGuid_Exception()
        {
            configurationSettingsServiceMock.Setup(x => x.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await configurationSettingsController.GetConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_PostConfigurationSettingsAsync_Exception()
        {
            await configurationSettingsController.PostConfigurationSettingsAsync(configurationSettingsCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task ConfigurationSettingsController_PutConfigurationSettingsAsync_ValidateFields()
        {
            var expected = configurationSettingsCollection.FirstOrDefault();
            configurationSettingsServiceMock.Setup(x => x.UpdateConfigurationSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = configurationSettingsCollection.FirstOrDefault();
            var actual = await configurationSettingsController.PutConfigurationSettingsAsync(sourceContext.Id, sourceContext);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ConfigurationSettingsController_DeleteConfigurationSettingsAsync_Exception()
        {
            await configurationSettingsController.DeleteConfigurationSettingsAsync(configurationSettingsCollection.FirstOrDefault().Id);
        }
    }
}