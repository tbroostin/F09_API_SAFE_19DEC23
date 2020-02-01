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
    public class EmergencyContactTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmergencyContactTypesService> emergencyContactTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private EmergencyContactTypesController emergencyContactTypesController;
        private IEnumerable<Domain.Base.Entities.IntgPersonEmerTypes> allIntgPersonEmerTypes;
        private List<Dtos.EmergencyContactTypes> emergencyContactTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            emergencyContactTypesServiceMock = new Mock<IEmergencyContactTypesService>();
            loggerMock = new Mock<ILogger>();
            emergencyContactTypesCollection = new List<Dtos.EmergencyContactTypes>();

            allIntgPersonEmerTypes = new List<Domain.Base.Entities.IntgPersonEmerTypes>()
                {
                    new Domain.Base.Entities.IntgPersonEmerTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.IntgPersonEmerTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.IntgPersonEmerTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allIntgPersonEmerTypes)
            {
                var emergencyContactTypes = new Ellucian.Colleague.Dtos.EmergencyContactTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                emergencyContactTypesCollection.Add(emergencyContactTypes);
            }

            emergencyContactTypesController = new EmergencyContactTypesController(emergencyContactTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            emergencyContactTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            emergencyContactTypesController = null;
            allIntgPersonEmerTypes = null;
            emergencyContactTypesCollection = null;
            loggerMock = null;
            emergencyContactTypesServiceMock = null;
        }

        [TestMethod]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_ValidateFields_Nocache()
        {
            emergencyContactTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false)).ReturnsAsync(emergencyContactTypesCollection);

            var sourceContexts = (await emergencyContactTypesController.GetEmergencyContactTypesAsync()).ToList();
            Assert.AreEqual(emergencyContactTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = emergencyContactTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_ValidateFields_Cache()
        {
            emergencyContactTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(true)).ReturnsAsync(emergencyContactTypesCollection);

            var sourceContexts = (await emergencyContactTypesController.GetEmergencyContactTypesAsync()).ToList();
            Assert.AreEqual(emergencyContactTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = emergencyContactTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_KeyNotFoundException()
        {
            //
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_PermissionsException()
        {

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false))
                .Throws<PermissionsException>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_ArgumentException()
        {

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false))
                .Throws<ArgumentException>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_RepositoryException()
        {

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false))
                .Throws<RepositoryException>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_IntegrationApiException()
        {

            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false))
                .Throws<IntegrationApiException>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuidAsync_ValidateFields()
        {
            var expected = emergencyContactTypesCollection.FirstOrDefault();
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypes_Exception()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesAsync(false)).Throws<Exception>();
            await emergencyContactTypesController.GetEmergencyContactTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuidAsync_Exception()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_KeyNotFoundException()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_PermissionsException()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_ArgumentException()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_RepositoryException()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_IntegrationApiException()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_GetEmergencyContactTypesByGuid_Exception()
        {
            emergencyContactTypesServiceMock.Setup(x => x.GetEmergencyContactTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await emergencyContactTypesController.GetEmergencyContactTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_PostEmergencyContactTypesAsync_Exception()
        {
            await emergencyContactTypesController.PostEmergencyContactTypesAsync(emergencyContactTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_PutEmergencyContactTypesAsync_Exception()
        {
            var sourceContext = emergencyContactTypesCollection.FirstOrDefault();
            await emergencyContactTypesController.PutEmergencyContactTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmergencyContactTypesController_DeleteEmergencyContactTypesAsync_Exception()
        {
            await emergencyContactTypesController.DeleteEmergencyContactTypesAsync(emergencyContactTypesCollection.FirstOrDefault().Id);
        }
    }
}