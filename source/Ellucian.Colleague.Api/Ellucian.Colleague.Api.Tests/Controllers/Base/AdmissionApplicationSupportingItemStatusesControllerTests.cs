//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AdmissionApplicationSupportingItemStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionApplicationSupportingItemStatusesService> admissionApplicationSupportingItemStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionApplicationSupportingItemStatusesController admissionApplicationSupportingItemStatusesController;
        private IEnumerable<Domain.Base.Entities.CorrStatus> allCorrStatuses;
        private List<Dtos.AdmissionApplicationSupportingItemStatus> admissionApplicationSupportingItemStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionApplicationSupportingItemStatusesServiceMock = new Mock<IAdmissionApplicationSupportingItemStatusesService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationSupportingItemStatusesCollection = new List<Dtos.AdmissionApplicationSupportingItemStatus>();

            allCorrStatuses = new List<Domain.Base.Entities.CorrStatus>()
                {
                    new CorrStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new CorrStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new CorrStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allCorrStatuses)
            {
                var admissionApplicationSupportingItemStatuses = new Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationSupportingItemStatusesCollection.Add(admissionApplicationSupportingItemStatuses);
            }

            admissionApplicationSupportingItemStatusesController = new AdmissionApplicationSupportingItemStatusesController(admissionApplicationSupportingItemStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionApplicationSupportingItemStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionApplicationSupportingItemStatusesController = null;
            allCorrStatuses = null;
            admissionApplicationSupportingItemStatusesCollection = null;
            loggerMock = null;
            admissionApplicationSupportingItemStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_ValidateFields_Nocache()
        {
            admissionApplicationSupportingItemStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false)).ReturnsAsync(admissionApplicationSupportingItemStatusesCollection);

            var sourceContexts = (await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSupportingItemStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSupportingItemStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_ValidateFields_Cache()
        {
            admissionApplicationSupportingItemStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(true)).ReturnsAsync(admissionApplicationSupportingItemStatusesCollection);

            var sourceContexts = (await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync()).ToList();
            Assert.AreEqual(admissionApplicationSupportingItemStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationSupportingItemStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_KeyNotFoundException()
        {
            //
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_PermissionsException()
        {

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false))
                .Throws<PermissionsException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_ArgumentException()
        {

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false))
                .Throws<ArgumentException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_RepositoryException()
        {

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false))
                .Throws<RepositoryException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_IntegrationApiException()
        {

            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationSupportingItemStatusesCollection.FirstOrDefault();
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatuses_Exception()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusesAsync(false)).Throws<Exception>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuidAsync_Exception()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_KeyNotFoundException()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_PermissionsException()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_ArgumentException()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_RepositoryException()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_IntegrationApiException()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_GetAdmissionApplicationSupportingItemStatusesByGuid_Exception()
        {
            admissionApplicationSupportingItemStatusesServiceMock.Setup(x => x.GetAdmissionApplicationSupportingItemStatusByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await admissionApplicationSupportingItemStatusesController.GetAdmissionApplicationSupportingItemStatusByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_PostAdmissionApplicationSupportingItemStatusesAsync_Exception()
        {
            await admissionApplicationSupportingItemStatusesController.PostAdmissionApplicationSupportingItemStatusAsync(admissionApplicationSupportingItemStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_PutAdmissionApplicationSupportingItemStatusesAsync_Exception()
        {
            var sourceContext = admissionApplicationSupportingItemStatusesCollection.FirstOrDefault();
            await admissionApplicationSupportingItemStatusesController.PutAdmissionApplicationSupportingItemStatusAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionApplicationSupportingItemStatusesController_DeleteAdmissionApplicationSupportingItemStatusesAsync_Exception()
        {
            await admissionApplicationSupportingItemStatusesController.DeleteAdmissionApplicationSupportingItemStatusAsync(admissionApplicationSupportingItemStatusesCollection.FirstOrDefault().Id);
        }
    }
}