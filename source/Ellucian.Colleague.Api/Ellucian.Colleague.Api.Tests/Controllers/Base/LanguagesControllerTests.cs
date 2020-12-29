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
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class LanguagesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private ILanguagesService languagesService;
        private Mock<ILanguagesService> languagesServiceMock;
        private Mock<ILogger> loggerMock;
        private LanguagesController languagesController;
        private IEnumerable<Domain.Base.Entities.Language2> allLanguages;
        private List<Dtos.Languages> languagesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        //private Mock<ILanguageIsoCodesService> languageIsoCodesServiceMock;
        private IEnumerable<Domain.Base.Entities.LanguageIsoCodes> allLanguageIsoCodes;
        private List<Dtos.LanguageIsoCodes> languageIsoCodesCollection;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            
            languagesServiceMock = new Mock<ILanguagesService>();
            languagesService = languagesServiceMock.Object;
            loggerMock = new Mock<ILogger>();
            languagesCollection = new List<Dtos.Languages>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            
            allLanguages = new List<Domain.Base.Entities.Language2>()
                {
                    new Domain.Base.Entities.Language2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") {IsoCode = "AT" },
                    new Domain.Base.Entities.Language2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic") {IsoCode = "AC" },
                    new Domain.Base.Entities.Language2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural") {IsoCode = "CU" }
                };

            foreach (var source in allLanguages)
            {
                var languages = new Ellucian.Colleague.Dtos.Languages
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    ISOCode = source.IsoCode
                };
                languagesCollection.Add(languages);
            }

            languagesController = new LanguagesController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object,languagesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            languagesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            languageIsoCodesCollection = new List<Dtos.LanguageIsoCodes>();

            allLanguageIsoCodes = new List<Domain.Base.Entities.LanguageIsoCodes>()
                {
                    new Domain.Base.Entities.LanguageIsoCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") {InactiveFlag = "Y" },
                    new Domain.Base.Entities.LanguageIsoCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.LanguageIsoCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allLanguageIsoCodes)
            {
                var languageIsoCodes = new Ellucian.Colleague.Dtos.LanguageIsoCodes
                {
                    Id = source.Guid,
                    IsoCode = source.Code,
                    Title = source.Description
                };
                languageIsoCodesCollection.Add(languageIsoCodes);
            }            
        }

        [TestCleanup]
        public void Cleanup()
        {
            languagesController = null;
            allLanguages = null;
            languagesCollection = null;
            loggerMock = null;
            languagesServiceMock = null;

            allLanguageIsoCodes = null;
            languageIsoCodesCollection = null;
        }

        [TestMethod]
        public async Task LanguagesController_GetLanguages_ValidateFields_Nocache()
        {
            languagesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false)).ReturnsAsync(languagesCollection);

            var sourceContexts = (await languagesController.GetLanguagesAsync()).ToList();
            Assert.AreEqual(languagesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = languagesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task LanguagesController_GetLanguages_ValidateFields_Cache()
        {
            languagesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(true)).ReturnsAsync(languagesCollection);

            var sourceContexts = (await languagesController.GetLanguagesAsync()).ToList();
            Assert.AreEqual(languagesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = languagesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_KeyNotFoundException()
        {
            //
            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false))
                .Throws<KeyNotFoundException>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_PermissionsException()
        {

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false))
                .Throws<PermissionsException>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_ArgumentException()
        {

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false))
                .Throws<ArgumentException>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_RepositoryException()
        {

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false))
                .Throws<RepositoryException>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_IntegrationApiException()
        {

            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false))
                .Throws<IntegrationApiException>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        public async Task LanguagesController_GetLanguagesByGuidAsync_ValidateFields()
        {
            var expected = languagesCollection.FirstOrDefault();
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await languagesController.GetLanguagesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguages_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesAsync(false)).Throws<Exception>();
            await languagesController.GetLanguagesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuidAsync_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await languagesController.GetLanguagesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_KeyNotFoundException()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_PermissionsException()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_ArgumentException()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_RepositoryException()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_IntegrationApiException()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_GetLanguagesByGuid_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await languagesController.GetLanguagesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_PostLanguagesAsync_Exception()
        {
            await languagesController.PostLanguagesAsync(languagesCollection.FirstOrDefault());
        }

        [TestMethod]
        public async Task LanguagesController_PutLanguagesAsync()
        {
            var languageDTO = languagesCollection.First();
            languagesServiceMock.Setup(x => x.GetLanguagesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(languageDTO);
            languagesServiceMock.Setup(x => x.UpdateLanguageAsync(It.IsAny<Languages>(), It.IsAny<bool>()))
                .ReturnsAsync(languagesCollection.First());

            var sourceContext = languagesCollection.FirstOrDefault();
            var result = await languagesController.PutLanguagesAsync(sourceContext.Id, sourceContext);
            Assert.AreEqual(sourceContext.ISOCode, result.ISOCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task LanguagesController_DeleteLanguagesAsync_Exception()
        {
            await languagesController.DeleteLanguagesAsync(languagesCollection.FirstOrDefault().Id);
        }

        [TestMethod]
        public async Task languagesController_GetLanguageIsoCodes_ValidateFields_Nocache()
        {
            languagesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false)).ReturnsAsync(languageIsoCodesCollection);

            var sourceContexts = (await languagesController.GetLanguageIsoCodesAsync()).ToList();
            Assert.AreEqual(languageIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = languageIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task languagesController_GetLanguageIsoCodes_ValidateFields_Cache()
        {
            languagesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(true)).ReturnsAsync(languageIsoCodesCollection);

            var sourceContexts = (await languagesController.GetLanguageIsoCodesAsync()).ToList();
            Assert.AreEqual(languageIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = languageIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_KeyNotFoundException()
        {
            //
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false))
                .Throws<KeyNotFoundException>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_PermissionsException()
        {

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false))
                .Throws<PermissionsException>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_ArgumentException()
        {

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_RepositoryException()
        {

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false))
                .Throws<RepositoryException>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_IntegrationApiException()
        {

            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false))
                .Throws<IntegrationApiException>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        public async Task languagesController_GetLanguageIsoCodesByGuidAsync_ValidateFields()
        {
            var expected = languageIsoCodesCollection.FirstOrDefault();
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await languagesController.GetLanguageIsoCodesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodes_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodesAsync(false)).Throws<Exception>();
            await languagesController.GetLanguageIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuidAsync_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_KeyNotFoundException()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_PermissionsException()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_ArgumentException()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_RepositoryException()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_IntegrationApiException()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_GetLanguageIsoCodesByGuid_Exception()
        {
            languagesServiceMock.Setup(x => x.GetLanguageIsoCodeByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await languagesController.GetLanguageIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_PostLanguageIsoCodesAsync_Exception()
        {
            await languagesController.PostLanguageIsoCodesAsync(languageIsoCodesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_PutLanguageIsoCodesAsync_Exception()
        {
            var sourceContext = languageIsoCodesCollection.FirstOrDefault();
            await languagesController.PutLanguageIsoCodesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task languagesController_DeleteLanguageIsoCodesAsync_Exception()
        {
            await languagesController.DeleteLanguageIsoCodesAsync(languageIsoCodesCollection.FirstOrDefault().Id);
        }
    }
}