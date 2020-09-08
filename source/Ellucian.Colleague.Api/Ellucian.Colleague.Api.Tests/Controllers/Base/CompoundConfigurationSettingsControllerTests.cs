//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CompoundConfigurationSettingsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICompoundConfigurationSettingsService> compoundConfigurationSettingsServiceMock;
        private Mock<ILogger> loggerMock;
        private CompoundConfigurationSettingsController compoundConfigurationSettingsController;
        private IEnumerable<Domain.Base.Entities.CompoundConfigurationSettings> allCompoundConfigurationSettings;
        private List<Dtos.CompoundConfigurationSettings> compoundConfigurationSettingsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            compoundConfigurationSettingsServiceMock = new Mock<ICompoundConfigurationSettingsService>();
            loggerMock = new Mock<ILogger>();
            compoundConfigurationSettingsCollection = new List<Dtos.CompoundConfigurationSettings>();



            allCompoundConfigurationSettings = new List<Domain.Base.Entities.CompoundConfigurationSettings>()
                {
                    new Domain.Base.Entities.CompoundConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.CompoundConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.CompoundConfigurationSettings("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allCompoundConfigurationSettings)
            {
                var compoundConfigurationSettings = new Ellucian.Colleague.Dtos.CompoundConfigurationSettings
                {
                    Id = source.Guid,
                    //Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                compoundConfigurationSettingsCollection.Add(compoundConfigurationSettings);
            }

            compoundConfigurationSettingsController = new CompoundConfigurationSettingsController(compoundConfigurationSettingsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            compoundConfigurationSettingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            compoundConfigurationSettingsController = null;
            allCompoundConfigurationSettings = null;
            compoundConfigurationSettingsCollection = null;
            loggerMock = null;
            compoundConfigurationSettingsServiceMock = null;
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_ValidateFields_Nocache()
        {
            compoundConfigurationSettingsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false)).ReturnsAsync(compoundConfigurationSettingsCollection);

            var sourceContexts = (await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(compoundConfigurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = compoundConfigurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_ValidateFields_Cache()
        {
            compoundConfigurationSettingsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), true)).ReturnsAsync(compoundConfigurationSettingsCollection);

            var sourceContexts = (await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>())).ToList();
            Assert.AreEqual(compoundConfigurationSettingsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = compoundConfigurationSettingsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_KeyNotFoundException()
        {
            //
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false))
                .Throws<KeyNotFoundException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_PermissionsException()
        {

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false))
                .Throws<PermissionsException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_ArgumentException()
        {

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false))
                .Throws<ArgumentException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_RepositoryException()
        {

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false))
                .Throws<RepositoryException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_IntegrationApiException()
        {

            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false))
                .Throws<IntegrationApiException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuidAsync_ValidateFields()
        {
            var expected = compoundConfigurationSettingsCollection.FirstOrDefault();
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettings_Exception()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsAsync(It.IsAny<List<CompoundConfigurationSettingsEthos>>(), false)).Throws<Exception>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuidAsync_Exception()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_KeyNotFoundException()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_PermissionsException()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_ArgumentException()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_RepositoryException()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_IntegrationApiException()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_GetCompoundConfigurationSettingsByGuid_Exception()
        {
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await compoundConfigurationSettingsController.GetCompoundConfigurationSettingsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_PostCompoundConfigurationSettingsAsync_Exception()
        {
            await compoundConfigurationSettingsController.PostCompoundConfigurationSettingsAsync(compoundConfigurationSettingsCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsController_PutCompoundConfigurationSettingsAsync_Exception()
        {
            var expected = compoundConfigurationSettingsCollection.FirstOrDefault();
            compoundConfigurationSettingsServiceMock.Setup(x => x.GetCompoundConfigurationSettingsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var sourceContext = compoundConfigurationSettingsCollection.FirstOrDefault();
            await compoundConfigurationSettingsController.PutCompoundConfigurationSettingsAsync(sourceContext.Id, sourceContext);
            Assert.AreEqual(expected.Id, sourceContext.Id);
            Assert.AreEqual(expected.Title, sourceContext.Title);
   
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CompoundConfigurationSettingsController_DeleteCompoundConfigurationSettingsAsync_Exception()
        {
            await compoundConfigurationSettingsController.DeleteCompoundConfigurationSettingsAsync(compoundConfigurationSettingsCollection.FirstOrDefault().Id);
        }
    }
}