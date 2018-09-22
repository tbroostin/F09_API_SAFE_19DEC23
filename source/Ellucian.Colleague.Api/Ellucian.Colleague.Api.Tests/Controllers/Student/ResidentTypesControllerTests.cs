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
    public class ResidentTypesControllerTests
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
            Mock<IStudentRepository> studentRepository;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            ResidentTypesController residentTypesController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ResidencyStatus> residentTypesEntities;
            List<Dtos.ResidentType> residentTypesDto = new List<Dtos.ResidentType>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentRepository = new Mock<IStudentRepository>();
                studentServiceMock = new Mock<IStudentService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                residentTypesEntities = new TestStudentRepository().GetResidencyStatusesAsync().Result;
                foreach (var residentTypeEntity in residentTypesEntities)
                {
                    Dtos.ResidentType residentTypeDto = new Dtos.ResidentType();
                    residentTypeDto.Id = residentTypeEntity.Guid;
                    residentTypeDto.Code = residentTypeEntity.Code;
                    residentTypeDto.Title = residentTypeEntity.Description;
                    residentTypeDto.Description = string.Empty;
                    residentTypesDto.Add(residentTypeDto);
                }

                residentTypesController = new ResidentTypesController(adapterRegistryMock.Object, studentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                residentTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                residentTypesController = null;
                residentTypesEntities = null;
                residentTypesDto = null;
            }

            [TestMethod]
            public async Task ResidentTypesController_GetAll_NoCache_True()
            {
                residentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                studentServiceMock.Setup(ac => ac.GetResidentTypesAsync(It.IsAny<bool>())).ReturnsAsync(residentTypesDto);

                var results = await residentTypesController.GetResidentTypesAsync();
                Assert.AreEqual(residentTypesDto.Count, results.Count());

                foreach (var residentTypeDto in residentTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == residentTypeDto.Id);
                    Assert.AreEqual(result.Id, residentTypeDto.Id);
                    Assert.AreEqual(result.Code, residentTypeDto.Code);
                    Assert.AreEqual(result.Title, residentTypeDto.Title);
                    Assert.AreEqual(result.Description, residentTypeDto.Description);
                }
            }

            [TestMethod]
            public async Task ResidentTypesController_GetAll_NoCache_False()
            {
                residentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                studentServiceMock.Setup(ac => ac.GetResidentTypesAsync(It.IsAny<bool>())).ReturnsAsync(residentTypesDto);

                var results = await residentTypesController.GetResidentTypesAsync();
                Assert.AreEqual(residentTypesDto.Count, results.Count());                
                
                foreach (var residentTypeDto in residentTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == residentTypeDto.Id);
                    Assert.AreEqual(result.Id, residentTypeDto.Id);
                    Assert.AreEqual(result.Code, residentTypeDto.Code);
                    Assert.AreEqual(result.Title, residentTypeDto.Title);
                    Assert.AreEqual(result.Description, residentTypeDto.Description);
                }

            }

            [TestMethod]
            public async Task ResidentTypesController_GetById()
            {
                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var residentTypeDto = residentTypesDto.FirstOrDefault(i => i.Id == id);
                residentTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                studentServiceMock.Setup(ac => ac.GetResidentTypeByIdAsync(id)).ReturnsAsync(residentTypeDto);

                var result = await residentTypesController.GetResidentTypeByIdAsync(id);
                Assert.AreEqual(result.Id, residentTypeDto.Id);
                Assert.AreEqual(result.Code, residentTypeDto.Code);
                Assert.AreEqual(result.Title, residentTypeDto.Title);
                Assert.AreEqual(result.Description, residentTypeDto.Description);
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
            public async Task ResidentTypesController_GetById_Exception()
            {
                studentServiceMock.Setup(ac => ac.GetResidentTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await residentTypesController.GetResidentTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_Exception()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<Exception>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_IntegrationApiException()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypeByIdAsync_PermissionsException()
            {
                var expected = residentTypesDto.FirstOrDefault();
                studentServiceMock.Setup(x => x.GetResidentTypeByIdAsync(expected.Id)).Throws<PermissionsException>();
                Debug.Assert(expected != null, "expected != null");
                await residentTypesController.GetResidentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_PermissionsException()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypeByIdAsync_KeyNotFoundException()
            {
                var expected = residentTypesDto.FirstOrDefault();
                studentServiceMock.Setup(x => x.GetResidentTypeByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
                Debug.Assert(expected != null, "expected != null");
                await residentTypesController.GetResidentTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_KeyNotFoundException()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_ArgumentNullException()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypeController_GetResidentTypesAsync_RepositoryException()
            {
                studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
                await residentTypesController.GetResidentTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypesController_PUT_Exception()
            {
                var result = await residentTypesController.PutResidentTypeAsync(new Dtos.ResidentType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypesController_POST_Exception()
            {
                var result = await residentTypesController.PostResidentTypeAsync(new Dtos.ResidentType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResidentTypesController_DELETE_Exception()
            {
                await residentTypesController.DeleteResidentTypeAsync(It.IsAny<string>());
            }
        }
    }
}
