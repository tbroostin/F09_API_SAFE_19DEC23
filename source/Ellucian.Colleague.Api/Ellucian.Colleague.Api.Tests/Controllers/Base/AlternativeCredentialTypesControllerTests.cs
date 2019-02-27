//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AlternativeCredentialTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAlternativeCredentialTypesService> alternativeCredentialTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AlternativeCredentialTypesController alternativeCredentialTypesController;      
        private IEnumerable<Domain.Base.Entities.AltIdTypes> allAltIdTypes;
        private List<Dtos.AlternativeCredentialTypes> alternativeCredentialTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            alternativeCredentialTypesServiceMock = new Mock<IAlternativeCredentialTypesService>();
            loggerMock = new Mock<ILogger>();
            alternativeCredentialTypesCollection = new List<Dtos.AlternativeCredentialTypes>();

            allAltIdTypes  = new List<Domain.Base.Entities.AltIdTypes>()
                {
                    new Domain.Base.Entities.AltIdTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.AltIdTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.AltIdTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allAltIdTypes)
            {
                var alternativeCredentialTypes = new Ellucian.Colleague.Dtos.AlternativeCredentialTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                alternativeCredentialTypesCollection.Add(alternativeCredentialTypes);
            }

            alternativeCredentialTypesController = new AlternativeCredentialTypesController(alternativeCredentialTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            alternativeCredentialTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            alternativeCredentialTypesController = null;
            allAltIdTypes = null;
            alternativeCredentialTypesCollection = null;
            loggerMock = null;
            alternativeCredentialTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_ValidateFields_Nocache()
        {
            alternativeCredentialTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false)).ReturnsAsync(alternativeCredentialTypesCollection);
       
            var sourceContexts = (await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync()).ToList();
            Assert.AreEqual(alternativeCredentialTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = alternativeCredentialTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_ValidateFields_Cache()
        {
            alternativeCredentialTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(true)).ReturnsAsync(alternativeCredentialTypesCollection);

            var sourceContexts = (await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync()).ToList();
            Assert.AreEqual(alternativeCredentialTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = alternativeCredentialTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_NoRecords()
        {
            alternativeCredentialTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(true)).ReturnsAsync(new List<Dtos.AlternativeCredentialTypes>());

            var sourceContexts = (await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync()).ToList();
            Assert.AreEqual(0, sourceContexts.Count());
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_KeyNotFoundException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_PermissionsException()
        {            
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false))
                .Throws<PermissionsException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_ArgumentException()
        {            
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false))
                .Throws<ArgumentException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_RepositoryException()
        {            
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false))
                .Throws<RepositoryException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_IntegrationApiException()
        {            
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false))
                .Throws<IntegrationApiException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();
        }

        [TestMethod]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuidAsync_ValidateFields()
        {
            alternativeCredentialTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = alternativeCredentialTypesCollection.FirstOrDefault();
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypes_Exception()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesAsync(false)).Throws<Exception>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuidAsync_Exception()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_KeyNotFoundException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_PermissionsException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_ArgumentException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_RepositoryException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_IntegrationApiException()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_GetAlternativeCredentialTypesByGuid_Exception()
        {
            alternativeCredentialTypesServiceMock.Setup(x => x.GetAlternativeCredentialTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await alternativeCredentialTypesController.GetAlternativeCredentialTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_PostAlternativeCredentialTypesAsync_Exception()
        {
            await alternativeCredentialTypesController.PostAlternativeCredentialTypesAsync(alternativeCredentialTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_PutAlternativeCredentialTypesAsync_Exception()
        {
            var sourceContext = alternativeCredentialTypesCollection.FirstOrDefault();
            await alternativeCredentialTypesController.PutAlternativeCredentialTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AlternativeCredentialTypesController_DeleteAlternativeCredentialTypesAsync_Exception()
        {
            await alternativeCredentialTypesController.DeleteAlternativeCredentialTypesAsync(alternativeCredentialTypesCollection.FirstOrDefault().Id);
        }
    }
}