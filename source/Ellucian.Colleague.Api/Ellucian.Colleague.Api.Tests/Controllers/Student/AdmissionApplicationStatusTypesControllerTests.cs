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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionApplicationStatusTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionDecisionTypesService> admissionApplicationStatusTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionApplicationStatusTypesController admissionApplicationStatusTypesController;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType> allApplicationStatuses;
        private List<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType> admissionApplicationStatusTypesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionApplicationStatusTypesServiceMock = new Mock<IAdmissionDecisionTypesService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationStatusTypesCollection = new List<Dtos.AdmissionApplicationStatusType>();


            allApplicationStatuses = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allApplicationStatuses)
            {
                var admissionApplicationStatusTypes = new Ellucian.Colleague.Dtos.AdmissionApplicationStatusType
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationStatusTypesCollection.Add(admissionApplicationStatusTypes);


            }

            admissionApplicationStatusTypesController = new AdmissionApplicationStatusTypesController(admissionApplicationStatusTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionApplicationStatusTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionApplicationStatusTypesController = null;
            allApplicationStatuses = null;
            admissionApplicationStatusTypesCollection = null;
            loggerMock = null;
            admissionApplicationStatusTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_ValidateFields_Nocache()
        {
            admissionApplicationStatusTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(false)).ReturnsAsync(admissionApplicationStatusTypesCollection);

            var sourceContexts = (await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync()).ToList();
            Assert.AreEqual(admissionApplicationStatusTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationStatusTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_ValidateFields_Cache()
        {
            admissionApplicationStatusTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(true)).ReturnsAsync(admissionApplicationStatusTypesCollection);

            var sourceContexts = (await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync()).ToList();
            Assert.AreEqual(admissionApplicationStatusTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationStatusTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationStatusTypesCollection.FirstOrDefault();
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_KeyNotFoundException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_PermissionsException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_ArgumentException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).Throws<ArgumentException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_RepositoryException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_IntegrationApiException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypes_Exception()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesAsync(false)).Throws<Exception>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_KeyNotFoundException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_PermissionsException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_ArgumentException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_RepositoryException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_IntegrationApiException()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_NoId_Exception()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_GetAdmissionApplicationStatusTypesByGuidAsync_Exception()
        {
            admissionApplicationStatusTypesServiceMock.Setup(x => x.GetAdmissionApplicationStatusTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationStatusTypesController.GetAdmissionApplicationStatusTypesByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_PostAdmissionApplicationStatusTypesAsync_Exception()
        {
            await admissionApplicationStatusTypesController.PostAdmissionApplicationStatusTypesAsync(admissionApplicationStatusTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationStatusTypesController_PutAdmissionApplicationStatusTypesAsync_Exception()
        {
            var sourceContext = admissionApplicationStatusTypesCollection.FirstOrDefault();
            await admissionApplicationStatusTypesController.PutAdmissionApplicationStatusTypesAsync(sourceContext.Id, sourceContext);
        }
    }
}