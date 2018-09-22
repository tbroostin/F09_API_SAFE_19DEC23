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
using CampusOrganization = Ellucian.Colleague.Dtos.CampusOrganization;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CampusOrganizationsControllerTests
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

            CampusOrganizationsController campusOrganizationsController;
            List<CampusOrganization> campusOrganizationDtos;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                campusOrganizationServiceMock = new Mock<ICampusOrganizationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                campusOrganizationDtos = BuildData();
                
                campusOrganizationsController = new CampusOrganizationsController(adapterRegistryMock.Object, campusOrganizationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                campusOrganizationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());               
            }

            private List<CampusOrganization> BuildData()
            {
                List<CampusOrganization> campusOrgs = new List<CampusOrganization>() 
                {
                    new CampusOrganization()
                    {
                        Id = "d190d4b5-03b5-41aa-99b8-b8286717c956",
                        CampusOrganizationName = "Assoc for Computing MacHinery",
                        CampusOrganizationType = new Dtos.GuidObject2("ea661349-133a-4025-86fa-68d73fbe14a5"),
                        ParentOrganization = new Dtos.GuidObject2("84efc02c-4b2e-4ad2-91fd-4688b94915e9")
                    },
                    new CampusOrganization()
                    {
                        Id = "2d37defe-6c88-4c06-bd37-17242956424e",
                        CampusOrganizationName = "Alpha Kappa Lamdba",
                        CampusOrganizationType = new Dtos.GuidObject2("606fd9cb-ca3c-4241-bb51-d760ad907788"),
                        ParentOrganization = new Dtos.GuidObject2("09035e0f-1a59-46e2-9abc-8e634ad4fdda")
                    },
                    new CampusOrganization()
                    {
                        Id = "cecdce5a-54a7-45fb-a975-5392a579e5bf",
                        CampusOrganizationName = "Art Club",
                        CampusOrganizationType = new Dtos.GuidObject2("143e48c3-80b3-41de-bf85-ef189a2615c8")
                    },
                    new CampusOrganization()
                    {
                        Id = "038179c8-8d34-4c94-99e8-e2a53bca0305",
                        CampusOrganizationName = "Bacon Lovers Of Ellucian Univ",
                        CampusOrganizationType = new Dtos.GuidObject2("462f0d57-563b-4807-b2b4-cac4df1f874c"),
                        ParentOrganization = new Dtos.GuidObject2("13e50284-676f-4df4-90be-1432c34dfe40")
                    },
                };
                return campusOrgs;
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationsController = null;
                campusOrganizationDtos = null;
            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetAll_NoCache_True()
            {
                campusOrganizationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationsAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationDtos);

                var results = await campusOrganizationsController.GetCampusOrganizationsAsync();
                Assert.AreEqual(campusOrganizationDtos.Count, results.Count());

                foreach (var campusOrganizationDto in campusOrganizationDtos)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusOrganizationDto.Id);
                    Assert.AreEqual(result.Id, campusOrganizationDto.Id);
                    Assert.AreEqual(result.CampusOrganizationName, campusOrganizationDto.CampusOrganizationName);
                    Assert.AreEqual(result.CampusOrganizationType, campusOrganizationDto.CampusOrganizationType);
                    Assert.AreEqual(result.ParentOrganization, campusOrganizationDto.ParentOrganization);
                }
            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetAll_WithCache_False()
            {
                campusOrganizationsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationsAsync(It.IsAny<bool>())).ReturnsAsync(campusOrganizationDtos);

                var results = await campusOrganizationsController.GetCampusOrganizationsAsync();
                Assert.AreEqual(campusOrganizationDtos.Count, results.Count());

                foreach (var campusOrganizationDto in campusOrganizationDtos)
                {
                    var result = results.FirstOrDefault(i => i.Id == campusOrganizationDto.Id);
                    Assert.AreEqual(result.Id, campusOrganizationDto.Id);
                    Assert.AreEqual(result.CampusOrganizationName, campusOrganizationDto.CampusOrganizationName);
                    Assert.AreEqual(result.CampusOrganizationType, campusOrganizationDto.CampusOrganizationType);
                    Assert.AreEqual(result.ParentOrganization, campusOrganizationDto.ParentOrganization);
                }
            }

            [TestMethod]
            public async Task CampusOrganizationTypesController_GetById()
            {
                var id = "cecdce5a-54a7-45fb-a975-5392a579e5bf";
                var campusOrganizationDto = campusOrganizationDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationByGuidAsync(id)).ReturnsAsync(campusOrganizationDto);

                var result = await campusOrganizationsController.GetCampusOrganizationByIdAsync(id);
                Assert.AreEqual(result.Id, campusOrganizationDto.Id);
                Assert.AreEqual(result.CampusOrganizationName, campusOrganizationDto.CampusOrganizationName);
                Assert.AreEqual(result.CampusOrganizationType, campusOrganizationDto.CampusOrganizationType);
                Assert.AreEqual(result.ParentOrganization, campusOrganizationDto.ParentOrganization);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetAll_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var results = await campusOrganizationsController.GetCampusOrganizationsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetById_KeyNotFoundException()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var result = await campusOrganizationsController.GetCampusOrganizationByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetById_Exception()
            {
                campusOrganizationServiceMock.Setup(ac => ac.GetCampusOrganizationByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await campusOrganizationsController.GetCampusOrganizationByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_PUT_Exception()
            {
                var result = await campusOrganizationsController.PutCampusOrganizationAsync(It.IsAny<string>(), It.IsAny<CampusOrganization>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_POST_Exception()
            {
                var result = await campusOrganizationsController.PostCampusOrganizationAsync(It.IsAny<CampusOrganization>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_DELETE_Exception()
            {
                await campusOrganizationsController.DeleteCampusOrganizationAsync(It.IsAny<string>());
            }
        }
    }
}
