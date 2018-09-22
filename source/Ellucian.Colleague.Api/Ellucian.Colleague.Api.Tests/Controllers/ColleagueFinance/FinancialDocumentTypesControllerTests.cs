//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FinancialDocumentTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialDocumentTypesService> financialDocumentTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialDocumentTypesController financialDocumentTypesController;
        private IEnumerable<Domain.ColleagueFinance.Entities.GlSourceCodes> allGlSourceCodes;
        private List<Dtos.FinancialDocumentTypes> financialDocumentTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialDocumentTypesServiceMock = new Mock<IFinancialDocumentTypesService>();
            loggerMock = new Mock<ILogger>();
            financialDocumentTypesCollection = new List<Dtos.FinancialDocumentTypes>();

            allGlSourceCodes = new List<Domain.ColleagueFinance.Entities.GlSourceCodes>()
                {
                    new Domain.ColleagueFinance.Entities.GlSourceCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic","1"),
                    new Domain.ColleagueFinance.Entities.GlSourceCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "2"),
                    new Domain.ColleagueFinance.Entities.GlSourceCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "3")
                };

            foreach (var source in allGlSourceCodes)
            {
                var financialDocumentTypes = new Ellucian.Colleague.Dtos.FinancialDocumentTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                financialDocumentTypesCollection.Add(financialDocumentTypes);
            }

            financialDocumentTypesController = new FinancialDocumentTypesController(financialDocumentTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialDocumentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialDocumentTypesController = null;
            allGlSourceCodes = null;
            financialDocumentTypesCollection = null;
            loggerMock = null;
            financialDocumentTypesServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_ValidateFields_Nocache()
        {
            financialDocumentTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false)).ReturnsAsync(financialDocumentTypesCollection);

            var sourceContexts = (await financialDocumentTypesController.GetFinancialDocumentTypesAsync()).ToList();
            Assert.AreEqual(financialDocumentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialDocumentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_ValidateFields_Cache()
        {
            financialDocumentTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(true)).ReturnsAsync(financialDocumentTypesCollection);

            var sourceContexts = (await financialDocumentTypesController.GetFinancialDocumentTypesAsync()).ToList();
            Assert.AreEqual(financialDocumentTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialDocumentTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_KeyNotFoundException()
        {
            //
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_PermissionsException()
        {

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false))
                .Throws<PermissionsException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_ArgumentException()
        {

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false))
                .Throws<ArgumentException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_RepositoryException()
        {

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false))
                .Throws<RepositoryException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_IntegrationApiException()
        {

            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false))
                .Throws<IntegrationApiException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuidAsync_ValidateFields()
        {
            var expected = financialDocumentTypesCollection.FirstOrDefault();
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(expected.Id, false)).ReturnsAsync(expected);

            var actual = await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypes_Exception()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesAsync(false)).Throws<Exception>();
            await financialDocumentTypesController.GetFinancialDocumentTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuidAsync_Exception()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(),It.IsAny<bool>())).Throws<Exception>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_KeyNotFoundException()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_PermissionsException()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_ArgumentException()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_RepositoryException()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_IntegrationApiException()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_GetFinancialDocumentTypesByGuid_Exception()
        {
            financialDocumentTypesServiceMock.Setup(x => x.GetFinancialDocumentTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await financialDocumentTypesController.GetFinancialDocumentTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_PostFinancialDocumentTypesAsync_Exception()
        {
            await financialDocumentTypesController.PostFinancialDocumentTypesAsync(financialDocumentTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_PutFinancialDocumentTypesAsync_Exception()
        {
            var sourceContext = financialDocumentTypesCollection.FirstOrDefault();
            await financialDocumentTypesController.PutFinancialDocumentTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialDocumentTypesController_DeleteFinancialDocumentTypesAsync_Exception()
        {
            await financialDocumentTypesController.DeleteFinancialDocumentTypesAsync(financialDocumentTypesCollection.FirstOrDefault().Id);
        }
    }
}