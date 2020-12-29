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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationSourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionApplicationSourcesService> admissionApplicationSourcesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionApplicationSourcesController admissionApplicationSourcesController;
        private IEnumerable<Domain.Student.Entities.ApplicationSource> allApplicationSources;
        private List<Dtos.AdmissionApplicationSources> admissionApplicationSourcesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionApplicationSourcesServiceMock = new Mock<IAdmissionApplicationSourcesService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationSourcesCollection = new List<Dtos.AdmissionApplicationSources>();

            allApplicationSources = new List<Domain.Student.Entities.ApplicationSource>()
                {
                    new Domain.Student.Entities.ApplicationSource("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.ApplicationSource("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.ApplicationSource("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allApplicationSources)
            {
                var admissionApplicationSources = new Ellucian.Colleague.Dtos.AdmissionApplicationSources
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationSourcesCollection.Add(admissionApplicationSources);
            }

            admissionApplicationSourcesController = new AdmissionApplicationSourcesController(admissionApplicationSourcesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionApplicationSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionApplicationSourcesController = null;
            allApplicationSources = null;
            admissionApplicationSourcesCollection = null;
            loggerMock = null;
            admissionApplicationSourcesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_ValidateFields_Nocache()
        {
            admissionApplicationSourcesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false)).ReturnsAsync(admissionApplicationSourcesCollection);

            var sourceContexts = (await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_ValidateFields_Cache()
        {
            admissionApplicationSourcesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(true)).ReturnsAsync(admissionApplicationSourcesCollection);

            var sourceContexts = (await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_KeyNotFoundException()
        {
            //
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false))
                .Throws<KeyNotFoundException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_PermissionsException()
        {

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false))
                .Throws<PermissionsException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_ArgumentException()
        {

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false))
                .Throws<ArgumentException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_RepositoryException()
        {

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false))
                .Throws<RepositoryException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_IntegrationApiException()
        {

            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false))
                .Throws<IntegrationApiException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationSourcesCollection.FirstOrDefault();
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSources_Exception()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesAsync(false)).Throws<Exception>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuidAsync_Exception()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_KeyNotFoundException()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_PermissionsException()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_ArgumentException()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_RepositoryException()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_IntegrationApiException()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_GetAdmissionApplicationSourcesByGuid_Exception()
        {
            admissionApplicationSourcesServiceMock.Setup(x => x.GetAdmissionApplicationSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await admissionApplicationSourcesController.GetAdmissionApplicationSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_PostAdmissionApplicationSourcesAsync_Exception()
        {
            await admissionApplicationSourcesController.PostAdmissionApplicationSourcesAsync(admissionApplicationSourcesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_PutAdmissionApplicationSourcesAsync_Exception()
        {
            var sourceContext = admissionApplicationSourcesCollection.FirstOrDefault();
            await admissionApplicationSourcesController.PutAdmissionApplicationSourcesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSourcesController_DeleteAdmissionApplicationSourcesAsync_Exception()
        {
            await admissionApplicationSourcesController.DeleteAdmissionApplicationSourcesAsync(admissionApplicationSourcesCollection.FirstOrDefault().Id);
        }
    }
}