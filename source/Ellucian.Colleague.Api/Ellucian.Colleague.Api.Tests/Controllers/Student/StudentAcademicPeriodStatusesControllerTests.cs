//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAcademicPeriodStatusesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentAcademicPeriodStatusesService> studentAcademicPeriodStatusesServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentAcademicPeriodStatusesController studentAcademicPeriodStatusesController;
        private IEnumerable<Domain.Student.Entities.StudentStatus> allStudentTermStatuses;
        private List<Dtos.StudentAcademicPeriodStatuses> studentAcademicPeriodStatusesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private QueryStringFilter queryStringFilter;
        private string contextSuffix = "criteria";

        [TestInitialize]
        public void Initialize()
        {
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            queryStringFilter = new QueryStringFilter(contextSuffix, "");
            studentAcademicPeriodStatusesServiceMock = new Mock<IStudentAcademicPeriodStatusesService>();
            loggerMock = new Mock<ILogger>();
            studentAcademicPeriodStatusesCollection = new List<Dtos.StudentAcademicPeriodStatuses>();

            allStudentTermStatuses = new List<Domain.Student.Entities.StudentStatus>()
                {
                    new Domain.Student.Entities.StudentStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "P", "Preregistered", "P"),
                    new Domain.Student.Entities.StudentStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "R", "Registered", "R"),
                    new Domain.Student.Entities.StudentStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "T", "Transcripted", "T"),
                    new Domain.Student.Entities.StudentStatus("e2253ac7-9931-4560-b42f-1fccd43c952e", "W", "Withdrawn", "W"),
                    new Domain.Student.Entities.StudentStatus("f2253ac7-9931-4560-b42f-1fccd43c952e", "X", "Deleted", "X"),
                };

            foreach (var source in allStudentTermStatuses)
            {
                var studentAcademicPeriodStatuses = new Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                studentAcademicPeriodStatusesCollection.Add(studentAcademicPeriodStatuses);
            }

            studentAcademicPeriodStatusesController = new StudentAcademicPeriodStatusesController(studentAcademicPeriodStatusesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentAcademicPeriodStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAcademicPeriodStatusesController = null;
            allStudentTermStatuses = null;
            studentAcademicPeriodStatusesCollection = null;
            loggerMock = null;
            studentAcademicPeriodStatusesServiceMock = null;
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_ValidateFields_Nocache()
        {
            studentAcademicPeriodStatusesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false)).ReturnsAsync(studentAcademicPeriodStatusesCollection);
           

           var sourceContexts = (await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter)).ToList();
            Assert.AreEqual(studentAcademicPeriodStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = studentAcademicPeriodStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_ValidateFields_Cache()
        {
            studentAcademicPeriodStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(true)).ReturnsAsync(studentAcademicPeriodStatusesCollection);

            var sourceContexts = (await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter)).ToList();
            Assert.AreEqual(studentAcademicPeriodStatusesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = studentAcademicPeriodStatusesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_KeyNotFoundException()
        {
            //
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false))
                .Throws<KeyNotFoundException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_PermissionsException()
        {

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false))
                .Throws<PermissionsException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_ArgumentException()
        {

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false))
                .Throws<ArgumentException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_RepositoryException()
        {

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false))
                .Throws<RepositoryException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_IntegrationApiException()
        {

            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false))
                .Throws<IntegrationApiException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuidAsync_ValidateFields()
        {
            var expected = studentAcademicPeriodStatusesCollection.FirstOrDefault();
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatuses_Exception()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesAsync(false)).Throws<Exception>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesAsync(queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuidAsync_Exception()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_KeyNotFoundException()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_PermissionsException()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_ArgumentException()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_RepositoryException()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_IntegrationApiException()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_GetStudentAcademicPeriodStatusesByGuid_Exception()
        {
            studentAcademicPeriodStatusesServiceMock.Setup(x => x.GetStudentAcademicPeriodStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await studentAcademicPeriodStatusesController.GetStudentAcademicPeriodStatusesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_PostStudentAcademicPeriodStatusesAsync_Exception()
        {
            await studentAcademicPeriodStatusesController.PostStudentAcademicPeriodStatusesAsync(studentAcademicPeriodStatusesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_PutStudentAcademicPeriodStatusesAsync_Exception()
        {
            var sourceContext = studentAcademicPeriodStatusesCollection.FirstOrDefault();
            await studentAcademicPeriodStatusesController.PutStudentAcademicPeriodStatusesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAcademicPeriodStatusesController_DeleteStudentAcademicPeriodStatusesAsync_Exception()
        {
            await studentAcademicPeriodStatusesController.DeleteStudentAcademicPeriodStatusesAsync(studentAcademicPeriodStatusesCollection.FirstOrDefault().Id);
        }
    }
}