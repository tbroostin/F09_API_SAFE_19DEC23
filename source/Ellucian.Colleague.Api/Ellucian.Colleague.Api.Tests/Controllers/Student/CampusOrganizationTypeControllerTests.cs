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
    public class CampusOrganizationTypeControllerTests
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

            CampusOrganizationTypesController campusOrganizationTypesController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CampusOrganizationType> campusOrganizationTypesEntities;
            List<Dtos.CampusOrganizationType> campusOrganizationTypesDto = new List<Dtos.CampusOrganizationType>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                campusOrganizationServiceMock = new Mock<ICampusOrganizationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                campusOrganizationTypesEntities = new TestStudentReferenceDataRepository().GetCampusOrganizationTypesAsync(false).Result;
                foreach (var campusOrgTypesEntity in campusOrganizationTypesEntities)
                {
                    Dtos.CampusOrganizationType campusOrgTypeDto = new Dtos.CampusOrganizationType();
                    campusOrgTypeDto.Id = campusOrgTypesEntity.Guid;
                    campusOrgTypeDto.Code = campusOrgTypesEntity.Code;
                    campusOrgTypeDto.Title = campusOrgTypesEntity.Description;
                    campusOrgTypeDto.Description = string.Empty;
                    campusOrganizationTypesDto.Add(campusOrgTypeDto);
                }

                campusOrganizationTypesController = new CampusOrganizationTypesController(adapterRegistryMock.Object, campusOrganizationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                campusOrganizationTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationTypesController = null;
                campusOrganizationTypesEntities = null;
                campusOrganizationTypesDto = null;
            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetAll_NoCache_True()
            {
                campusOrganizationTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypesAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationTypesDto);

                var results = await campusOrganizationTypesController.GetCampusOrganizationTypesAsync();
                Assert.AreEqual(campusOrganizationTypesDto.Count, results.Count());

                foreach (var campusOrganizationTypeDto in campusOrganizationTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusOrganizationTypeDto.Id);
                    Assert.AreEqual(result.Id, campusOrganizationTypeDto.Id);
                    Assert.AreEqual(result.Code, campusOrganizationTypeDto.Code);
                    Assert.AreEqual(result.Title, campusOrganizationTypeDto.Title);
                    Assert.AreEqual(result.Description, campusOrganizationTypeDto.Description);
                }
            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetAll_NoCache_False()
            {
                campusOrganizationTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypesAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationTypesDto);

                var results = await campusOrganizationTypesController.GetCampusOrganizationTypesAsync();
                Assert.AreEqual(campusOrganizationTypesDto.Count, results.Count());                
                
                foreach (var campusOrganizationTypeDto in campusOrganizationTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusOrganizationTypeDto.Id);
                    Assert.AreEqual(result.Id, campusOrganizationTypeDto.Id);
                    Assert.AreEqual(result.Code, campusOrganizationTypeDto.Code);
                    Assert.AreEqual(result.Title, campusOrganizationTypeDto.Title);
                    Assert.AreEqual(result.Description, campusOrganizationTypeDto.Description);
                }

            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetById()
            {
                string id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
                var campusOrganizationTypeDto = campusOrganizationTypesDto.FirstOrDefault(i => i.Id == id);
                campusOrganizationTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypeByGuidAsync(id)).ReturnsAsync(campusOrganizationTypeDto);

                var result = await campusOrganizationTypesController.GetCampusOrganizationTypeByIdAsync(id);
                Assert.AreEqual(result.Id, campusOrganizationTypeDto.Id);
                Assert.AreEqual(result.Code, campusOrganizationTypeDto.Code);
                Assert.AreEqual(result.Title, campusOrganizationTypeDto.Title);
                Assert.AreEqual(result.Description, campusOrganizationTypeDto.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetAll_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var result = await campusOrganizationTypesController.GetCampusOrganizationTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetById_KeyNotFoundException()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypeByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var result = await campusOrganizationTypesController.GetCampusOrganizationTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetById_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationTypeByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await campusOrganizationTypesController.GetCampusOrganizationTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_PUT_Exception()
            {
                var result = await campusOrganizationTypesController.PutCampusOrganizationTypeAsync(It.IsAny<string>(), new Dtos.CampusOrganizationType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_POST_Exception()
            {
                var result = await campusOrganizationTypesController.PostCampusOrganizationTypeAsync(new Dtos.CampusOrganizationType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_DELETE_Exception()
            {
                await campusOrganizationTypesController.DeleteCampusOrganizationTypeAsync(It.IsAny<string>());
            }
        }
    }
}
