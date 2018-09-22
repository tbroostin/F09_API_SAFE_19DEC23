//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    public class AdmissionDecisionTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionDecisionTypesService> admissionDecisionTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionDecisionTypesController admissionDecisionTypesController;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType> allDecisionTypes;
        private List<Ellucian.Colleague.Dtos.AdmissionDecisionType2> admissionDecisionTypes2Collection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionDecisionTypesServiceMock = new Mock<IAdmissionDecisionTypesService>();
            loggerMock = new Mock<ILogger>();
            admissionDecisionTypes2Collection = new List<Dtos.AdmissionDecisionType2>();

            allDecisionTypes = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allDecisionTypes)
            {
                
                var admissionDecisionTypes2 = new Ellucian.Colleague.Dtos.AdmissionDecisionType2
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                admissionDecisionTypes2Collection.Add(admissionDecisionTypes2);
            }

            admissionDecisionTypesController = new AdmissionDecisionTypesController(admissionDecisionTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            admissionDecisionTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionDecisionTypesController = null;
            allDecisionTypes = null;
            loggerMock = null;
            admissionDecisionTypesServiceMock = null;
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_ValidateFields_Nocache()
        {
            admissionDecisionTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(false)).ReturnsAsync(admissionDecisionTypes2Collection);

            var sourceContexts = (await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async()).ToList();
            Assert.AreEqual(admissionDecisionTypes2Collection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionDecisionTypes2Collection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_ValidateFields_Cache()
        {
            admissionDecisionTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(true)).ReturnsAsync(admissionDecisionTypes2Collection);

            var sourceContexts = (await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async()).ToList();
            Assert.AreEqual(admissionDecisionTypes2Collection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = admissionDecisionTypes2Collection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuidAsync_ValidateFields()
        {
            admissionDecisionTypesController.Request.Headers.CacheControl = 
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = admissionDecisionTypes2Collection.FirstOrDefault();
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes()
        {
            var result = await admissionDecisionTypesController.GetAdmissionDecisionTypesAsync();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_KeyNotFoundException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_PermissionsException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_ArgumentException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).Throws<ArgumentException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_RepositoryException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypes_IntegrationApiException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionType2s_Exception()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesAsync(false)).Throws<Exception>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypes2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_NullId()
        {
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_KeyNotFoundException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_PermissionsException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_ArgumentException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_RepositoryException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_IntegrationApiException()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuid2Async_Exception()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuid2Async("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuidAsync_NoId_Exception()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_GetAdmissionDecisionTypeByGuidAsync_Exception()
        {
            admissionDecisionTypesServiceMock.Setup(x => x.GetAdmissionDecisionTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await admissionDecisionTypesController.GetAdmissionDecisionTypeByGuidAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_Post()
        {
            await admissionDecisionTypesController.PostAdmissionDecisionTypesAsync(It.IsAny< Dtos.AdmissionDecisionType2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_Put()
        {
            await admissionDecisionTypesController.PutAdmissionDecisionTypesAsync(It.IsAny<string>(), It.IsAny<Dtos.AdmissionDecisionType2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionTypesController_Delete()
        {
            await admissionDecisionTypesController.DeleteAdmissionDecisionTypesAsync(It.IsAny<string>());
        }
    }
}