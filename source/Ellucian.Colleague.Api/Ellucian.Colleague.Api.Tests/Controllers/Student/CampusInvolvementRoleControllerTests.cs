// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
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
    public class CampusInvolvementRoleControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<ICampusOrganizationService> campusOrganizationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            CampusInvolvementRolesController campusInvolvementRolesController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CampusInvRole> campusInvolvementRolesEntities;
            List<Dtos.CampusInvolvementRole> CampusInvolvementRolesDto = new List<Dtos.CampusInvolvementRole>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                campusOrganizationServiceMock = new Mock<ICampusOrganizationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                campusInvolvementRolesEntities = new TestStudentReferenceDataRepository().GetCampusInvolvementRolesAsync(false).Result;
                foreach (var campusInvRolesEntity in campusInvolvementRolesEntities)
                {
                    Dtos.CampusInvolvementRole campusInvRoleDto = new Dtos.CampusInvolvementRole();
                    campusInvRoleDto.Id = campusInvRolesEntity.Guid;
                    campusInvRoleDto.Code = campusInvRolesEntity.Code;
                    campusInvRoleDto.Title = campusInvRolesEntity.Description;
                    campusInvRoleDto.Description = string.Empty;
                    CampusInvolvementRolesDto.Add(campusInvRoleDto);
                }

                campusInvolvementRolesController = new CampusInvolvementRolesController(adapterRegistryMock.Object, campusOrganizationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                campusInvolvementRolesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusInvolvementRolesController = null;
                campusInvolvementRolesEntities = null;
                CampusInvolvementRolesDto = null;
            }

            [TestMethod]
            public async Task CampusInvolvementRolesController_GetAll_NoCache_True()
            {
                campusInvolvementRolesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRolesAsync(It.IsAny<bool>())).ReturnsAsync(CampusInvolvementRolesDto);

                var results = await campusInvolvementRolesController.GetCampusInvolvementRolesAsync();
                Assert.AreEqual(CampusInvolvementRolesDto.Count, results.Count());

                foreach (var campusInvolvementRoleDto in CampusInvolvementRolesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusInvolvementRoleDto.Id);
                    Assert.AreEqual(result.Id, campusInvolvementRoleDto.Id);
                    Assert.AreEqual(result.Code, campusInvolvementRoleDto.Code);
                    Assert.AreEqual(result.Title, campusInvolvementRoleDto.Title);
                    Assert.AreEqual(result.Description, campusInvolvementRoleDto.Description);
                }
            }

            [TestMethod]
            public async Task CampusInvolvementRolesController_GetAll_NoCache_False()
            {
                campusInvolvementRolesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRolesAsync(It.IsAny<bool>())).ReturnsAsync(CampusInvolvementRolesDto);

                var results = await campusInvolvementRolesController.GetCampusInvolvementRolesAsync();
                Assert.AreEqual(CampusInvolvementRolesDto.Count, results.Count());                
                
                foreach (var campusInvolvementRoleDto in CampusInvolvementRolesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusInvolvementRoleDto.Id);
                    Assert.AreEqual(result.Id, campusInvolvementRoleDto.Id);
                    Assert.AreEqual(result.Code, campusInvolvementRoleDto.Code);
                    Assert.AreEqual(result.Title, campusInvolvementRoleDto.Title);
                    Assert.AreEqual(result.Description, campusInvolvementRoleDto.Description);
                }

            }

            [TestMethod]
            public async Task CampusInvolvementRolesController_GetById()
            {
                string id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
                var campusInvolvementRoleDto = CampusInvolvementRolesDto.FirstOrDefault(i => i.Id == id);
                campusInvolvementRolesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRoleByGuidAsync(id)).ReturnsAsync(campusInvolvementRoleDto);

                var result = await campusInvolvementRolesController.GetCampusInvolvementRoleByIdAsync(id);
                Assert.AreEqual(result.Id, campusInvolvementRoleDto.Id);
                Assert.AreEqual(result.Code, campusInvolvementRoleDto.Code);
                Assert.AreEqual(result.Title, campusInvolvementRoleDto.Title);
                Assert.AreEqual(result.Description, campusInvolvementRoleDto.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_GetAll_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRolesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var result = await campusInvolvementRolesController.GetCampusInvolvementRolesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_GetById_KeyNotFoundException()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRoleByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var result = await campusInvolvementRolesController.GetCampusInvolvementRoleByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_GetById_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusInvolvementRoleByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await campusInvolvementRolesController.GetCampusInvolvementRoleByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_PUT_Exception()
            {
                var result = await campusInvolvementRolesController.PutCampusInvolvementRoleAsync(It.IsAny<string>(), new Dtos.CampusInvolvementRole());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_POST_Exception()
            {
                var result = await campusInvolvementRolesController.PostCampusInvolvementRoleAsync(new Dtos.CampusInvolvementRole());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementRolesController_DELETE_Exception()
            {
                await campusInvolvementRolesController.DeleteCampusInvolvementRoleAsync(It.IsAny<string>());
            }
        }
    }
}
