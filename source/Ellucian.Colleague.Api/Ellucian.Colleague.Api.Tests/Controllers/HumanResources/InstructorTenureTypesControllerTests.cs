//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class InstructorTenureTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInstructorTenureTypesService> instructorTenureTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private InstructorTenureTypesController instructorTenureTypesController;
        private IEnumerable<TenureTypes> allTenureTypes;
        private List<Dtos.InstructorTenureTypes> instructorTenureTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            instructorTenureTypesServiceMock = new Mock<IInstructorTenureTypesService>();
            loggerMock = new Mock<ILogger>();
            instructorTenureTypesCollection = new List<Dtos.InstructorTenureTypes>();

            allTenureTypes = new List<TenureTypes>()
                {
                    new TenureTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new TenureTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new TenureTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allTenureTypes)
            {
                var instructorTenureTypes = new Ellucian.Colleague.Dtos.InstructorTenureTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                instructorTenureTypesCollection.Add(instructorTenureTypes);
            }

            instructorTenureTypesController = new InstructorTenureTypesController(instructorTenureTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            instructorTenureTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            instructorTenureTypesController = null;
            allTenureTypes = null;
            instructorTenureTypesCollection = null;
            loggerMock = null;
            instructorTenureTypesServiceMock = null;
        }

        [TestMethod]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_ValidateFields_Nocache()
        {
            instructorTenureTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false)).ReturnsAsync(instructorTenureTypesCollection);

            var sourceContexts = (await instructorTenureTypesController.GetInstructorTenureTypesAsync()).ToList();
            Assert.AreEqual(instructorTenureTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorTenureTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_ValidateFields_Cache()
        {
            instructorTenureTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(true)).ReturnsAsync(instructorTenureTypesCollection);

            var sourceContexts = (await instructorTenureTypesController.GetInstructorTenureTypesAsync()).ToList();
            Assert.AreEqual(instructorTenureTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorTenureTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_KeyNotFoundException()
        {
            //
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_PermissionsException()
        {

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false))
                .Throws<PermissionsException>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_ArgumentException()
        {

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false))
                .Throws<ArgumentException>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_RepositoryException()
        {

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false))
                .Throws<RepositoryException>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_IntegrationApiException()
        {

            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false))
                .Throws<IntegrationApiException>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuidAsync_ValidateFields()
        {
            var expected = instructorTenureTypesCollection.FirstOrDefault();
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypes_Exception()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesAsync(false)).Throws<Exception>();
            await instructorTenureTypesController.GetInstructorTenureTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuidAsync_Exception()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_KeyNotFoundException()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_PermissionsException()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_ArgumentException()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_RepositoryException()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_IntegrationApiException()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_GetInstructorTenureTypesByGuid_Exception()
        {
            instructorTenureTypesServiceMock.Setup(x => x.GetInstructorTenureTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await instructorTenureTypesController.GetInstructorTenureTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_PostInstructorTenureTypesAsync_Exception()
        {
            await instructorTenureTypesController.PostInstructorTenureTypesAsync(instructorTenureTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_PutInstructorTenureTypesAsync_Exception()
        {
            var sourceContext = instructorTenureTypesCollection.FirstOrDefault();
            await instructorTenureTypesController.PutInstructorTenureTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorTenureTypesController_DeleteInstructorTenureTypesAsync_Exception()
        {
            await instructorTenureTypesController.DeleteInstructorTenureTypesAsync(instructorTenureTypesCollection.FirstOrDefault().Id);
        }
    }
}