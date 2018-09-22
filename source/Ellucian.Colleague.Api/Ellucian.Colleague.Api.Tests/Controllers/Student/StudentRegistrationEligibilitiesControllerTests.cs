//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.Models;
using System.Threading;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentRegistrationEligibilitiesControllerTests_V9
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }

        private Mock<IStudentRegistrationEligibilitiesService> studentRegistrationEligibilitiesServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentRegistrationEligibilitiesController studentRegistrationEligibilitiesController;
        private IEnumerable<Domain.Student.Entities.RegistrationEligibility> allRegistrationEligibility;
        private Dtos.StudentRegistrationEligibilities studentRegistrationEligibilities;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentRegistrationEligibilitiesServiceMock = new Mock<IStudentRegistrationEligibilitiesService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            studentRegistrationEligibilitiesController = new StudentRegistrationEligibilitiesController(studentRegistrationEligibilitiesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentRegistrationEligibilitiesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        private void BuildData()
        {
            studentRegistrationEligibilities = new Ellucian.Colleague.Dtos.StudentRegistrationEligibilities()
            {
                AcademicPeriod = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                Student = new GuidObject2("1b2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                AlternatePin = Dtos.EnumProperties.StudentRegistrationEligibilitiesAlternatePin.Required,
                EligibilityStatus = Dtos.EnumProperties.StudentRegistrationEligibilitiesEligibilityStatus.Eligible,
                StartOn = DateTime.Today
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentRegistrationEligibilitiesController = null;
            allRegistrationEligibility = null;
            studentRegistrationEligibilities = null;
            loggerMock = null;
            studentRegistrationEligibilitiesServiceMock = null;
        }

        #endregion

        #region EXCEPTIONS

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_KeyNotFoundException()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<KeyNotFoundException>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_PermissionsException()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<PermissionsException>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_ArgumentException()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<ArgumentException>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_RepositoryException()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<RepositoryException>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_IntegrationApiException()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<IntegrationApiException>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_Exception()
        {
            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Throws<Exception>();
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());
        }

        #endregion

        [TestMethod]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_ValidateFields_Nocache()
        {
            studentRegistrationEligibilitiesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false };

            studentRegistrationEligibilitiesServiceMock.Setup(m => m.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), false))
                .ReturnsAsync(studentRegistrationEligibilities);

            var result = await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());

            Assert.IsNotNull(result);

            Assert.AreEqual(studentRegistrationEligibilities.Student.Id, result.Student.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AcademicPeriod.Id, result.AcademicPeriod.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AlternatePin, result.AlternatePin);
            Assert.AreEqual(studentRegistrationEligibilities.EligibilityStatus, result.EligibilityStatus);
        }

        [TestMethod]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_ValidateFields_Cache()
        {
            studentRegistrationEligibilitiesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(studentRegistrationEligibilities);

            var result = await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(It.IsAny<QueryStringFilter>());

            Assert.IsNotNull(result);

            Assert.AreEqual(studentRegistrationEligibilities.Student.Id, result.Student.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AcademicPeriod.Id, result.AcademicPeriod.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AlternatePin, result.AlternatePin);
            Assert.AreEqual(studentRegistrationEligibilities.EligibilityStatus, result.EligibilityStatus);
        }

        [TestMethod]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilities_With_Filters()
        {
            var filterGroupName = "criteria";

            studentRegistrationEligibilitiesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            studentRegistrationEligibilitiesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };

            studentRegistrationEligibilitiesController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.StudentRegistrationEligibilities() { Student = new GuidObject2("3a082180-b897-46f3-8435-df25caaca921") });

            studentRegistrationEligibilitiesServiceMock.Setup(x => x.GetStudentRegistrationEligibilitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentRegistrationEligibilities);

            QueryStringFilter student = new QueryStringFilter("student", "{'student':{'id': '3a082180-b897-46f3-8435-df25caaca921'}}");

            var result = await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesAsync(student);

            Assert.IsNotNull(result);

            Assert.AreEqual(studentRegistrationEligibilities.Student.Id, result.Student.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AcademicPeriod.Id, result.AcademicPeriod.Id);
            Assert.AreEqual(studentRegistrationEligibilities.AlternatePin, result.AlternatePin);
            Assert.AreEqual(studentRegistrationEligibilities.EligibilityStatus, result.EligibilityStatus);
        }

        #region NOT SUPPORTED

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_GetStudentRegistrationEligibilitiesByGuidAsync_Exception()
        {
            await studentRegistrationEligibilitiesController.GetStudentRegistrationEligibilitiesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_PostStudentRegistrationEligibilitiesAsync_Exception()
        {
            await studentRegistrationEligibilitiesController.PostStudentRegistrationEligibilitiesAsync(studentRegistrationEligibilities);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_PutStudentRegistrationEligibilitiesAsync_Exception()
        {
            await studentRegistrationEligibilitiesController.PutStudentRegistrationEligibilitiesAsync(Guid.NewGuid().ToString(), studentRegistrationEligibilities);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentRegistrationEligibilitiesController_DeleteStudentRegistrationEligibilitiesAsync_Exception()
        {
            await studentRegistrationEligibilitiesController.DeleteStudentRegistrationEligibilitiesAsync(Guid.NewGuid().ToString());
        }

        #endregion
    }
}