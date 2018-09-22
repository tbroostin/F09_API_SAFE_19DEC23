//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class AdministrativeInstructionalMethodsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdministrativeInstructionalMethodsService> administrativeInstructionalMethodsServiceMock;
        private Mock<ILogger> loggerMock;
        private AdministrativeInstructionalMethodsController administrativeInstructionalMethodsController;      
        private IEnumerable<Domain.Student.Entities.SapType> allSaptype;
        private List<Dtos.AdministrativeInstructionalMethods> administrativeInstructionalMethodsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            administrativeInstructionalMethodsServiceMock = new Mock<IAdministrativeInstructionalMethodsService>();
            loggerMock = new Mock<ILogger>();
            administrativeInstructionalMethodsCollection = new List<Dtos.AdministrativeInstructionalMethods>();

            allSaptype  = new List<Domain.Student.Entities.SapType>()
                {
                    new Domain.Student.Entities.SapType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SapType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SapType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allSaptype)
            {
                var administrativeInstructionalMethods = new Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                administrativeInstructionalMethodsCollection.Add(administrativeInstructionalMethods);
            }

            administrativeInstructionalMethodsController = new AdministrativeInstructionalMethodsController(administrativeInstructionalMethodsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            administrativeInstructionalMethodsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            administrativeInstructionalMethodsController = null;
            allSaptype = null;
            administrativeInstructionalMethodsCollection = null;
            loggerMock = null;
            administrativeInstructionalMethodsServiceMock = null;
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_ValidateFields_Nocache()
        {
            administrativeInstructionalMethodsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false)).ReturnsAsync(administrativeInstructionalMethodsCollection);
       
            var sourceContexts = (await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync()).ToList();
            Assert.AreEqual(administrativeInstructionalMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = administrativeInstructionalMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_ValidateFields_Cache()
        {
            administrativeInstructionalMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(true)).ReturnsAsync(administrativeInstructionalMethodsCollection);

            var sourceContexts = (await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync()).ToList();
            Assert.AreEqual(administrativeInstructionalMethodsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = administrativeInstructionalMethodsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_KeyNotFoundException()
        {
            //
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false))
                .Throws<KeyNotFoundException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_PermissionsException()
        {
            
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false))
                .Throws<PermissionsException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_ArgumentException()
        {
            
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false))
                .Throws<ArgumentException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_RepositoryException()
        {
            
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false))
                .Throws<RepositoryException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_IntegrationApiException()
        {
            
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false))
                .Throws<IntegrationApiException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuidAsync_ValidateFields()
        {
            var expected = administrativeInstructionalMethodsCollection.FirstOrDefault();
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuidAsync_ValidateFields_CacheTrue()
        {
            administrativeInstructionalMethodsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = administrativeInstructionalMethodsCollection.FirstOrDefault();
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethods_Exception()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsAsync(false)).Throws<Exception>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuidAsync_Exception()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_KeyNotFoundException()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_PermissionsException()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_ArgumentException()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_RepositoryException()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_IntegrationApiException()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_GetAdministrativeInstructionalMethodsByGuid_Exception()
        {
            administrativeInstructionalMethodsServiceMock.Setup(x => x.GetAdministrativeInstructionalMethodsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await administrativeInstructionalMethodsController.GetAdministrativeInstructionalMethodsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_PostAdministrativeInstructionalMethodsAsync_Exception()
        {
            await administrativeInstructionalMethodsController.PostAdministrativeInstructionalMethodsAsync(administrativeInstructionalMethodsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_PutAdministrativeInstructionalMethodsAsync_Exception()
        {
            var sourceContext = administrativeInstructionalMethodsCollection.FirstOrDefault();
            await administrativeInstructionalMethodsController.PutAdministrativeInstructionalMethodsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdministrativeInstructionalMethodsController_DeleteAdministrativeInstructionalMethodsAsync_Exception()
        {
            await administrativeInstructionalMethodsController.DeleteAdministrativeInstructionalMethodsAsync(administrativeInstructionalMethodsCollection.FirstOrDefault().Id);
        }
    }
}