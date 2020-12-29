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
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class FixedAssetDesignationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFixedAssetDesignationsService> fixedAssetDesignationsServiceMock;
        private Mock<ILogger> loggerMock;
        private FixedAssetDesignationsController fixedAssetDesignationsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.FxaTransferFlags> allFxaTransferFlags;
        private List<Dtos.FixedAssetDesignations> fixedAssetDesignationsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            fixedAssetDesignationsServiceMock = new Mock<IFixedAssetDesignationsService>();
            loggerMock = new Mock<ILogger>();
            fixedAssetDesignationsCollection = new List<Dtos.FixedAssetDesignations>();

            allFxaTransferFlags = new List<Domain.ColleagueFinance.Entities.FxaTransferFlags>()
                {
                    new Domain.ColleagueFinance.Entities.FxaTransferFlags("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.FxaTransferFlags("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.FxaTransferFlags("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allFxaTransferFlags)
            {
                var fixedAssetDesignations = new Ellucian.Colleague.Dtos.FixedAssetDesignations
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                fixedAssetDesignationsCollection.Add(fixedAssetDesignations);
            }

            fixedAssetDesignationsController = new FixedAssetDesignationsController(fixedAssetDesignationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            fixedAssetDesignationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            fixedAssetDesignationsController = null;
            allFxaTransferFlags = null;
            fixedAssetDesignationsCollection = null;
            loggerMock = null;
            fixedAssetDesignationsServiceMock = null;
        }

        [TestMethod]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_ValidateFields_Nocache()
        {
            fixedAssetDesignationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false)).ReturnsAsync(fixedAssetDesignationsCollection);

            var sourceContexts = (await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync()).ToList();
            Assert.AreEqual(fixedAssetDesignationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetDesignationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_ValidateFields_Cache()
        {
            fixedAssetDesignationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(true)).ReturnsAsync(fixedAssetDesignationsCollection);

            var sourceContexts = (await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync()).ToList();
            Assert.AreEqual(fixedAssetDesignationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = fixedAssetDesignationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_KeyNotFoundException()
        {
            //
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false))
                .Throws<KeyNotFoundException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_PermissionsException()
        {

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false))
                .Throws<PermissionsException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_ArgumentException()
        {

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false))
                .Throws<ArgumentException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_RepositoryException()
        {

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false))
                .Throws<RepositoryException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_IntegrationApiException()
        {

            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false))
                .Throws<IntegrationApiException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuidAsync_ValidateFields()
        {
            var expected = fixedAssetDesignationsCollection.FirstOrDefault();
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignations_Exception()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsAsync(false)).Throws<Exception>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuidAsync_Exception()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_KeyNotFoundException()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_PermissionsException()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_ArgumentException()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_RepositoryException()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_IntegrationApiException()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_GetFixedAssetDesignationsByGuid_Exception()
        {
            fixedAssetDesignationsServiceMock.Setup(x => x.GetFixedAssetDesignationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await fixedAssetDesignationsController.GetFixedAssetDesignationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_PostFixedAssetDesignationsAsync_Exception()
        {
            await fixedAssetDesignationsController.PostFixedAssetDesignationsAsync(fixedAssetDesignationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_PutFixedAssetDesignationsAsync_Exception()
        {
            var sourceContext = fixedAssetDesignationsCollection.FirstOrDefault();
            await fixedAssetDesignationsController.PutFixedAssetDesignationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FixedAssetDesignationsController_DeleteFixedAssetDesignationsAsync_Exception()
        {
            await fixedAssetDesignationsController.DeleteFixedAssetDesignationsAsync(fixedAssetDesignationsCollection.FirstOrDefault().Id);
        }
    }
}