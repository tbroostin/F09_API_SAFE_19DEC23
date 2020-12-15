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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CipCodesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICipCodeService> cipCodesServiceMock;
        private Mock<ILogger> loggerMock;
        private CipCodesController cipCodesController;
        private IEnumerable<Domain.Student.Entities.CipCode> allCipcodes;
        private List<Dtos.CipCode> cipCodesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            cipCodesServiceMock = new Mock<ICipCodeService>();
            loggerMock = new Mock<ILogger>();
            cipCodesCollection = new List<Dtos.CipCode>();

            allCipcodes = new List<Domain.Student.Entities.CipCode>()
                {
                    new Domain.Student.Entities.CipCode("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", 2020),
                    new Domain.Student.Entities.CipCode("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", 2020),
                    new Domain.Student.Entities.CipCode("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", 2020)
                };

            foreach (var source in allCipcodes)
            {
                var cipCodes = new Dtos.CipCode
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    RevisionYear = source.RevisionYear
                };
                cipCodesCollection.Add(cipCodes);
            }

            cipCodesController = new CipCodesController(cipCodesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            cipCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            cipCodesController = null;
            allCipcodes = null;
            cipCodesCollection = null;
            loggerMock = null;
            cipCodesServiceMock = null;
        }

        [TestMethod]
        public async Task CipCodesController_GetCipCodes_ValidateFields_Nocache()
        {
            cipCodesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false)).ReturnsAsync(cipCodesCollection);

            var sourceContexts = (await cipCodesController.GetCipCodesAsync()).ToList();
            Assert.AreEqual(cipCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = cipCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.RevisionYear, actual.RevisionYear, "RevisionYear, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CipCodesController_GetCipCodes_ValidateFields_Cache()
        {
            cipCodesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(true)).ReturnsAsync(cipCodesCollection);

            var sourceContexts = (await cipCodesController.GetCipCodesAsync()).ToList();
            Assert.AreEqual(cipCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = cipCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.RevisionYear, actual.RevisionYear, "RevisionYear, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_KeyNotFoundException()
        {
            //
            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false))
                .Throws<KeyNotFoundException>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_PermissionsException()
        {

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false))
                .Throws<PermissionsException>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_ArgumentException()
        {

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false))
                .Throws<ArgumentException>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_RepositoryException()
        {

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false))
                .Throws<RepositoryException>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_IntegrationApiException()
        {

            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false))
                .Throws<IntegrationApiException>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        public async Task CipCodesController_GetCipCodeByGuidAsync_ValidateFields()
        {
            var expected = cipCodesCollection.FirstOrDefault();
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await cipCodesController.GetCipCodeByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.RevisionYear, actual.RevisionYear, "RevisionYear");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodes_Exception()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodesAsync(false)).Throws<Exception>();
            await cipCodesController.GetCipCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodeByGuidAsync_Exception()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await cipCodesController.GetCipCodeByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_KeyNotFoundException()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_PermissionsException()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_ArgumentException()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_RepositoryException()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_IntegrationApiException()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_GetCipCodesByGuid_Exception()
        {
            cipCodesServiceMock.Setup(x => x.GetCipCodeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await cipCodesController.GetCipCodeByIdAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_PostCipCodesAsync_Exception()
        {
            await cipCodesController.PostCipCodeAsync(cipCodesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_PutCipCodesAsync_Exception()
        {
            var sourceContext = cipCodesCollection.FirstOrDefault();
            await cipCodesController.PutCipCodeAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CipCodesController_DeleteCipCodesAsync_Exception()
        {
            await cipCodesController.DeleteCipCodeAsync(cipCodesCollection.FirstOrDefault().Id);
        }
    }
}
