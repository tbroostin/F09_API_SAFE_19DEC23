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
    public class StudentFinancialAidAcademicProgressStatusesControllerTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IStudentFinancialAidAcademicProgressStatusesService> _studentFinancialAidAcademicProgressStatusesServiceMock;
        private Mock<ILogger> _loggerMock;

        private StudentFinancialAidAcademicProgressStatusesController _studentFinancialAidAcademicProgressStatusesController;

        private List<Dtos.StudentFinancialAidAcademicProgressStatuses> _studentFinancialAidAcademicProgressStatusesCollection;
        private readonly DateTime _currentDate = DateTime.Now;
        private const string StudentFinancialAidAcademicProgressStatusesGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _studentFinancialAidAcademicProgressStatusesServiceMock = new Mock<IStudentFinancialAidAcademicProgressStatusesService>();
            _loggerMock = new Mock<ILogger>();

            _studentFinancialAidAcademicProgressStatusesCollection = new List<Dtos.StudentFinancialAidAcademicProgressStatuses>();

            var studentFinancialAidAcademicProgressStatuses = new Ellucian.Colleague.Dtos.StudentFinancialAidAcademicProgressStatuses
            {
                Id = StudentFinancialAidAcademicProgressStatusesGuid,
                AssignedOn = DateTime.Now,
                EffectiveOn = DateTime.Now,
                Person = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438911"),
                ProgressType = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438922"),
                Status = new Dtos.GuidObject2("a830e686-7692-4012-8da5-b1b5d4438933"),
            };

            _studentFinancialAidAcademicProgressStatusesCollection.Add(studentFinancialAidAcademicProgressStatuses);

            _studentFinancialAidAcademicProgressStatusesController = new StudentFinancialAidAcademicProgressStatusesController(_studentFinancialAidAcademicProgressStatusesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentFinancialAidAcademicProgressStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentFinancialAidAcademicProgressStatusesController = null;
            _studentFinancialAidAcademicProgressStatusesCollection = null;
            _loggerMock = null;
            _studentFinancialAidAcademicProgressStatusesServiceMock = null;
        }

        #region GET ALL

        [TestMethod]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync()
        {
            _studentFinancialAidAcademicProgressStatusesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentFinancialAidAcademicProgressStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var tuple = new Tuple<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>, int>(_studentFinancialAidAcademicProgressStatusesCollection, 1);

            _studentFinancialAidAcademicProgressStatusesServiceMock
                .Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(),It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .ReturnsAsync(tuple);

            var studentFinancialAidAcademicProgressStatuses = await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(new Paging(10, 0), new QueryStringFilter("criteria", "{'person':{'id':'123'}}") );

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await studentFinancialAidAcademicProgressStatuses.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAcademicProgressStatuses>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentFinancialAidAcademicProgressStatusesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.AssignedOn, actual.AssignedOn);
                Assert.AreEqual(expected.EffectiveOn, actual.EffectiveOn);
                Assert.AreEqual(expected.ProgressType, actual.ProgressType);
                Assert.AreEqual(expected.Status, actual.Status);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_PermissionsException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_KeyNotFoundException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_ArgumentNullException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_ArgumentException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_RepositoryException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesAsync_Exception()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.StudentFinancialAidAcademicProgressStatuses>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesAsync(paging, queryStringFilter);
        }

        #endregion

        #region GETBYID

        [TestMethod]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid()
        {
            _studentFinancialAidAcademicProgressStatusesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = _studentFinancialAidAcademicProgressStatusesCollection.FirstOrDefault(x => x.Id.Equals(StudentFinancialAidAcademicProgressStatusesGuid, StringComparison.OrdinalIgnoreCase));

            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.AssignedOn, actual.AssignedOn);
            Assert.AreEqual(expected.EffectiveOn, actual.EffectiveOn);
            Assert.AreEqual(expected.ProgressType, actual.ProgressType);
            Assert.AreEqual(expected.Status, actual.Status);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_GuidAsNull()
        {
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()));
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_KeyNotFoundException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_PermissionsException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_ArgumentNullException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_ArgumentException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_RepositoryException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_IntegrationApiException()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_GetStudentFinancialAidAcademicProgressStatusesByGuid_Exception()
        {
            var paging = new Paging(100, 0);
            var queryStringFilter = new QueryStringFilter("", "");
            _studentFinancialAidAcademicProgressStatusesServiceMock.Setup(x => x.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await _studentFinancialAidAcademicProgressStatusesController.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(StudentFinancialAidAcademicProgressStatusesGuid);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_PutStudentFinancialAidAcademicProgressStatuses_Exception()
        {
            var expected = _studentFinancialAidAcademicProgressStatusesCollection.FirstOrDefault();
            await _studentFinancialAidAcademicProgressStatusesController.PutStudentFinancialAidAcademicProgressStatusesAsync(expected.Id, expected);
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_PostStudentFinancialAidAcademicProgressStatuses_Exception()
        {
            var expected = _studentFinancialAidAcademicProgressStatusesCollection.FirstOrDefault();
            await _studentFinancialAidAcademicProgressStatusesController.PostStudentFinancialAidAcademicProgressStatusesAsync(expected);
        }

        #endregion

        #region DELETE

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFAAPSController_DeleteStudentFinancialAidAcademicProgressStatuses_Exception()
        {
            var expected = _studentFinancialAidAcademicProgressStatusesCollection.FirstOrDefault();
            await _studentFinancialAidAcademicProgressStatusesController.DeleteStudentFinancialAidAcademicProgressStatusesAsync(expected.Id);
        }

        #endregion
    }
}
