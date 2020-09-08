//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AptitudeAssessmentSourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAptitudeAssessmentSourcesService> aptitudeAssessmentSourcesServiceMock;
        private Mock<ILogger> loggerMock;
        private AptitudeAssessmentSourcesController aptitudeAssessmentSourcesController;
        private IEnumerable<Domain.Student.Entities.TestSource> allApplTestSources;
        private List<Dtos.AptitudeAssessmentSources> aptitudeAssessmentSourcesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            aptitudeAssessmentSourcesServiceMock = new Mock<IAptitudeAssessmentSourcesService>();
            loggerMock = new Mock<ILogger>();
            aptitudeAssessmentSourcesCollection = new List<Dtos.AptitudeAssessmentSources>();

            allApplTestSources = new List<Domain.Student.Entities.TestSource>()
                {
                    new Domain.Student.Entities.TestSource("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.TestSource("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.TestSource("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allApplTestSources)
            {
                var aptitudeAssessmentSources = new Ellucian.Colleague.Dtos.AptitudeAssessmentSources
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                aptitudeAssessmentSourcesCollection.Add(aptitudeAssessmentSources);
            }

            aptitudeAssessmentSourcesController = new AptitudeAssessmentSourcesController(aptitudeAssessmentSourcesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            aptitudeAssessmentSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            aptitudeAssessmentSourcesController = null;
            allApplTestSources = null;
            aptitudeAssessmentSourcesCollection = null;
            loggerMock = null;
            aptitudeAssessmentSourcesServiceMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_ValidateFields_Nocache()
        {
            aptitudeAssessmentSourcesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false)).ReturnsAsync(aptitudeAssessmentSourcesCollection);

            var sourceContexts = (await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_ValidateFields_Cache()
        {
            aptitudeAssessmentSourcesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(true)).ReturnsAsync(aptitudeAssessmentSourcesCollection);

            var sourceContexts = (await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync()).ToList();
            Assert.AreEqual(aptitudeAssessmentSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = aptitudeAssessmentSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_KeyNotFoundException()
        {
            //
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false))
                .Throws<KeyNotFoundException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_PermissionsException()
        {

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false))
                .Throws<PermissionsException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_ArgumentException()
        {

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false))
                .Throws<ArgumentException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_RepositoryException()
        {

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false))
                .Throws<RepositoryException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_IntegrationApiException()
        {

            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false))
                .Throws<IntegrationApiException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuidAsync_ValidateFields()
        {
            var expected = aptitudeAssessmentSourcesCollection.FirstOrDefault();
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSources_Exception()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesAsync(false)).Throws<Exception>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuidAsync_Exception()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_KeyNotFoundException()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_PermissionsException()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_ArgumentException()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_RepositoryException()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_IntegrationApiException()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_GetAptitudeAssessmentSourcesByGuid_Exception()
        {
            aptitudeAssessmentSourcesServiceMock.Setup(x => x.GetAptitudeAssessmentSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await aptitudeAssessmentSourcesController.GetAptitudeAssessmentSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_PostAptitudeAssessmentSourcesAsync_Exception()
        {
            await aptitudeAssessmentSourcesController.PostAptitudeAssessmentSourcesAsync(aptitudeAssessmentSourcesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_PutAptitudeAssessmentSourcesAsync_Exception()
        {
            var sourceContext = aptitudeAssessmentSourcesCollection.FirstOrDefault();
            await aptitudeAssessmentSourcesController.PutAptitudeAssessmentSourcesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AptitudeAssessmentSourcesController_DeleteAptitudeAssessmentSourcesAsync_Exception()
        {
            await aptitudeAssessmentSourcesController.DeleteAptitudeAssessmentSourcesAsync(aptitudeAssessmentSourcesCollection.FirstOrDefault().Id);
        }
    }
}