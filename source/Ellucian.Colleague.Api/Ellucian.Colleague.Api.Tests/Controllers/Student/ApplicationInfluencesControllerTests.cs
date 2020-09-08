
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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ApplicationInfluencesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionApplicationInfluencesService> admissionApplicationInfluencesServiceMock;
        private Mock<ILogger> loggerMock;

        private ApplicationInfluencesController ApplicationInfluencesController;
        private IEnumerable<Domain.Student.Entities.ApplicationInfluence> allApplInfluences;
        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;


        private Mock<IAdapterRegistry> adapterRegistryMock;

        private List<Dtos.AdmissionApplicationInfluences> admissionApplicationInfluencesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            admissionApplicationInfluencesServiceMock = new Mock<IAdmissionApplicationInfluencesService>();
            loggerMock = new Mock<ILogger>();
            admissionApplicationInfluencesCollection = new List<Dtos.AdmissionApplicationInfluences>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            allApplInfluences = new List<Domain.Student.Entities.ApplicationInfluence>()
                {
                    new Domain.Student.Entities.ApplicationInfluence("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.ApplicationInfluence("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.ApplicationInfluence("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allApplInfluences)
            {
                var admissionApplicationInfluences = new Ellucian.Colleague.Dtos.AdmissionApplicationInfluences
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionApplicationInfluencesCollection.Add(admissionApplicationInfluences);
            }

            ApplicationInfluencesController = new ApplicationInfluencesController(adapterRegistryMock.Object, studentReferenceDataRepositoryMock.Object, 
                    admissionApplicationInfluencesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            ApplicationInfluencesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            ApplicationInfluencesController = null;
            allApplInfluences = null;
            admissionApplicationInfluencesCollection = null;
            loggerMock = null;
            admissionApplicationInfluencesServiceMock = null;
        }

        [TestMethod]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_ValidateFields_Nocache()
        {
            ApplicationInfluencesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false)).ReturnsAsync(admissionApplicationInfluencesCollection);

            var sourceContexts = (await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync()).ToList();
            Assert.AreEqual(admissionApplicationInfluencesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationInfluencesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_ValidateFields_Cache()
        {
            ApplicationInfluencesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(true)).ReturnsAsync(admissionApplicationInfluencesCollection);

            var sourceContexts = (await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync()).ToList();
            Assert.AreEqual(admissionApplicationInfluencesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionApplicationInfluencesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_KeyNotFoundException()
        {
            //
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false))
                .Throws<KeyNotFoundException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_PermissionsException()
        {

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false))
                .Throws<PermissionsException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_ArgumentException()
        {

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false))
                .Throws<ArgumentException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_RepositoryException()
        {

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false))
                .Throws<RepositoryException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_IntegrationApiException()
        {

            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false))
                .Throws<IntegrationApiException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuidAsync_ValidateFields()
        {
            var expected = admissionApplicationInfluencesCollection.FirstOrDefault();
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluences_Exception()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesAsync(false)).Throws<Exception>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuidAsync_Exception()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_KeyNotFoundException()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_PermissionsException()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_ArgumentException()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_RepositoryException()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_IntegrationApiException()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_GetAdmissionApplicationInfluencesByGuid_Exception()
        {
            admissionApplicationInfluencesServiceMock.Setup(x => x.GetAdmissionApplicationInfluencesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await ApplicationInfluencesController.GetAdmissionApplicationInfluencesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_PostAdmissionApplicationInfluencesAsync_Exception()
        {
            await ApplicationInfluencesController.PostAdmissionApplicationInfluencesAsync(admissionApplicationInfluencesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_PutAdmissionApplicationInfluencesAsync_Exception()
        {
            var sourceContext = admissionApplicationInfluencesCollection.FirstOrDefault();
            await ApplicationInfluencesController.PutAdmissionApplicationInfluencesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ApplicationInfluencesController_DeleteAdmissionApplicationInfluencesAsync_Exception()
        {
            await ApplicationInfluencesController.DeleteAdmissionApplicationInfluencesAsync(admissionApplicationInfluencesCollection.FirstOrDefault().Id);
        }
    }
}