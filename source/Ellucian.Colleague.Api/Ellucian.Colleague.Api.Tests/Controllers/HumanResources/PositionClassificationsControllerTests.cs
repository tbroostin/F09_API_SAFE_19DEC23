//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResoures;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResoures.Services;
using Ellucian.Colleague.Domain.Exceptions;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResoures
{
    [TestClass]
    public class PositionClassificationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPositionClassificationsService> positionClassificationsServiceMock;
        private Mock<ILogger> loggerMock;
        private PositionClassificationsController positionClassificationsController;
        private IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> allClassifications;
        private List<Dtos.PositionClassification> positionClassificationsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            positionClassificationsServiceMock = new Mock<IPositionClassificationsService>();
            loggerMock = new Mock<ILogger>();
            positionClassificationsCollection = new List<Dtos.PositionClassification>();

            allClassifications = new List<Domain.HumanResources.Entities.EmploymentClassification>()
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", Domain.HumanResources.Entities.EmploymentClassificationType.Position)
                };

            foreach (var source in allClassifications)
            {
                var positionClassifications = new Ellucian.Colleague.Dtos.PositionClassification
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                positionClassificationsCollection.Add(positionClassifications);
            }

            positionClassificationsController = new PositionClassificationsController(positionClassificationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            positionClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            positionClassificationsController = null;
            allClassifications = null;
            positionClassificationsCollection = null;
            loggerMock = null;
            positionClassificationsServiceMock = null;
        }

        [TestMethod]
        public async Task PositionClassificationsController_GetPositionClassifications_ValidateFields_Nocache()
        {
            positionClassificationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false)).ReturnsAsync(positionClassificationsCollection);

            var sourceContexts = (await positionClassificationsController.GetPositionClassificationsAsync()).ToList();
            Assert.AreEqual(positionClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = positionClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PositionClassificationsController_GetPositionClassifications_ValidateFields_Cache()
        {
            positionClassificationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(true)).ReturnsAsync(positionClassificationsCollection);

            var sourceContexts = (await positionClassificationsController.GetPositionClassificationsAsync()).ToList();
            Assert.AreEqual(positionClassificationsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = positionClassificationsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_KeyNotFoundException()
        {
            //
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false))
                .Throws<KeyNotFoundException>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_PermissionsException()
        {

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false))
                .Throws<PermissionsException>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_ArgumentException()
        {

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false))
                .Throws<ArgumentException>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_RepositoryException()
        {

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false))
                .Throws<RepositoryException>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_IntegrationApiException()
        {

            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false))
                .Throws<IntegrationApiException>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuidAsync_ValidateFields()
        {
            var expected = positionClassificationsCollection.FirstOrDefault();
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await positionClassificationsController.GetPositionClassificationsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassifications_Exception()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsAsync(false)).Throws<Exception>();
            await positionClassificationsController.GetPositionClassificationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuidAsync_Exception()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_KeyNotFoundException()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_PermissionsException()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_ArgumentException()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_RepositoryException()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_IntegrationApiException()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_GetPositionClassificationsByGuid_Exception()
        {
            positionClassificationsServiceMock.Setup(x => x.GetPositionClassificationsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await positionClassificationsController.GetPositionClassificationsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_PostPositionClassificationsAsync_Exception()
        {
            await positionClassificationsController.PostPositionClassificationsAsync(positionClassificationsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_PutPositionClassificationsAsync_Exception()
        {
            var sourceContext = positionClassificationsCollection.FirstOrDefault();
            await positionClassificationsController.PutPositionClassificationsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PositionClassificationsController_DeletePositionClassificationsAsync_Exception()
        {
            await positionClassificationsController.DeletePositionClassificationsAsync(positionClassificationsCollection.FirstOrDefault().Id);
        }
    }
}