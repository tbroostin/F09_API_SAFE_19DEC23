//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EmergencyContactPhoneAvailabilitiesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmergencyContactPhoneAvailabilitiesService> emergencyContactPhoneAvailabilitiesServiceMock;
        private Mock<ILogger> loggerMock;
        private EmergencyContactPhoneAvailabilitiesController emergencyContactPhoneAvailabilitiesController;
        private IEnumerable<Domain.Base.Entities.IntgPersonEmerPhoneTypes> allIntgPersonEmerPhoneTypes;
        private List<Dtos.EmergencyContactPhoneAvailabilities> emergencyContactPhoneAvailabilitiesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            emergencyContactPhoneAvailabilitiesServiceMock = new Mock<IEmergencyContactPhoneAvailabilitiesService>();
            loggerMock = new Mock<ILogger>();
            emergencyContactPhoneAvailabilitiesCollection = new List<Dtos.EmergencyContactPhoneAvailabilities>();

            allIntgPersonEmerPhoneTypes = new List<Domain.Base.Entities.IntgPersonEmerPhoneTypes>()
                {
                    new Domain.Base.Entities.IntgPersonEmerPhoneTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.IntgPersonEmerPhoneTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.IntgPersonEmerPhoneTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allIntgPersonEmerPhoneTypes)
            {
                var emergencyContactPhoneAvailabilities = new Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                emergencyContactPhoneAvailabilitiesCollection.Add(emergencyContactPhoneAvailabilities);
            }

            emergencyContactPhoneAvailabilitiesController = new EmergencyContactPhoneAvailabilitiesController(emergencyContactPhoneAvailabilitiesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            emergencyContactPhoneAvailabilitiesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            emergencyContactPhoneAvailabilitiesController = null;
            allIntgPersonEmerPhoneTypes = null;
            emergencyContactPhoneAvailabilitiesCollection = null;
            loggerMock = null;
            emergencyContactPhoneAvailabilitiesServiceMock = null;
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_ValidateFields_Nocache()
        {
            emergencyContactPhoneAvailabilitiesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false)).ReturnsAsync(emergencyContactPhoneAvailabilitiesCollection);

            var sourceContexts = (await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync()).ToList();
            Assert.AreEqual(emergencyContactPhoneAvailabilitiesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = emergencyContactPhoneAvailabilitiesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_ValidateFields_Cache()
        {
            emergencyContactPhoneAvailabilitiesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(true)).ReturnsAsync(emergencyContactPhoneAvailabilitiesCollection);

            var sourceContexts = (await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync()).ToList();
            Assert.AreEqual(emergencyContactPhoneAvailabilitiesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = emergencyContactPhoneAvailabilitiesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_KeyNotFoundException()
        {
            //
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false))
                .Throws<KeyNotFoundException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_PermissionsException()
        {

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false))
                .Throws<PermissionsException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_ArgumentException()
        {

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false))
                .Throws<ArgumentException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_RepositoryException()
        {

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false))
                .Throws<RepositoryException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_IntegrationApiException()
        {

            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false))
                .Throws<IntegrationApiException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_ValidateFields()
        {
            var expected = emergencyContactPhoneAvailabilitiesCollection.FirstOrDefault();
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilities_Exception()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesAsync(false)).Throws<Exception>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuidAsync_Exception()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_KeyNotFoundException()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_PermissionsException()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_ArgumentException()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_RepositoryException()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_IntegrationApiException()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_GetEmergencyContactPhoneAvailabilitiesByGuid_Exception()
        {
            emergencyContactPhoneAvailabilitiesServiceMock.Setup(x => x.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await emergencyContactPhoneAvailabilitiesController.GetEmergencyContactPhoneAvailabilitiesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_PostEmergencyContactPhoneAvailabilitiesAsync_Exception()
        {
            await emergencyContactPhoneAvailabilitiesController.PostEmergencyContactPhoneAvailabilitiesAsync(emergencyContactPhoneAvailabilitiesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_PutEmergencyContactPhoneAvailabilitiesAsync_Exception()
        {
            var sourceContext = emergencyContactPhoneAvailabilitiesCollection.FirstOrDefault();
            await emergencyContactPhoneAvailabilitiesController.PutEmergencyContactPhoneAvailabilitiesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactPhoneAvailabilitiesController_DeleteEmergencyContactPhoneAvailabilitiesAsync_Exception()
        {
            await emergencyContactPhoneAvailabilitiesController.DeleteEmergencyContactPhoneAvailabilitiesAsync(emergencyContactPhoneAvailabilitiesCollection.FirstOrDefault().Id);
        }
    }
}