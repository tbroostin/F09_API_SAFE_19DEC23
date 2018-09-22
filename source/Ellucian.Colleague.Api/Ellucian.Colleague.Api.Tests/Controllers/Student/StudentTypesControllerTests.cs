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
    public class StudentTypesControllerTests
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

            StudentTypesController studentTypesController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentType> studentTypesEntities;
            List<Dtos.StudentType> studentTypesDto = new List<Dtos.StudentType>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                referenceDataRepository = new Mock<IStudentReferenceDataRepository>();
                curriculumServiceMock = new Mock<ICurriculumService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                studentTypesEntities = new TestStudentReferenceDataRepository().GetStudentTypesAsync().Result;
                foreach (var studentTypeEntity in studentTypesEntities)
                {
                    Dtos.StudentType studentTypeDto = new Dtos.StudentType();
                    studentTypeDto.Id = studentTypeEntity.Guid;
                    studentTypeDto.Code = studentTypeEntity.Code;
                    studentTypeDto.Title = studentTypeEntity.Description;
                    studentTypeDto.Description = string.Empty;
                    studentTypesDto.Add(studentTypeDto);
                }

                studentTypesController = new StudentTypesController(adapterRegistryMock.Object, referenceDataRepository.Object, curriculumServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentTypesController = null;
                studentTypesEntities = null;
                studentTypesDto = null;
            }

            [TestMethod]
            public async Task StudentTypesController_GetAll_NoCache_True()
            {
                studentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypesDto);

                var results = await studentTypesController.GetStudentTypesAsync();
                Assert.AreEqual(studentTypesDto.Count, results.Count());

                foreach (var studentTypeDto in studentTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentTypeDto.Id);
                    Assert.AreEqual(result.Id, studentTypeDto.Id);
                    Assert.AreEqual(result.Code, studentTypeDto.Code);
                    Assert.AreEqual(result.Title, studentTypeDto.Title);
                    Assert.AreEqual(result.Description, studentTypeDto.Description);
                }
            }

            [TestMethod]
            public async Task StudentTypesController_GetAll_NoCache_False()
            {
                studentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypesDto);

                var results = await studentTypesController.GetStudentTypesAsync();
                Assert.AreEqual(studentTypesDto.Count, results.Count());                
                
                foreach (var studentTypeDto in studentTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentTypeDto.Id);
                    Assert.AreEqual(result.Id, studentTypeDto.Id);
                    Assert.AreEqual(result.Code, studentTypeDto.Code);
                    Assert.AreEqual(result.Title, studentTypeDto.Title);
                    Assert.AreEqual(result.Description, studentTypeDto.Description);
                }

            }

            [TestMethod]
            public async Task StudentTypesController_GetById()
            {
                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var studentTypeDto = studentTypesDto.FirstOrDefault(i => i.Id == id);
                studentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetStudentTypeByIdAsync(id)).ReturnsAsync(studentTypeDto);

                var result = await studentTypesController.GetStudentTypeByIdAsync(id);
                Assert.AreEqual(result.Id, studentTypeDto.Id);
                Assert.AreEqual(result.Code, studentTypeDto.Code);
                Assert.AreEqual(result.Title, studentTypeDto.Title);
                Assert.AreEqual(result.Description, studentTypeDto.Description);
            }

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentTypesController_GetAll_Exception()
            //{
            //    curriculumServiceMock.Setup(ac => ac.GetStudentTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            //    var result = await StudentTypesController.GetStudentTypesAsync();
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task StudentTypesController_GetById_KeyNotFoundException()
            //{
            //    curriculumServiceMock.Setup(ac => ac.GetStudentTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            //    var result = await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(It.IsAny<string>());
            //}

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypesController_GetById_Exception()
            {
                curriculumServiceMock.Setup(ac => ac.GetStudentTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await studentTypesController.GetStudentTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_Exception()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<Exception>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_IntegrationApiException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypeByIdAsync_PermissionsException()
            {
                var expected = studentTypesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetStudentTypeByIdAsync(expected.Id)).Throws<PermissionsException>();
                Debug.Assert(expected != null, "expected != null");
                await studentTypesController.GetStudentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_PermissionsException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypeByIdAsync_KeyNotFoundException()
            {
                var expected = studentTypesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetStudentTypeByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
                Debug.Assert(expected != null, "expected != null");
                await studentTypesController.GetStudentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_KeyNotFoundException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_ArgumentNullException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypeController_GetStudentTypesAsync_RepositoryException()
            {
                curriculumServiceMock.Setup(s => s.GetStudentTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
                await studentTypesController.GetStudentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypesController_PUT_Exception()
            {
                var result = await studentTypesController.PutStudentTypeAsync(new Dtos.StudentType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypesController_POST_Exception()
            {
                var result = await studentTypesController.PostStudentTypeAsync(new Dtos.StudentType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTypesController_DELETE_Exception()
            {
                await studentTypesController.DeleteStudentTypeAsync(It.IsAny<string>());
            }
        }
    }
}
