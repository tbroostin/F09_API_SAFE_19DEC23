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
    public class ResidencyTypesControllerTests
    {

        public TestContext TestContext { get; set; }

        Mock<IStudentService> studentServiceMock;
        Mock<IStudentRepository> studentRepository;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;

        ResidencyTypesController residencyTypesController;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AdmissionResidencyType> residencyTypesEntities;
        List<Dtos.ResidentType> residencyTypesDto = new List<Dtos.ResidentType>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentRepository = new Mock<IStudentRepository>();
            studentServiceMock = new Mock<IStudentService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            residencyTypesEntities = new TestStudentRepository().GetResidencyStatusesAsync().Result;
            foreach (var residencyTypeEntity in residencyTypesEntities)
            {
                Dtos.ResidentType residencyTypeDto = new Dtos.ResidentType();
                residencyTypeDto.Id = residencyTypeEntity.Guid;
                residencyTypeDto.Code = residencyTypeEntity.Code;
                residencyTypeDto.Title = residencyTypeEntity.Description;
                residencyTypeDto.Description = string.Empty;
                residencyTypesDto.Add(residencyTypeDto);
            }

            residencyTypesController = new ResidencyTypesController(adapterRegistryMock.Object, studentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            residencyTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
            new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            residencyTypesController = null;
            residencyTypesEntities = null;
            residencyTypesDto = null;
        }

        [TestMethod]
        public async Task ResidencyTypesController_GetAll_NoCache_True()
        {
            residencyTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            studentServiceMock.Setup(ac => ac.GetResidentTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypesDto);

            var results = await residencyTypesController.GetResidencyTypesAsync();
            Assert.AreEqual(residencyTypesDto.Count, results.Count());

            foreach (var residencyTypeDto in residencyTypesDto)
            {
                var result = results.FirstOrDefault(i => i.Id == residencyTypeDto.Id);
                Assert.AreEqual(result.Id, residencyTypeDto.Id);
                Assert.AreEqual(result.Code, residencyTypeDto.Code);
                Assert.AreEqual(result.Title, residencyTypeDto.Title);
                Assert.AreEqual(result.Description, residencyTypeDto.Description);
            }
        }

        [TestMethod]
        public async Task ResidencyTypesController_GetAll_NoCache_False()
        {
            residencyTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };

            studentServiceMock.Setup(ac => ac.GetResidentTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypesDto);

            var results = await residencyTypesController.GetResidencyTypesAsync();
            Assert.AreEqual(residencyTypesDto.Count, results.Count());

            foreach (var residencyTypeDto in residencyTypesDto)
            {
                var result = results.FirstOrDefault(i => i.Id == residencyTypeDto.Id);
                Assert.AreEqual(result.Id, residencyTypeDto.Id);
                Assert.AreEqual(result.Code, residencyTypeDto.Code);
                Assert.AreEqual(result.Title, residencyTypeDto.Title);
                Assert.AreEqual(result.Description, residencyTypeDto.Description);
            }

        }

        [TestMethod]
        public async Task residencyTypesController_GetById()
        {
            string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
            var residencyTypeDto = residencyTypesDto.FirstOrDefault(i => i.Id == id);
            residencyTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };

            studentServiceMock.Setup(ac => ac.GetResidentTypeByIdAsync(id)).ReturnsAsync(residencyTypeDto);

            var result = await residencyTypesController.GetResidencyTypeByIdAsync(id);
            Assert.AreEqual(result.Id, residencyTypeDto.Id);
            Assert.AreEqual(result.Code, residencyTypeDto.Code);
            Assert.AreEqual(result.Title, residencyTypeDto.Title);
            Assert.AreEqual(result.Description, residencyTypeDto.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ResidentTypesController_GetById_Exception()
        {
            studentServiceMock.Setup(ac => ac.GetResidentTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var result = await residencyTypesController.GetResidencyTypeByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_Exception()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<Exception>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_IntegrationApiException()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypeByIdAsync_PermissionsException()
        {
            var expected = residencyTypesDto.FirstOrDefault();
            studentServiceMock.Setup(x => x.GetResidentTypeByIdAsync(expected.Id)).Throws<PermissionsException>();
            Debug.Assert(expected != null, "expected != null");
            await residencyTypesController.GetResidencyTypeByIdAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_PermissionsException()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypeByIdAsync_KeyNotFoundException()
        {
            var expected = residencyTypesDto.FirstOrDefault();
            studentServiceMock.Setup(x => x.GetResidentTypeByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
            Debug.Assert(expected != null, "expected != null");
            await residencyTypesController.GetResidencyTypeByIdAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_KeyNotFoundException()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_ArgumentNullException()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypeController_GetresidencyTypesAsync_RepositoryException()
        {
            studentServiceMock.Setup(s => s.GetResidentTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
            await residencyTypesController.GetResidencyTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypesController_PUT_Exception()
        {
            var result = await residencyTypesController.PutResidencyTypeAsync(new Dtos.ResidentType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypesController_POST_Exception()
        {
            var result = await residencyTypesController.PostResidencyTypeAsync(new Dtos.ResidentType());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task residencyTypesController_DELETE_Exception()
        {
            await residencyTypesController.DeleteResidencyTypeAsync(It.IsAny<string>());
        }

    }
}
