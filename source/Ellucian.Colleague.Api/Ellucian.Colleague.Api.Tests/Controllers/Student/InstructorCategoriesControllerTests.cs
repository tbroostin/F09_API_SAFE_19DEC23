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
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstructorCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInstructorCategoriesService> instructorCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private InstructorCategoriesController instructorCategoriesController;
        private IEnumerable<FacultySpecialStatuses> allFacultySpecialStatuses;
        private List<Dtos.InstructorCategories> instructorCategoriesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            instructorCategoriesServiceMock = new Mock<IInstructorCategoriesService>();
            loggerMock = new Mock<ILogger>();
            instructorCategoriesCollection = new List<Dtos.InstructorCategories>();

            allFacultySpecialStatuses = new List<FacultySpecialStatuses>()
                {
                    new FacultySpecialStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FacultySpecialStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FacultySpecialStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allFacultySpecialStatuses)
            {
                var instructorCategories = new Ellucian.Colleague.Dtos.InstructorCategories
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                instructorCategoriesCollection.Add(instructorCategories);
            }

            instructorCategoriesController = new InstructorCategoriesController(instructorCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            instructorCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            instructorCategoriesController = null;
            allFacultySpecialStatuses = null;
            instructorCategoriesCollection = null;
            loggerMock = null;
            instructorCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task InstructorCategoriesController_GetInstructorCategories_ValidateFields_Nocache()
        {
            instructorCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).ReturnsAsync(instructorCategoriesCollection);

            var sourceContexts = (await instructorCategoriesController.GetInstructorCategoriesAsync()).ToList();
            Assert.AreEqual(instructorCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InstructorCategoriesController_GetInstructorCategories_ValidateFields_Cache()
        {
            instructorCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(true)).ReturnsAsync(instructorCategoriesCollection);

            var sourceContexts = (await instructorCategoriesController.GetInstructorCategoriesAsync()).ToList();
            Assert.AreEqual(instructorCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = instructorCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_ValidateFields()
        {
            var expected = instructorCategoriesCollection.FirstOrDefault();
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await instructorCategoriesController.GetInstructorCategoriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_Exception()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<Exception>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_KeyNotFoundException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<KeyNotFoundException>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_PermissionsException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<PermissionsException>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_ArgumentException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<ArgumentException>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_RepositoryException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<RepositoryException>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategories_IntegrationApiException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesAsync(false)).Throws<IntegrationApiException>();
            await instructorCategoriesController.GetInstructorCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_Exception()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_KeyNotFoundException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_PermissionsException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_ArgumentException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_RepositoryException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_GetInstructorCategoriesByGuidAsync_IntegrationApiException()
        {
            instructorCategoriesServiceMock.Setup(x => x.GetInstructorCategoriesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await instructorCategoriesController.GetInstructorCategoriesByGuidAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_PostInstructorCategoriesAsync_Exception()
        {
            await instructorCategoriesController.PostInstructorCategoriesAsync(instructorCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_PutInstructorCategoriesAsync_Exception()
        {
            var sourceContext = instructorCategoriesCollection.FirstOrDefault();
            await instructorCategoriesController.PutInstructorCategoriesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InstructorCategoriesController_DeleteInstructorCategoriesAsync_Exception()
        {
            await instructorCategoriesController.DeleteInstructorCategoriesAsync(instructorCategoriesCollection.FirstOrDefault().Id);
        }
    }
}