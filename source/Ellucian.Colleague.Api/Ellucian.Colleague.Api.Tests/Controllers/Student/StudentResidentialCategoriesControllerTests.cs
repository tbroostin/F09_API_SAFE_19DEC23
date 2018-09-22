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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentResidentialCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentResidentialCategoriesService> studentResidentialCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentResidentialCategoriesController studentResidentialCategoriesController;
        private IEnumerable<Domain.Student.Entities.StudentResidentialCategories> allMealClass;
        private List<Dtos.StudentResidentialCategories> studentResidentialCategoriesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentResidentialCategoriesServiceMock = new Mock<IStudentResidentialCategoriesService>();
            loggerMock = new Mock<ILogger>();
            studentResidentialCategoriesCollection = new List<Dtos.StudentResidentialCategories>();

            allMealClass = new List<Domain.Student.Entities.StudentResidentialCategories>()
                {
                    new Domain.Student.Entities.StudentResidentialCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.StudentResidentialCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.StudentResidentialCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allMealClass)
            {
                var studentResidentialCategories = new Ellucian.Colleague.Dtos.StudentResidentialCategories
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                studentResidentialCategoriesCollection.Add(studentResidentialCategories);
            }

            studentResidentialCategoriesController = new StudentResidentialCategoriesController(studentResidentialCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentResidentialCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentResidentialCategoriesController = null;
            allMealClass = null;
            studentResidentialCategoriesCollection = null;
            loggerMock = null;
            studentResidentialCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_ValidateFields_Nocache()
        {
            studentResidentialCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false)).ReturnsAsync(studentResidentialCategoriesCollection);

            var sourceContexts = (await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync()).ToList();
            Assert.AreEqual(studentResidentialCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = studentResidentialCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_ValidateFields_Cache()
        {
            studentResidentialCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(true)).ReturnsAsync(studentResidentialCategoriesCollection);

            var sourceContexts = (await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync()).ToList();
            Assert.AreEqual(studentResidentialCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = studentResidentialCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_KeyNotFoundException()
        {
            //
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false))
                .Throws<KeyNotFoundException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_PermissionsException()
        {

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false))
                .Throws<PermissionsException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_ArgumentException()
        {

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false))
                .Throws<ArgumentException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_RepositoryException()
        {

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false))
                .Throws<RepositoryException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_IntegrationApiException()
        {

            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false))
                .Throws<IntegrationApiException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuidAsync_ValidateFields()
        {
            var expected = studentResidentialCategoriesCollection.FirstOrDefault();
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategories_Exception()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesAsync(false)).Throws<Exception>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuidAsync_Exception()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_KeyNotFoundException()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_PermissionsException()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_ArgumentException()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_RepositoryException()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_IntegrationApiException()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_GetStudentResidentialCategoriesByGuid_Exception()
        {
            studentResidentialCategoriesServiceMock.Setup(x => x.GetStudentResidentialCategoriesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await studentResidentialCategoriesController.GetStudentResidentialCategoriesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_PostStudentResidentialCategoriesAsync_Exception()
        {
            await studentResidentialCategoriesController.PostStudentResidentialCategoriesAsync(studentResidentialCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_PutStudentResidentialCategoriesAsync_Exception()
        {
            var sourceContext = studentResidentialCategoriesCollection.FirstOrDefault();
            await studentResidentialCategoriesController.PutStudentResidentialCategoriesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentResidentialCategoriesController_DeleteStudentResidentialCategoriesAsync_Exception()
        {
            await studentResidentialCategoriesController.DeleteStudentResidentialCategoriesAsync(studentResidentialCategoriesCollection.FirstOrDefault().Id);
        }
    }
}