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
    public class AttendanceCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAttendanceCategoriesService> attendanceCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private AttendanceCategoriesController attendanceCategoriesController;
        private IEnumerable<Domain.Student.Entities.AttendanceTypes> allAttendanceTypes;
        private List<Dtos.AttendanceCategories> attendanceCategoriesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            attendanceCategoriesServiceMock = new Mock<IAttendanceCategoriesService>();
            loggerMock = new Mock<ILogger>();
            attendanceCategoriesCollection = new List<Dtos.AttendanceCategories>();

            allAttendanceTypes = new List<Domain.Student.Entities.AttendanceTypes>()
                {
                    new Domain.Student.Entities.AttendanceTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.AttendanceTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.AttendanceTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allAttendanceTypes)
            {
                var attendanceCategories = new Ellucian.Colleague.Dtos.AttendanceCategories
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                attendanceCategoriesCollection.Add(attendanceCategories);
            }

            attendanceCategoriesController = new AttendanceCategoriesController(attendanceCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            attendanceCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            attendanceCategoriesController = null;
            allAttendanceTypes = null;
            attendanceCategoriesCollection = null;
            loggerMock = null;
            attendanceCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_ValidateFields_Nocache()
        {
            attendanceCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false)).ReturnsAsync(attendanceCategoriesCollection);

            var sourceContexts = (await attendanceCategoriesController.GetAttendanceCategoriesAsync()).ToList();
            Assert.AreEqual(attendanceCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = attendanceCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_ValidateFields_Cache()
        {
            attendanceCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(true)).ReturnsAsync(attendanceCategoriesCollection);

            var sourceContexts = (await attendanceCategoriesController.GetAttendanceCategoriesAsync()).ToList();
            Assert.AreEqual(attendanceCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = attendanceCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_KeyNotFoundException()
        {
            //
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false))
                .Throws<KeyNotFoundException>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_PermissionsException()
        {

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false))
                .Throws<PermissionsException>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_ArgumentException()
        {

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false))
                .Throws<ArgumentException>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_RepositoryException()
        {

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false))
                .Throws<RepositoryException>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_IntegrationApiException()
        {

            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false))
                .Throws<IntegrationApiException>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuidAsync_ValidateFields()
        {
            var expected = attendanceCategoriesCollection.FirstOrDefault();
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategories_Exception()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesAsync(false)).Throws<Exception>();
            await attendanceCategoriesController.GetAttendanceCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuidAsync_Exception()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_KeyNotFoundException()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_PermissionsException()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_ArgumentException()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_RepositoryException()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_IntegrationApiException()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_GetAttendanceCategoriesByGuid_Exception()
        {
            attendanceCategoriesServiceMock.Setup(x => x.GetAttendanceCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await attendanceCategoriesController.GetAttendanceCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_PostAttendanceCategoriesAsync_Exception()
        {
            await attendanceCategoriesController.PostAttendanceCategoriesAsync(attendanceCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_PutAttendanceCategoriesAsync_Exception()
        {
            var sourceContext = attendanceCategoriesCollection.FirstOrDefault();
            await attendanceCategoriesController.PutAttendanceCategoriesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AttendanceCategoriesController_DeleteAttendanceCategoriesAsync_Exception()
        {
            await attendanceCategoriesController.DeleteAttendanceCategoriesAsync(attendanceCategoriesCollection.FirstOrDefault().Id);
        }
    }
}