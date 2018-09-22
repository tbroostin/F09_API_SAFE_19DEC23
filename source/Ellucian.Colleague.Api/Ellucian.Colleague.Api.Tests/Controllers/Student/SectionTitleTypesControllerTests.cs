//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class SectionTitleTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ISectionTitleTypesService> sectionTitleTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private SectionTitleTypesController sectionTitleTypesController;
        private IEnumerable<Domain.Student.Entities.SectionTitleType> allIntgSecTitleTypes;
        private List<Dtos.SectionTitleType> sectionTitleTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {

            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            sectionTitleTypesServiceMock = new Mock<ISectionTitleTypesService>();
            loggerMock = new Mock<ILogger>();
            sectionTitleTypesCollection = new List<Dtos.SectionTitleType>();

            allIntgSecTitleTypes = new List<Domain.Student.Entities.SectionTitleType>()
                {
                    new Domain.Student.Entities.SectionTitleType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionTitleType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionTitleType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allIntgSecTitleTypes)
            {
                var sectionTitleTypes = new Ellucian.Colleague.Dtos.SectionTitleType
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                sectionTitleTypesCollection.Add(sectionTitleTypes);
            }

            sectionTitleTypesController = new SectionTitleTypesController(sectionTitleTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sectionTitleTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionTitleTypesController = null;
            allIntgSecTitleTypes = null;
            sectionTitleTypesCollection = null;
            loggerMock = null;
            sectionTitleTypesServiceMock = null;
        }

        [TestMethod]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_ValidateFields_Nocache()
        {
            sectionTitleTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false)).ReturnsAsync(sectionTitleTypesCollection);

            var sourceContexts = (await sectionTitleTypesController.GetSectionTitleTypesAsync()).ToList();
            Assert.AreEqual(sectionTitleTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionTitleTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_ValidateFields_Cache()
        {
            sectionTitleTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(true)).ReturnsAsync(sectionTitleTypesCollection);

            var sourceContexts = (await sectionTitleTypesController.GetSectionTitleTypesAsync()).ToList();
            Assert.AreEqual(sectionTitleTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionTitleTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_KeyNotFoundException()
        {
            //
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_PermissionsException()
        {

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false))
                .Throws<PermissionsException>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_ArgumentException()
        {

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false))
                .Throws<ArgumentException>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_RepositoryException()
        {

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false))
                .Throws<RepositoryException>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_IntegrationApiException()
        {

            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false))
                .Throws<IntegrationApiException>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        public async Task SectionTitleTypesController_GetSectionTitleTypeByGuidAsync_ValidateFields()
        {
            var expected = sectionTitleTypesCollection.FirstOrDefault();
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypes_Exception()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypesAsync(false)).Throws<Exception>();
            await sectionTitleTypesController.GetSectionTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypeByGuidAsync_Exception()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_KeyNotFoundException()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_PermissionsException()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_ArgumentException()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_RepositoryException()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_IntegrationApiException()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_GetSectionTitleTypesByGuid_Exception()
        {
            sectionTitleTypesServiceMock.Setup(x => x.GetSectionTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await sectionTitleTypesController.GetSectionTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_PostSectionTitleTypesAsync_Exception()
        {
            await sectionTitleTypesController.PostSectionTitleTypeAsync(sectionTitleTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_PutSectionTitleTypesAsync_Exception()
        {
            var sourceContext = sectionTitleTypesCollection.FirstOrDefault();
            await sectionTitleTypesController.PutSectionTitleTypeAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionTitleTypesController_DeleteSectionTitleTypesAsync_Exception()
        {
            await sectionTitleTypesController.DeleteSectionTitleTypeAsync(sectionTitleTypesCollection.FirstOrDefault().Id);
        }
    }
}