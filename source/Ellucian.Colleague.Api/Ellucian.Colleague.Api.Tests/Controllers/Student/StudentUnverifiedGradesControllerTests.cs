//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentUnverifiedGradesControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IStudentUnverifiedGradesService> _studentUnverifiedGradesServiceMock;
        private Mock<ILogger> _loggerMock;

        private StudentUnverifiedGradesController _studentUnverifiedGradesController;

        private List<Dtos.StudentUnverifiedGrades> _studentUnverifiedGradesCollection;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string StudentUnverifiedGradesGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _studentUnverifiedGradesServiceMock = new Mock<IStudentUnverifiedGradesService>();
            _loggerMock = new Mock<ILogger>();

            _studentUnverifiedGradesCollection = new List<Dtos.StudentUnverifiedGrades>();

            var studentUnverifiedGrades = new Ellucian.Colleague.Dtos.StudentUnverifiedGrades
            {
                Id = StudentUnverifiedGradesGuid,
                Student = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438911"),
                SectionRegistration = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438922"),
            };

            _studentUnverifiedGradesCollection.Add(studentUnverifiedGrades);

            _studentUnverifiedGradesController = new StudentUnverifiedGradesController(_studentUnverifiedGradesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentUnverifiedGradesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentUnverifiedGradesController = null;
            _studentUnverifiedGradesCollection = null;
            _loggerMock = null;
            _studentUnverifiedGradesServiceMock = null;
        }

        #region GET ALL

        [TestMethod]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync()
        {
            _studentUnverifiedGradesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentUnverifiedGradesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var tuple = new Tuple<IEnumerable<Dtos.StudentUnverifiedGrades>, int>(_studentUnverifiedGradesCollection, 1);

            _studentUnverifiedGradesServiceMock
                .Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var studentUnverifiedGrades = await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(new Paging(10, 0), new QueryStringFilter("criteria", "{'student':{'id':'123'}}"), new QueryStringFilter("section", "{'section':{'id':'123'}}"));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await studentUnverifiedGrades.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentUnverifiedGrades>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentUnverifiedGrades>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentUnverifiedGradesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Student.Id, actual.Student.Id);
                Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_PermissionsException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_KeyNotFoundException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_ArgumentNullException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_ArgumentException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_RepositoryException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesAsync_Exception()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesAsync(paging, queryStringFilter, queryStringFilter);
        }

        #endregion

        #region GETBYID

        [TestMethod]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid()
        {
            _studentUnverifiedGradesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = _studentUnverifiedGradesCollection.FirstOrDefault(x => x.Id.Equals(StudentUnverifiedGradesGuid, StringComparison.OrdinalIgnoreCase));

            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Student.Id, actual.Student.Id);
            Assert.AreEqual(expected.SectionRegistration.Id, actual.SectionRegistration.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_GuidAsNull()
        {
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()));
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_KeyNotFoundException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_PermissionsException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_ArgumentNullException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_ArgumentException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_RepositoryException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentUnverifiedGradesController_GetStudentUnverifiedGradesByGuid_Exception()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentUnverifiedGradesServiceMock.Setup(x => x.GetStudentUnverifiedGradesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _studentUnverifiedGradesController.GetStudentUnverifiedGradesByGuidAsync(StudentUnverifiedGradesGuid);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_PutStudentUnverifiedGrades_Exception()
        {
            var expected = _studentUnverifiedGradesCollection.FirstOrDefault();
            await _studentUnverifiedGradesController.PutStudentUnverifiedGradesAsync(expected.Id, expected);
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_PostStudentUnverifiedGrades_Exception()
        {
            var expected = _studentUnverifiedGradesCollection.FirstOrDefault();
            await _studentUnverifiedGradesController.PostStudentUnverifiedGradesAsync(expected);
        }

        #endregion

        #region DELETE

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_DeleteStudentUnverifiedGrades_Exception()
        {
            var expected = _studentUnverifiedGradesCollection.FirstOrDefault();
            await _studentUnverifiedGradesController.DeleteStudentUnverifiedGradesAsync(expected.Id);
        }

        #endregion
    }
}
