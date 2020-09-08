//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class MappingSettingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IMappingSettingsService> mappingSettingsServiceMock;
        private Mock<ILogger> loggerMock;
        private MappingSettingsController mappingSettingsController;
        private IEnumerable<Domain.Base.Entities.MappingSettings> allMappingSettings;
        private List<Dtos.MappingSettings> mappingSettingsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            mappingSettingsServiceMock = new Mock<IMappingSettingsService>();
            loggerMock = new Mock<ILogger>();
            mappingSettingsCollection = new List<Dtos.MappingSettings>();

            allMappingSettings = new List<Domain.Base.Entities.MappingSettings>()
                {
                
                    new Domain.Base.Entities.MappingSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Email Types")
                    {
                        EthosResource = "email-types",
                        EthosPropertyName = "type",
                        Enumeration = "personal",
                        SourceTitle = "Primary",
                        SourceValue = "PRI"
                    },
                    new Domain.Base.Entities.MappingSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Sessions")
                    {
                        EthosResource = "academic-periods",
                        EthosPropertyName = "category",
                        Enumeration = "term",
                        SourceTitle = "Fall",
                        SourceValue = "FA"
                    }
                };

            foreach (var source in allMappingSettings)
            {
                var mappingSettings = new Ellucian.Colleague.Dtos.MappingSettings
                {
                    Id = source.Guid,
                    Title = source.Description,
                };
                mappingSettingsCollection.Add(mappingSettings);
            }

            mappingSettingsController = new MappingSettingsController(mappingSettingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            mappingSettingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            mappingSettingsController = null;
            allMappingSettings = null;
            mappingSettingsCollection = null;
            loggerMock = null;
            mappingSettingsServiceMock = null;
        }

        [TestMethod]
        public async Task MappingSettingsController_GetMappingSettings_Nocache()
        {
            var tuple = new Tuple<IEnumerable<Dtos.MappingSettings>, int>(mappingSettingsCollection, 1);

            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.MappingSettings>(), 
                It.IsAny<bool>())).ReturnsAsync(tuple);

            mappingSettingsController.Request.Headers.CacheControl =
                     new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var mappingCollection = await mappingSettingsController.GetMappingSettingsAsync(new Paging(1, 0), It.IsAny<QueryStringFilter>());
            Assert.IsTrue(mappingCollection is IHttpActionResult);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettings_Exception()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.MappingSettings>(), It.IsAny<bool>())).Throws<Exception>();
            await mappingSettingsController.GetMappingSettingsAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter);
        }

        [TestMethod]
        public async Task MappingSettingsController_GetMappingSettingsByGuidAsync_ValidateFields()
        {
            var expected = mappingSettingsCollection.FirstOrDefault();
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await mappingSettingsController.GetMappingSettingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuidAsync_Exception()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_KeyNotFoundException()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_PermissionsException()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_ArgumentException()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_RepositoryException()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_IntegrationApiException()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_GetMappingSettingsByGuid_Exception()
        {
            mappingSettingsServiceMock.Setup(x => x.GetMappingSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await mappingSettingsController.GetMappingSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_PostMappingSettingsAsync_Exception()
        {
            await mappingSettingsController.PostMappingSettingsAsync(mappingSettingsCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task MappingSettingsController_PutMappingSettingsAsync_ValidateFields()
        {
            var expected = mappingSettingsCollection.ToArray().FirstOrDefault();
            mappingSettingsServiceMock.Setup(x => x.UpdateMappingSettingsAsync(expected)).ReturnsAsync(expected);

            var sourceContext = mappingSettingsCollection.FirstOrDefault();
            var actual = await mappingSettingsController.PutMappingSettingsAsync(sourceContext.Id, sourceContext);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MappingSettingsController_DeleteMappingSettingsAsync_Exception()
        {
            await mappingSettingsController.DeleteMappingSettingsAsync(mappingSettingsCollection.FirstOrDefault().Id);
        }
    }
}