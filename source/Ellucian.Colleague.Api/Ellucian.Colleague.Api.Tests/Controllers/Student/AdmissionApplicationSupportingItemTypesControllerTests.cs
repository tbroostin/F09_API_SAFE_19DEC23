//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationSupportingItemTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionApplicationSupportingItemTypesService> admissionApplicationSupportingItemTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionApplicationSupportingItemTypesController admissionApplicationSupportingItemTypesController;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode> allSupportingItemTypes;
        private List<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes> admissionApplicationSupportingItemTypesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionApplicationSupportingItemTypesServiceMock = new Mock<IAdmissionApplicationSupportingItemTypesService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationSupportingItemTypesCollection = new List<Dtos.AdmissionApplicationSupportingItemTypes>();

            allSupportingItemTypes = new List<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode>()
                {
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allSupportingItemTypes)
            {
                var admissionApplicationSupportingItemTypes = new Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationSupportingItemTypesCollection.Add(admissionApplicationSupportingItemTypes);
            }
            
            admissionApplicationSupportingItemTypesController = new AdmissionApplicationSupportingItemTypesController(admissionApplicationSupportingItemTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionApplicationSupportingItemTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionApplicationSupportingItemTypesController = null;
            allSupportingItemTypes = null;
            admissionApplicationSupportingItemTypesCollection = null;
            loggerMock = null;
            admissionApplicationSupportingItemTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_ValidateFields_Nocache()
        {
            admissionApplicationSupportingItemTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(false)).ReturnsAsync(admissionApplicationSupportingItemTypesCollection);

            var sourceContexts = (await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSupportingItemTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSupportingItemTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_ValidateFields_Cache()
        {
            admissionApplicationSupportingItemTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(true)).ReturnsAsync(admissionApplicationSupportingItemTypesCollection);

            var sourceContexts = (await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSupportingItemTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSupportingItemTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationSupportingItemTypesCollection.FirstOrDefault();
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_KeyNotFoundException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_PermissionsException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_ArgumentException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).Throws<ArgumentException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_RepositoryException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_IntegrationApiException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypes_Exception()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesAsync(false)).Throws<Exception>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_KeyNotFoundException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_PermissionsException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_ArgumentException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_RepositoryException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_IntegrationApiException()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_NoId_Exception()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_GetAdmissionApplicationSupportingItemTypesByGuidAsync_Exception()
        {
            admissionApplicationSupportingItemTypesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationSupportingItemTypesController.GetAdmissionApplicationSupportingItemTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_PostAdmissionApplicationSupportingItemTypesAsync_Exception()
        {
            await admissionApplicationSupportingItemTypesController.PostAdmissionApplicationSupportingItemTypesAsync(admissionApplicationSupportingItemTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemTypesController_PutAdmissionApplicationSupportingItemTypesAsync_Exception()
        {
            var sourceContext = admissionApplicationSupportingItemTypesCollection.FirstOrDefault();
            await admissionApplicationSupportingItemTypesController.PutAdmissionApplicationSupportingItemTypesAsync(sourceContext.Id, sourceContext);
        }       
    }
}