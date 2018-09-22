//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ExternalEmploymentPositionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IExternalEmploymentPositionsService> externalEmploymentPositionsServiceMock;
        private Mock<ILogger> loggerMock;
        private ExternalEmploymentPositionsController externalEmploymentPositionsController;
        private IEnumerable<Domain.Base.Entities.Positions> allPositions;
        private List<Dtos.ExternalEmploymentPositions> externalEmploymentPositionsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            externalEmploymentPositionsServiceMock = new Mock<IExternalEmploymentPositionsService>();
            loggerMock = new Mock<ILogger>();
            externalEmploymentPositionsCollection = new List<Dtos.ExternalEmploymentPositions>();

            allPositions = new List<Domain.Base.Entities.Positions>()
                {
                    new Domain.Base.Entities.Positions("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.Positions("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.Positions("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allPositions)
            {
                var externalEmploymentPositions = new Ellucian.Colleague.Dtos.ExternalEmploymentPositions
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                externalEmploymentPositionsCollection.Add(externalEmploymentPositions);
            }

            externalEmploymentPositionsController = new ExternalEmploymentPositionsController(externalEmploymentPositionsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            externalEmploymentPositionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            externalEmploymentPositionsController = null;
            allPositions = null;
            externalEmploymentPositionsCollection = null;
            loggerMock = null;
            externalEmploymentPositionsServiceMock = null;
        }

        [TestMethod]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_ValidateFields_Nocache()
        {
            externalEmploymentPositionsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false)).ReturnsAsync(externalEmploymentPositionsCollection);

            var sourceContexts = (await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync()).ToList();
            Assert.AreEqual(externalEmploymentPositionsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = externalEmploymentPositionsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_ValidateFields_Cache()
        {
            externalEmploymentPositionsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(true)).ReturnsAsync(externalEmploymentPositionsCollection);

            var sourceContexts = (await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync()).ToList();
            Assert.AreEqual(externalEmploymentPositionsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = externalEmploymentPositionsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_KeyNotFoundException()
        {
            //
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false))
                .Throws<KeyNotFoundException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_PermissionsException()
        {

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false))
                .Throws<PermissionsException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_ArgumentException()
        {

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false))
                .Throws<ArgumentException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_RepositoryException()
        {

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false))
                .Throws<RepositoryException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_IntegrationApiException()
        {

            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false))
                .Throws<IntegrationApiException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuidAsync_ValidateFields()
        {
            var expected = externalEmploymentPositionsCollection.FirstOrDefault();
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositions_Exception()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsAsync(false)).Throws<Exception>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuidAsync_Exception()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_KeyNotFoundException()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_PermissionsException()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_ArgumentException()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_RepositoryException()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_IntegrationApiException()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_GetExternalEmploymentPositionsByGuid_Exception()
        {
            externalEmploymentPositionsServiceMock.Setup(x => x.GetExternalEmploymentPositionsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await externalEmploymentPositionsController.GetExternalEmploymentPositionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_PostExternalEmploymentPositionsAsync_Exception()
        {
            await externalEmploymentPositionsController.PostExternalEmploymentPositionsAsync(externalEmploymentPositionsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_PutExternalEmploymentPositionsAsync_Exception()
        {
            var sourceContext = externalEmploymentPositionsCollection.FirstOrDefault();
            await externalEmploymentPositionsController.PutExternalEmploymentPositionsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ExternalEmploymentPositionsController_DeleteExternalEmploymentPositionsAsync_Exception()
        {
            await externalEmploymentPositionsController.DeleteExternalEmploymentPositionsAsync(externalEmploymentPositionsCollection.FirstOrDefault().Id);
        }
    }
}