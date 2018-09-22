// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentStatusesControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<ICurriculumService> curriculumServiceMock;
            Mock<IStudentReferenceDataRepository> referenceDataRepository;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentStatusesController studentStatusesController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStatus> studentStatusesEntities;
            List<Dtos.StudentStatus> studentStatusesDto = new List<Dtos.StudentStatus>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                referenceDataRepository = new Mock<IStudentReferenceDataRepository>();
                curriculumServiceMock = new Mock<ICurriculumService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                studentStatusesEntities = new TestStudentReferenceDataRepository().GetStudentStatusesAsync().Result;
                foreach (var studentStatusEntity in studentStatusesEntities)
                {
                    Dtos.StudentStatus studentStatusDto = new Dtos.StudentStatus();
                    studentStatusDto.Id = studentStatusEntity.Guid;
                    studentStatusDto.Code = studentStatusEntity.Code;
                    studentStatusDto.Title = studentStatusEntity.Description;
                    studentStatusDto.Description = string.Empty;
                    studentStatusesDto.Add(studentStatusDto);
                }

                studentStatusesController = new StudentStatusesController(adapterRegistryMock.Object, referenceDataRepository.Object, curriculumServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentStatusesController = null;
                studentStatusesEntities = null;
                studentStatusesDto = null;
            }

            [TestMethod]
            public async Task StudentStatusesController_GetAll_NoCache_True()
            {
                studentStatusesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatusesDto);

                var results = await studentStatusesController.GetStudentStatusesAsync();
                Assert.AreEqual(studentStatusesDto.Count, results.Count());

                foreach (var studentStatusDto in studentStatusesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentStatusDto.Id);
                    Assert.AreEqual(result.Id, studentStatusDto.Id);
                    Assert.AreEqual(result.Code, studentStatusDto.Code);
                    Assert.AreEqual(result.Title, studentStatusDto.Title);
                    Assert.AreEqual(result.Description, studentStatusDto.Description);
                }
            }

            [TestMethod]
            public async Task StudentStatusesController_GetAll_NoCache_False()
            {
                studentStatusesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatusesDto);

                var results = await studentStatusesController.GetStudentStatusesAsync();
                Assert.AreEqual(studentStatusesDto.Count, results.Count());                
                
                foreach (var studentStatusDto in studentStatusesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentStatusDto.Id);
                    Assert.AreEqual(result.Id, studentStatusDto.Id);
                    Assert.AreEqual(result.Code, studentStatusDto.Code);
                    Assert.AreEqual(result.Title, studentStatusDto.Title);
                    Assert.AreEqual(result.Description, studentStatusDto.Description);
                }

            }

            [TestMethod]
            public async Task StudentStatusesController_GetById()
            {
                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var studentStatusDto = studentStatusesDto.FirstOrDefault(i => i.Id == id);
                studentStatusesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentStatusByIdAsync(id)).ReturnsAsync(studentStatusDto);

                var result = await studentStatusesController.GetStudentStatusByIdAsync(id);
                Assert.AreEqual(result.Id, studentStatusDto.Id);
                Assert.AreEqual(result.Code, studentStatusDto.Code);
                Assert.AreEqual(result.Title, studentStatusDto.Title);
                Assert.AreEqual(result.Description, studentStatusDto.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetById_Exception()
            {
                curriculumServiceMock.Setup(ac => ac.GetStudentStatusByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await studentStatusesController.GetStudentStatusByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_Exception()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<Exception>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_IntegrationApiException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusByIdAsync_PermissionsException()
            {
                var expected = studentStatusesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetStudentStatusByIdAsync(expected.Id)).Throws<PermissionsException>();
                Debug.Assert(expected != null, "expected != null");
                await studentStatusesController.GetStudentStatusByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_PermissionsException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusByIdAsync_KeyNotFoundException()
            {
                var expected = studentStatusesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetStudentStatusByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
                Debug.Assert(expected != null, "expected != null");
                await studentStatusesController.GetStudentStatusByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_KeyNotFoundException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_ArgumentNullException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_GetStudentStatusesAsync_RepositoryException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentStatusesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
                await studentStatusesController.GetStudentStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_PUT_Exception()
            {
                var result = await studentStatusesController.PutStudentStatusAsync(new Dtos.StudentStatus());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_POST_Exception()
            {
                var result = await studentStatusesController.PostStudentStatusAsync(new Dtos.StudentStatus());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentStatusController_DELETE_Exception()
            {
                await studentStatusesController.DeleteStudentStatusAsync(It.IsAny<string>());
            }
        }
    }
}
