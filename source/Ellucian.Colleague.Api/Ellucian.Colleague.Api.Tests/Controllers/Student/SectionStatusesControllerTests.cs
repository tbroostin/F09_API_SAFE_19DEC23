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
    public class SectionStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ISectionStatusesService> sectionStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private SectionStatusesController sectionStatusesController;      
        private IEnumerable<Domain.Student.Entities.SectionStatuses> allSectionStatuses;
        private List<Dtos.SectionStatuses> sectionStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            sectionStatusesServiceMock = new Mock<ISectionStatusesService>();
            loggerMock = new Mock<ILogger>();
            sectionStatusesCollection = new List<Dtos.SectionStatuses>();

            allSectionStatuses  = new List<Domain.Student.Entities.SectionStatuses>()
                {
                    new Domain.Student.Entities.SectionStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allSectionStatuses)
            {
                var sectionStatuses = new Ellucian.Colleague.Dtos.SectionStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                sectionStatusesCollection.Add(sectionStatuses);
            }

            sectionStatusesController = new SectionStatusesController(sectionStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sectionStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionStatusesController = null;
            allSectionStatuses = null;
            sectionStatusesCollection = null;
            loggerMock = null;
            sectionStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task SectionStatusesController_GetSectionStatuses_ValidateFields_Nocache()
        {
            sectionStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false)).ReturnsAsync(sectionStatusesCollection);
       
            var sourceContexts = (await sectionStatusesController.GetSectionStatusesAsync()).ToList();
            Assert.AreEqual(sectionStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SectionStatusesController_GetSectionStatuses_ValidateFields_Cache()
        {
            sectionStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(true)).ReturnsAsync(sectionStatusesCollection);

            var sourceContexts = (await sectionStatusesController.GetSectionStatusesAsync()).ToList();
            Assert.AreEqual(sectionStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_KeyNotFoundException()
        {
            //
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await sectionStatusesController.GetSectionStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_PermissionsException()
        {
            
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false))
                .Throws<PermissionsException>();
            await sectionStatusesController.GetSectionStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_ArgumentException()
        {
            
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false))
                .Throws<ArgumentException>();
            await sectionStatusesController.GetSectionStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_RepositoryException()
        {
            
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false))
                .Throws<RepositoryException>();
            await sectionStatusesController.GetSectionStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_IntegrationApiException()
        {
            
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await sectionStatusesController.GetSectionStatusesAsync();
        }

        [TestMethod]
        public async Task SectionStatusesController_GetSectionStatusesByGuidAsync_ValidateFields()
        {
            var expected = sectionStatusesCollection.FirstOrDefault();
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await sectionStatusesController.GetSectionStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatuses_Exception()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesAsync(false)).Throws<Exception>();
            await sectionStatusesController.GetSectionStatusesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuidAsync_Exception()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_KeyNotFoundException()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_PermissionsException()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_ArgumentException()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_RepositoryException()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_IntegrationApiException()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_GetSectionStatusesByGuid_Exception()
        {
            sectionStatusesServiceMock.Setup(x => x.GetSectionStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await sectionStatusesController.GetSectionStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_PostSectionStatusesAsync_Exception()
        {
            await sectionStatusesController.PostSectionStatusesAsync(sectionStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_PutSectionStatusesAsync_Exception()
        {
            var sourceContext = sectionStatusesCollection.FirstOrDefault();
            await sectionStatusesController.PutSectionStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionStatusesController_DeleteSectionStatusesAsync_Exception()
        {
            await sectionStatusesController.DeleteSectionStatusesAsync(sectionStatusesCollection.FirstOrDefault().Id);
        }
    }
}