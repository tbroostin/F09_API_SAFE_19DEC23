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
    public class CourseTransferStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICourseTransferStatusesService> courseTransferStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private CourseTransferStatusesController courseTransferStatusesController;      
        private IEnumerable<Domain.Student.Entities.CourseTransferStatus> allStudentAcadCredStatuses;
        private List<Dtos.CourseTransferStatuses> courseTransferStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            courseTransferStatusesServiceMock = new Mock<ICourseTransferStatusesService>();
            loggerMock = new Mock<ILogger>();
            courseTransferStatusesCollection = new List<Dtos.CourseTransferStatuses>();

            allStudentAcadCredStatuses  = new List<Domain.Student.Entities.CourseTransferStatus>()
                {
                    new Domain.Student.Entities.CourseTransferStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.CourseTransferStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.CourseTransferStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allStudentAcadCredStatuses)
            {
                var courseTransferStatuses = new Ellucian.Colleague.Dtos.CourseTransferStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                courseTransferStatusesCollection.Add(courseTransferStatuses);
            }

            courseTransferStatusesController = new CourseTransferStatusesController(courseTransferStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            courseTransferStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseTransferStatusesController = null;
            allStudentAcadCredStatuses = null;
            courseTransferStatusesCollection = null;
            loggerMock = null;
            courseTransferStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_ValidateFields_Nocache()
        {
            courseTransferStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false)).ReturnsAsync(courseTransferStatusesCollection);
       
            var sourceContexts = (await courseTransferStatusesController.GetCourseTransferStatusesAsync()).ToList();
            Assert.AreEqual(courseTransferStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTransferStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_ValidateFields_Cache()
        {
            courseTransferStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(true)).ReturnsAsync(courseTransferStatusesCollection);

            var sourceContexts = (await courseTransferStatusesController.GetCourseTransferStatusesAsync()).ToList();
            Assert.AreEqual(courseTransferStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = courseTransferStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_KeyNotFoundException()
        {
            //
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_PermissionsException()
        {
            
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false))
                .Throws<PermissionsException>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_ArgumentException()
        {
            
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false))
                .Throws<ArgumentException>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_RepositoryException()
        {
            
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false))
                .Throws<RepositoryException>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_IntegrationApiException()
        {
            
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();
        }

        [TestMethod]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuidAsync_ValidateFields()
        {
            var expected = courseTransferStatusesCollection.FirstOrDefault();
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatuses_Exception()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesAsync(false)).Throws<Exception>();
            await courseTransferStatusesController.GetCourseTransferStatusesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuidAsync_Exception()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_KeyNotFoundException()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_PermissionsException()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_ArgumentException()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_RepositoryException()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_IntegrationApiException()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_GetCourseTransferStatusesByGuid_Exception()
        {
            courseTransferStatusesServiceMock.Setup(x => x.GetCourseTransferStatusesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await courseTransferStatusesController.GetCourseTransferStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_PostCourseTransferStatusesAsync_Exception()
        {
            await courseTransferStatusesController.PostCourseTransferStatusesAsync(courseTransferStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_PutCourseTransferStatusesAsync_Exception()
        {
            var sourceContext = courseTransferStatusesCollection.FirstOrDefault();
            await courseTransferStatusesController.PutCourseTransferStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CourseTransferStatusesController_DeleteCourseTransferStatusesAsync_Exception()
        {
            await courseTransferStatusesController.DeleteCourseTransferStatusesAsync(courseTransferStatusesCollection.FirstOrDefault().Id);
        }
    }
}