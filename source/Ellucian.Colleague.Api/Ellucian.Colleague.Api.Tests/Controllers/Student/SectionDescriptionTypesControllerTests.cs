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
    public class SectionDescriptionTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ISectionDescriptionTypesService> sectionDescriptionTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private SectionDescriptionTypesController sectionDescriptionTypesController;
        private IEnumerable<Domain.Student.Entities.SectionDescriptionType> allIntgSecDescTypes;
        private List<Dtos.SectionDescriptionTypes> sectionDescriptionTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {

            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            sectionDescriptionTypesServiceMock = new Mock<ISectionDescriptionTypesService>();
            loggerMock = new Mock<ILogger>();
            sectionDescriptionTypesCollection = new List<Dtos.SectionDescriptionTypes>();

            allIntgSecDescTypes = new List<Domain.Student.Entities.SectionDescriptionType>()
                {
                    new Domain.Student.Entities.SectionDescriptionType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionDescriptionType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionDescriptionType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allIntgSecDescTypes)
            {
                var sectionDescriptionTypes = new Ellucian.Colleague.Dtos.SectionDescriptionTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                sectionDescriptionTypesCollection.Add(sectionDescriptionTypes);
            }

            sectionDescriptionTypesController = new SectionDescriptionTypesController(sectionDescriptionTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sectionDescriptionTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionDescriptionTypesController = null;
            allIntgSecDescTypes = null;
            sectionDescriptionTypesCollection = null;
            loggerMock = null;
            sectionDescriptionTypesServiceMock = null;
        }

        [TestMethod]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_ValidateFields_Nocache()
        {
            sectionDescriptionTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false)).ReturnsAsync(sectionDescriptionTypesCollection);

            var sourceContexts = (await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync()).ToList();
            Assert.AreEqual(sectionDescriptionTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionDescriptionTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_ValidateFields_Cache()
        {
            sectionDescriptionTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(true)).ReturnsAsync(sectionDescriptionTypesCollection);

            var sourceContexts = (await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync()).ToList();
            Assert.AreEqual(sectionDescriptionTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sectionDescriptionTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_KeyNotFoundException()
        {
            //
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_PermissionsException()
        {

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false))
                .Throws<PermissionsException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_ArgumentException()
        {

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false))
                .Throws<ArgumentException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_RepositoryException()
        {

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false))
                .Throws<RepositoryException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_IntegrationApiException()
        {

            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false))
                .Throws<IntegrationApiException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypeByGuidAsync_ValidateFields()
        {
            var expected = sectionDescriptionTypesCollection.FirstOrDefault();
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypes_Exception()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypesAsync(false)).Throws<Exception>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypeByGuidAsync_Exception()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_KeyNotFoundException()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_PermissionsException()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_ArgumentException()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_RepositoryException()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_IntegrationApiException()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_GetSectionDescriptionTypesByGuid_Exception()
        {
            sectionDescriptionTypesServiceMock.Setup(x => x.GetSectionDescriptionTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await sectionDescriptionTypesController.GetSectionDescriptionTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_PostSectionDescriptionTypesAsync_Exception()
        {
            await sectionDescriptionTypesController.PostSectionDescriptionTypesAsync(sectionDescriptionTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_PutSectionDescriptionTypesAsync_Exception()
        {
            var sourceContext = sectionDescriptionTypesCollection.FirstOrDefault();
            await sectionDescriptionTypesController.PutSectionDescriptionTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SectionDescriptionTypesController_DeleteSectionDescriptionTypesAsync_Exception()
        {
            await sectionDescriptionTypesController.DeleteSectionDescriptionTypesAsync(sectionDescriptionTypesCollection.FirstOrDefault().Id);
        }
    }
}