// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using StudentClassification = Ellucian.Colleague.Dtos.StudentClassification;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentClassificationsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IStudentService> studentServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentClassificationsController studentClassificationsController;
            List<Dtos.StudentClassification> studentClassifications;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentServiceMock = new Mock<IStudentService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                studentClassificationsController = new StudentClassificationsController(adapterRegistryMock.Object, studentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentClassificationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private void BuildData()
            {
                studentClassifications = new List<StudentClassification>() 
                {
                    new StudentClassification(){ Id = "3b8f02a3-d349-46b5-a0df-710121fa1f64", Code = "1G", Description = "First Year Graduate", Title = "First Year Graduate" },
                    new StudentClassification(){ Id = "7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", Code = "1L", Description = "First Year Law", Title = "First Year Law" },
                    new StudentClassification(){ Id = "bd98c3ed-6adb-4c7c-bc80-7507ea868a23", Code = "2A", Description = "Second Year", Title = "Second Year" },
                    new StudentClassification(){ Id = "6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", Code = "2G", Description = "Second Year Graduate", Title = "Second Year Graduate" },
                    new StudentClassification(){ Id = "7e990bda-9427-4de6-b0ef-bba9b015e399", Code = "2L", Description = "Second Year Law", Title = "Second Year Law" }
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentClassificationsController = null;
                studentClassifications = null;
                studentServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentClassificationsController_GetAll_NoCache_True()
            {
                studentClassificationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                studentServiceMock.Setup(i => i.GetAllStudentClassificationsAsync(true)).ReturnsAsync(studentClassifications);

                var actuals = await studentClassificationsController.GetStudentClassificationsAsync();

                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassifications.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task StudentClassificationsController_GetAll_NoCache_False()
            {
                studentClassificationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                studentServiceMock.Setup(i => i.GetAllStudentClassificationsAsync(false)).ReturnsAsync(studentClassifications);

                var actuals = await studentClassificationsController.GetStudentClassificationsAsync();

                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassifications.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task StudentClassificationsController_GetById()
            {
                string id = "7b8c4ba7-ea28-4604-bca7-da7223f6e2b3";
                var expected = studentClassifications.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                studentServiceMock.Setup(i => i.GetStudentClassificationByGuidAsync(id)).ReturnsAsync(expected);

                var actual = await studentClassificationsController.GetStudentClassificationByIdAsync(id);

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_GetAll_Exception()
            {
                studentServiceMock.Setup(i => i.GetAllStudentClassificationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await studentClassificationsController.GetStudentClassificationsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_GetById_Exception()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                studentServiceMock.Setup(i => i.GetStudentClassificationByGuidAsync(id)).ThrowsAsync(new Exception());

                var actual = await studentClassificationsController.GetStudentClassificationByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_GetById_KeyNotFoundException()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                studentServiceMock.Setup(i => i.GetStudentClassificationByGuidAsync(id)).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentClassificationsController.GetStudentClassificationByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_PUT_Not_Supported()
            {
                var actual = await studentClassificationsController.PutStudentClassificationAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentClassification>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_POST_Not_Supported()
            {
                var actual = await studentClassificationsController.PostStudentClassificationAsync(It.IsAny<Dtos.StudentClassification>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentClassificationsController_DELETE_Not_Supported()
            {
                await studentClassificationsController.DeleteStudentClassificationAsync(It.IsAny<string>());
            }
        }
    }
}