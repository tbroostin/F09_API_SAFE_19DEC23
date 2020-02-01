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

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonSourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonSourcesService> personSourcesServiceMock;
        private Mock<ILogger> loggerMock;
        private PersonSourcesController personSourcesController;
        private IEnumerable<Domain.Base.Entities.PersonOriginCodes> allPersonOriginCodes;
        private List<Dtos.PersonSources> personSourcesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            personSourcesServiceMock = new Mock<IPersonSourcesService>();
            loggerMock = new Mock<ILogger>();
            personSourcesCollection = new List<Dtos.PersonSources>();

            allPersonOriginCodes = new List<Domain.Base.Entities.PersonOriginCodes>()
                {
                    new Domain.Base.Entities.PersonOriginCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.PersonOriginCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.PersonOriginCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allPersonOriginCodes)
            {
                var personSources = new Ellucian.Colleague.Dtos.PersonSources
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                personSourcesCollection.Add(personSources);
            }

            personSourcesController = new PersonSourcesController(personSourcesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            personSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            personSourcesController = null;
            allPersonOriginCodes = null;
            personSourcesCollection = null;
            loggerMock = null;
            personSourcesServiceMock = null;
        }

        [TestMethod]
        public async Task PersonSourcesController_GetPersonSources_ValidateFields_Nocache()
        {
            personSourcesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false)).ReturnsAsync(personSourcesCollection);

            var sourceContexts = (await personSourcesController.GetPersonSourcesAsync()).ToList();
            Assert.AreEqual(personSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = personSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PersonSourcesController_GetPersonSources_ValidateFields_Cache()
        {
            personSourcesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(true)).ReturnsAsync(personSourcesCollection);

            var sourceContexts = (await personSourcesController.GetPersonSourcesAsync()).ToList();
            Assert.AreEqual(personSourcesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = personSourcesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_KeyNotFoundException()
        {
            //
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false))
                .Throws<KeyNotFoundException>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_PermissionsException()
        {

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false))
                .Throws<PermissionsException>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_ArgumentException()
        {

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false))
                .Throws<ArgumentException>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_RepositoryException()
        {

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false))
                .Throws<RepositoryException>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_IntegrationApiException()
        {

            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false))
                .Throws<IntegrationApiException>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        public async Task PersonSourcesController_GetPersonSourcesByGuidAsync_ValidateFields()
        {
            var expected = personSourcesCollection.FirstOrDefault();
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await personSourcesController.GetPersonSourcesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSources_Exception()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesAsync(false)).Throws<Exception>();
            await personSourcesController.GetPersonSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuidAsync_Exception()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await personSourcesController.GetPersonSourcesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_KeyNotFoundException()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_PermissionsException()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_ArgumentException()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_RepositoryException()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_IntegrationApiException()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_GetPersonSourcesByGuid_Exception()
        {
            personSourcesServiceMock.Setup(x => x.GetPersonSourcesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await personSourcesController.GetPersonSourcesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_PostPersonSourcesAsync_Exception()
        {
            await personSourcesController.PostPersonSourcesAsync(personSourcesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_PutPersonSourcesAsync_Exception()
        {
            var sourceContext = personSourcesCollection.FirstOrDefault();
            await personSourcesController.PutPersonSourcesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonSourcesController_DeletePersonSourcesAsync_Exception()
        {
            await personSourcesController.DeletePersonSourcesAsync(personSourcesCollection.FirstOrDefault().Id);
        }
    }
}