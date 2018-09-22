//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class VeteranStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IVeteranStatusesService> veteranStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private VeteranStatusesController veteranStatusesController;
        private IEnumerable<Domain.Base.Entities.MilStatuses> allMilStatuses;
        private List<Dtos.VeteranStatuses> veteranStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            veteranStatusesServiceMock = new Mock<IVeteranStatusesService>();
            loggerMock = new Mock<ILogger>();
            veteranStatusesCollection = new List<Dtos.VeteranStatuses>();

            allMilStatuses = new List<Domain.Base.Entities.MilStatuses>()
                {
                    new Domain.Base.Entities.MilStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Base.Entities.MilStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.MilStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allMilStatuses)
            {
                var veteranStatuses = new Ellucian.Colleague.Dtos.VeteranStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                veteranStatusesCollection.Add(veteranStatuses);
            }

            veteranStatusesController = new VeteranStatusesController(veteranStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            veteranStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            veteranStatusesController = null;
            allMilStatuses = null;
            veteranStatusesCollection = null;
            loggerMock = null;
            veteranStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task VeteranStatusesController_GetVeteranStatuses_ValidateFields_Nocache()
        {
            veteranStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false)).ReturnsAsync(veteranStatusesCollection);

            var sourceContexts = (await veteranStatusesController.GetVeteranStatusesAsync()).ToList();
            Assert.AreEqual(veteranStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = veteranStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task VeteranStatusesController_GetVeteranStatuses_ValidateFields_Cache()
        {
            veteranStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(true)).ReturnsAsync(veteranStatusesCollection);

            var sourceContexts = (await veteranStatusesController.GetVeteranStatusesAsync()).ToList();
            Assert.AreEqual(veteranStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = veteranStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_KeyNotFoundException()
        {
            //
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_PermissionsException()
        {

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false))
                .Throws<PermissionsException>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_ArgumentException()
        {

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false))
                .Throws<ArgumentException>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_RepositoryException()
        {

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false))
                .Throws<RepositoryException>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_IntegrationApiException()
        {

            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuidAsync_ValidateFields()
        {
            var expected = veteranStatusesCollection.FirstOrDefault();
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await veteranStatusesController.GetVeteranStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatuses_Exception()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesAsync(false)).Throws<Exception>();
            await veteranStatusesController.GetVeteranStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuidAsync_Exception()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_KeyNotFoundException()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_PermissionsException()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_ArgumentException()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_RepositoryException()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_IntegrationApiException()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_GetVeteranStatusesByGuid_Exception()
        {
            veteranStatusesServiceMock.Setup(x => x.GetVeteranStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await veteranStatusesController.GetVeteranStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_PostVeteranStatusesAsync_Exception()
        {
            await veteranStatusesController.PostVeteranStatusesAsync(veteranStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_PutVeteranStatusesAsync_Exception()
        {
            var sourceContext = veteranStatusesCollection.FirstOrDefault();
            await veteranStatusesController.PutVeteranStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VeteranStatusesController_DeleteVeteranStatusesAsync_Exception()
        {
            await veteranStatusesController.DeleteVeteranStatusesAsync(veteranStatusesCollection.FirstOrDefault().Id);
        }
    }
}