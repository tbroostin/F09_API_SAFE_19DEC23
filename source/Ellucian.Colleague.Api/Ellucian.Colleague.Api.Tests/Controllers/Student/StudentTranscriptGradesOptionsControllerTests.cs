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
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentTranscriptGradesOptionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentTranscriptGradesOptionsService> studentTranscriptGradesOptionsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentTranscriptGradesOptionsController studentTranscriptGradesOptionsController;
        private IEnumerable<Domain.Student.Entities.StudentTranscriptGradesOptions> allStudentAcademicCredit;
        private List<Dtos.StudentTranscriptGradesOptions> studentTranscriptGradesOptionsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private Ellucian.Web.Http.Models.QueryStringFilter studentCriteriaFilter
         = new Web.Http.Models.QueryStringFilter("student", "");
        private Dtos.Filters.StudentFilter studentFilter;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentTranscriptGradesOptionsServiceMock = new Mock<IStudentTranscriptGradesOptionsService>();
            loggerMock = new Mock<ILogger>();
            studentTranscriptGradesOptionsCollection = new List<Dtos.StudentTranscriptGradesOptions>();

            allStudentAcademicCredit = new List<Domain.Student.Entities.StudentTranscriptGradesOptions>()
                {
                    new Domain.Student.Entities.StudentTranscriptGradesOptions("1", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.Student.Entities.StudentTranscriptGradesOptions("2", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.Student.Entities.StudentTranscriptGradesOptions("3", "d2253ac7-9931-4560-b42f-1fccd43c952e")
                };

            foreach (var source in allStudentAcademicCredit)
            {
                var studentTranscriptGradesOptions = new Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions()
                {
                    Id = source.Id

                };
                studentTranscriptGradesOptionsCollection.Add(studentTranscriptGradesOptions);
            }

            var gradeOptionsTuple = new Tuple<IEnumerable<Dtos.StudentTranscriptGradesOptions>, int>(studentTranscriptGradesOptionsCollection, 3);
            studentFilter = new Dtos.Filters.StudentFilter { Student = new GuidObject2("guid1") };
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).ReturnsAsync(gradeOptionsTuple);

            studentTranscriptGradesOptionsController = new StudentTranscriptGradesOptionsController(studentTranscriptGradesOptionsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentTranscriptGradesOptionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentTranscriptGradesOptionsController = null;
            allStudentAcademicCredit = null;
            studentTranscriptGradesOptionsCollection = null;
            loggerMock = null;
            studentTranscriptGradesOptionsServiceMock = null;
        }

        #region student-transcript-grades-options

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuidAsync_Exception()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_KeyNotFoundException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_PermissionsException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_ArgumentException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_RepositoryException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_IntegrationApiException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsByGuid_Exception()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        public async Task GetStudentTranscriptGradesOptionsAsync_NoCache()
        {
            studentTranscriptGradesOptionsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentTranscriptGradesOptionsController.Request.Properties.Add(
                 string.Format("FilterObject{0}", "student"),
                 new Dtos.Filters.StudentFilter { Student = new GuidObject2("guid1") });
            var student = await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
            Assert.IsTrue(student is IHttpActionResult);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionsAsync_Exception()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<Exception>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptions_KeyNotFoundException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptions_PermissionsException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptionns_ArgumentException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptions_RepositoryException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptions_IntegrationApiException()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_GetStudentTranscriptGradesOptions_Exception()
        {
            studentTranscriptGradesOptionsServiceMock.Setup(x => x.GetStudentTranscriptGradesOptionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.StudentFilter>(), It.IsAny<bool>())).Throws<Exception>();
            await studentTranscriptGradesOptionsController.GetStudentTranscriptGradesOptionsAsync(new Paging(1, 0), studentCriteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_PostStudentTranscriptGradesOptionsAsync_Exception()
        {
            await studentTranscriptGradesOptionsController.PostStudentTranscriptGradesOptionsAsync(studentTranscriptGradesOptionsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentTranscriptGradesOptionsController_PutStudentTranscriptGradesOptionsAsync_Exception()
        {
            var sourceContext = studentTranscriptGradesOptionsCollection.FirstOrDefault();
            await studentTranscriptGradesOptionsController.PutStudentTranscriptGradesOptionsAsync(sourceContext.Id, sourceContext);
        }



        #endregion


    }
}