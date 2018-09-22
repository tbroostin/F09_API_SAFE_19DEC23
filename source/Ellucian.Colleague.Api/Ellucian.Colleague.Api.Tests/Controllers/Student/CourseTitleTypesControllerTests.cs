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
    public class CourseTitleTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICourseTitleTypesService> courseTitleTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private CourseTitleTypesController courseTitleTypesController;
        private IEnumerable<Domain.Student.Entities.CourseTitleType> allIntgCrsTitleTypes;
        private List<Dtos.CourseTitleType> courseTitleTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            courseTitleTypesServiceMock = new Mock<ICourseTitleTypesService>();
            loggerMock = new Mock<ILogger>();
            courseTitleTypesCollection = new List<Dtos.CourseTitleType>();

            allIntgCrsTitleTypes = new List<Domain.Student.Entities.CourseTitleType>()
                {
                    new Domain.Student.Entities.CourseTitleType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CourseTitleType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CourseTitleType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allIntgCrsTitleTypes)
            {
                var courseTitleTypes = new Ellucian.Colleague.Dtos.CourseTitleType
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                courseTitleTypesCollection.Add(courseTitleTypes);
            }

            courseTitleTypesController = new CourseTitleTypesController(courseTitleTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            courseTitleTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseTitleTypesController = null;
            allIntgCrsTitleTypes = null;
            courseTitleTypesCollection = null;
            loggerMock = null;
            courseTitleTypesServiceMock = null;
        }

        [TestMethod]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_ValidateFields_Nocache()
        {
            courseTitleTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false)).ReturnsAsync(courseTitleTypesCollection);

            var sourceContexts = (await courseTitleTypesController.GetCourseTitleTypesAsync()).ToList();
            Assert.AreEqual(courseTitleTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTitleTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_ValidateFields_Cache()
        {
            courseTitleTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(true)).ReturnsAsync(courseTitleTypesCollection);

            var sourceContexts = (await courseTitleTypesController.GetCourseTitleTypesAsync()).ToList();
            Assert.AreEqual(courseTitleTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTitleTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_KeyNotFoundException()
        {
            //
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_PermissionsException()
        {

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false))
                .Throws<PermissionsException>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_ArgumentException()
        {

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false))
                .Throws<ArgumentException>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_RepositoryException()
        {

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false))
                .Throws<RepositoryException>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_IntegrationApiException()
        {

            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false))
                .Throws<IntegrationApiException>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        public async Task CourseTitleTypesController_GetCourseTitleTypeByGuidAsync_ValidateFields()
        {
            var expected = courseTitleTypesCollection.FirstOrDefault();
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypes_Exception()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypesAsync(false)).Throws<Exception>();
            await courseTitleTypesController.GetCourseTitleTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypeByGuidAsync_Exception()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_KeyNotFoundException()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_PermissionsException()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_ArgumentException()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_RepositoryException()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_IntegrationApiException()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_GetCourseTitleTypesByGuid_Exception()
        {
            courseTitleTypesServiceMock.Setup(x => x.GetCourseTitleTypeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await courseTitleTypesController.GetCourseTitleTypeByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_PostCourseTitleTypesAsync_Exception()
        {
            await courseTitleTypesController.PostCourseTitleTypesAsync(courseTitleTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_PutCourseTitleTypesAsync_Exception()
        {
            var sourceContext = courseTitleTypesCollection.FirstOrDefault();
            await courseTitleTypesController.PutCourseTitleTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTitleTypesController_DeleteCourseTitleTypesAsync_Exception()
        {
            await courseTitleTypesController.DeleteCourseTitleTypesAsync(courseTitleTypesCollection.FirstOrDefault().Id);
        }
    }
}