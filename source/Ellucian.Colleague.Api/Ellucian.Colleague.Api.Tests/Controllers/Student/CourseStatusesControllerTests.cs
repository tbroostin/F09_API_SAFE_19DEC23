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
    public class CourseStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICourseStatusesService> courseStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private CourseStatusesController courseStatusesController;      
        private IEnumerable<Domain.Student.Entities.CourseStatuses> allCourseStatuses;
        private List<Dtos.CourseStatuses> courseStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            courseStatusesServiceMock = new Mock<ICourseStatusesService>();
            loggerMock = new Mock<ILogger>();
            courseStatusesCollection = new List<Dtos.CourseStatuses>();

            allCourseStatuses  = new List<Domain.Student.Entities.CourseStatuses>()
                {
                    new Domain.Student.Entities.CourseStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CourseStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CourseStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allCourseStatuses)
            {
                var courseStatuses = new Ellucian.Colleague.Dtos.CourseStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                courseStatusesCollection.Add(courseStatuses);
            }

            courseStatusesController = new CourseStatusesController(courseStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            courseStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseStatusesController = null;
            allCourseStatuses = null;
            courseStatusesCollection = null;
            loggerMock = null;
            courseStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task CourseStatusesController_GetCourseStatuses_ValidateFields_Nocache()
        {
            courseStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false)).ReturnsAsync(courseStatusesCollection);
       
            var sourceContexts = (await courseStatusesController.GetCourseStatusesAsync()).ToList();
            Assert.AreEqual(courseStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CourseStatusesController_GetCourseStatuses_ValidateFields_Cache()
        {
            courseStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(true)).ReturnsAsync(courseStatusesCollection);

            var sourceContexts = (await courseStatusesController.GetCourseStatusesAsync()).ToList();
            Assert.AreEqual(courseStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_KeyNotFoundException()
        {
            //
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await courseStatusesController.GetCourseStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_PermissionsException()
        {
            
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false))
                .Throws<PermissionsException>();
            await courseStatusesController.GetCourseStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_ArgumentException()
        {
            
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false))
                .Throws<ArgumentException>();
            await courseStatusesController.GetCourseStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_RepositoryException()
        {
            
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false))
                .Throws<RepositoryException>();
            await courseStatusesController.GetCourseStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_IntegrationApiException()
        {
            
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await courseStatusesController.GetCourseStatusesAsync();
        }

        [TestMethod]
        public async Task CourseStatusesController_GetCourseStatusesByGuidAsync_ValidateFields()
        {
            var expected = courseStatusesCollection.FirstOrDefault();
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await courseStatusesController.GetCourseStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatuses_Exception()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesAsync(false)).Throws<Exception>();
            await courseStatusesController.GetCourseStatusesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuidAsync_Exception()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_KeyNotFoundException()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_PermissionsException()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_ArgumentException()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_RepositoryException()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_IntegrationApiException()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_GetCourseStatusesByGuid_Exception()
        {
            courseStatusesServiceMock.Setup(x => x.GetCourseStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await courseStatusesController.GetCourseStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_PostCourseStatusesAsync_Exception()
        {
            await courseStatusesController.PostCourseStatusesAsync(courseStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_PutCourseStatusesAsync_Exception()
        {
            var sourceContext = courseStatusesCollection.FirstOrDefault();
            await courseStatusesController.PutCourseStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseStatusesController_DeleteCourseStatusesAsync_Exception()
        {
            await courseStatusesController.DeleteCourseStatusesAsync(courseStatusesCollection.FirstOrDefault().Id);
        }
    }
}