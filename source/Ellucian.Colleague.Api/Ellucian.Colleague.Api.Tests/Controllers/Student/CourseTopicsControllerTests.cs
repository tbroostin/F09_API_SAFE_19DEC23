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
    public class CourseTopicsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICourseTopicsService> courseTopicsServiceMock;
        private Mock<ILogger> loggerMock;
        private CourseTopicsController courseTopicsController;      
        private IEnumerable<Domain.Student.Entities.CourseTopic> allCourseTopic;
        private List<Dtos.CourseTopics> courseTopicsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            courseTopicsServiceMock = new Mock<ICourseTopicsService>();
            loggerMock = new Mock<ILogger>();
            courseTopicsCollection = new List<Dtos.CourseTopics>();

            allCourseTopic  = new List<Domain.Student.Entities.CourseTopic>()
                {
                    new Domain.Student.Entities.CourseTopic("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CourseTopic("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CourseTopic("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allCourseTopic)
            {
                var courseTopics = new Ellucian.Colleague.Dtos.CourseTopics
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                courseTopicsCollection.Add(courseTopics);
            }

            courseTopicsController = new CourseTopicsController(courseTopicsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            courseTopicsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseTopicsController = null;
            allCourseTopic = null;
            courseTopicsCollection = null;
            loggerMock = null;
            courseTopicsServiceMock = null;
        }

        [TestMethod]
        public async Task CourseTopicsController_GetCourseTopics_ValidateFields_Nocache()
        {
            courseTopicsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false)).ReturnsAsync(courseTopicsCollection);
       
            var sourceContexts = (await courseTopicsController.GetCourseTopicsAsync()).ToList();
            Assert.AreEqual(courseTopicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTopicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CourseTopicsController_GetCourseTopics_ValidateFields_Cache()
        {
            courseTopicsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(true)).ReturnsAsync(courseTopicsCollection);

            var sourceContexts = (await courseTopicsController.GetCourseTopicsAsync()).ToList();
            Assert.AreEqual(courseTopicsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTopicsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_KeyNotFoundException()
        {
            //
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false))
                .Throws<KeyNotFoundException>();
            await courseTopicsController.GetCourseTopicsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_PermissionsException()
        {
            
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false))
                .Throws<PermissionsException>();
            await courseTopicsController.GetCourseTopicsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_ArgumentException()
        {
            
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false))
                .Throws<ArgumentException>();
            await courseTopicsController.GetCourseTopicsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_RepositoryException()
        {
            
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false))
                .Throws<RepositoryException>();
            await courseTopicsController.GetCourseTopicsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_IntegrationApiException()
        {
            
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false))
                .Throws<IntegrationApiException>();
            await courseTopicsController.GetCourseTopicsAsync();
        }

        [TestMethod]
        public async Task CourseTopicsController_GetCourseTopicsByGuidAsync_ValidateFields()
        {
            var expected = courseTopicsCollection.FirstOrDefault();
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await courseTopicsController.GetCourseTopicsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopics_Exception()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsAsync(false)).Throws<Exception>();
            await courseTopicsController.GetCourseTopicsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuidAsync_Exception()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_KeyNotFoundException()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_PermissionsException()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_ArgumentException()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_RepositoryException()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_IntegrationApiException()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_GetCourseTopicsByGuid_Exception()
        {
            courseTopicsServiceMock.Setup(x => x.GetCourseTopicsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await courseTopicsController.GetCourseTopicsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_PostCourseTopicsAsync_Exception()
        {
            await courseTopicsController.PostCourseTopicsAsync(courseTopicsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_PutCourseTopicsAsync_Exception()
        {
            var sourceContext = courseTopicsCollection.FirstOrDefault();
            await courseTopicsController.PutCourseTopicsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTopicsController_DeleteCourseTopicsAsync_Exception()
        {
            await courseTopicsController.DeleteCourseTopicsAsync(courseTopicsCollection.FirstOrDefault().Id);
        }
    }
}