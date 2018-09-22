// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class SitesControllerTests
    {
        [TestClass]
        public class SitesControllerGet
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private SitesController sitesController;
            private Mock<IReferenceDataRepository> refRepositoryMock;
            private IReferenceDataRepository refRepository;
           
            private List<Domain.Base.Entities.Location> allLocations = new List<Domain.Base.Entities.Location>();
            private List<Domain.Base.Entities.Building> allBuildings = new List<Domain.Base.Entities.Building>();
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IFacilitiesService> facilitiesServiceMock;
            private IFacilitiesService facilitiesService;
            List<Site2> locationList = new List<Site2>();
            
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                refRepositoryMock = new Mock<IReferenceDataRepository>();
                refRepository = refRepositoryMock.Object;

                // HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                // var AdapterRegistry = new AdapterRegistry(adapters, logger);
                // var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Location, Site2>(AdapterRegistry, logger);
                // AdapterRegistry.AddAdapter(testAdapter);

                facilitiesServiceMock = new Mock<IFacilitiesService>();
                facilitiesService = facilitiesServiceMock.Object;

                var location1 = new Domain.Base.Entities.Location("68c24373-70ff-4744-9945-5c59c17fa256", "MAIN", "Main campus");
                location1.AddBuilding("BIO");
                allLocations.Add(location1);

                var location2 = new Domain.Base.Entities.Location("eaf2e6da-b190-4a89-af76-97bd255fa92c", "CITY", "City campus");
                location2.AddBuilding("BIO");
                allLocations.Add(location2);

                var location3 = new Domain.Base.Entities.Location("e9807fd6-6b61-42e4-b001-ba2ae400cb5e", "SEC", "Secondary campus");
                location3.AddBuilding("BIO");
                allLocations.Add(location3);

                sitesController = new SitesController(facilitiesService, logger);
                sitesController.Request = new HttpRequestMessage();
                sitesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                allBuildings.Add(new Domain.Base.Entities.Building("6802f68b-e1af-4950-a680-d9da487fafb7", "AH", "Alumni Hall"));
                allBuildings.Add(new Domain.Base.Entities.Building("226dd583-739a-4b03-9ad1-f9567bec93c0", "BIO", "Biology Building"));
                allBuildings.Add(new Domain.Base.Entities.Building("6404a494-9740-45fd-8d32-04e3846d6927", "LIB", "Library"));

                refRepositoryMock.Setup(x => x.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(allBuildings);

                var buildingGuids = new List<GuidObject2>();
                buildingGuids.Add(new GuidObject2(allBuildings.Select(x => x.Guid).FirstOrDefault()));

                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Location, Site2>();
                foreach (var location in allLocations)
                {
                    Site2 target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.Location, Site2>(location);
                    target.Id = location.Guid;
                    locationList.Add(target);
                }
                refRepositoryMock.Setup(x => x.GetLocations(It.IsAny<bool>())).Returns(allLocations);

                facilitiesServiceMock.Setup<Task<IEnumerable<Site2>>>(s => s.GetSites2Async(It.IsAny<bool>())).ReturnsAsync(locationList);

                //For obsolete testing (line 121-134)
                var buildingGuidsObsolete = new List<GuidObject>();
                buildingGuidsObsolete.Add(new GuidObject(allBuildings.Select(x => x.Guid).FirstOrDefault()));
               
            }   

            [TestCleanup]
            public void Cleanup()
            {
                sitesController = null;
                refRepository = null;
                facilitiesServiceMock = null;
                locationList = null;
            }

            [TestMethod]
            public async Task GetHedmSitesAsync()
            {
                 var sites = await sitesController.GetHedmSitesAsync();
                Assert.AreEqual(allLocations.Count(), sites.Count());
                Assert.IsTrue(sites is IEnumerable<Dtos.Site2>);            
            }

            [TestMethod]
            public async Task GetHedmSiteByIdAsync()
            {
                var mainLocation = locationList.Where(x => x.Code == "MAIN").FirstOrDefault();
                facilitiesServiceMock.Setup<Task<Site2>>(s => s.GetSite2Async(mainLocation.Id.ToString())).ReturnsAsync(mainLocation);
               
                var sites = await sitesController.GetHedmSiteByIdAsync(mainLocation.Id);
                Assert.AreEqual(sites.Id, mainLocation.Id);
                Assert.AreEqual(sites.Code, mainLocation.Code);
                Assert.AreEqual(sites.Description, mainLocation.Description);
                Assert.AreEqual(sites.Title, mainLocation.Title);
            }

            [TestMethod]
            public async Task SitesController_GetHedmAsync_CacheControlNotNull()
            {
                sitesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                var sites = await sitesController.GetHedmSitesAsync();
                Assert.AreEqual(sites.Count(), allLocations.Count());

                int count = allLocations.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = locationList[i];
                    var actual = allLocations.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task SitesController_GetHedmAsync_NoCache()
            {
                sitesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                sitesController.Request.Headers.CacheControl.NoCache = true;

                var sites = await sitesController.GetHedmSitesAsync();
                Assert.AreEqual(sites.Count(), allLocations.Count());

                int count = allLocations.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = locationList[i];
                    var actual = allLocations.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task SitesController_GetHedmAsync_Cache()
            {
                sitesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                sitesController.Request.Headers.CacheControl.NoCache = false;

                var sites = await sitesController.GetHedmSitesAsync();
                Assert.AreEqual(sites.Count(), allLocations.Count());

                int count = allLocations.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = locationList[i];
                    var actual = allLocations.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Sites2Controller_GetThrowsIntAppiExc()
            {
                facilitiesServiceMock.Setup(gc => gc.GetSites2Async(It.IsAny<bool>())).Throws<Exception>();

                await sitesController.GetHedmSitesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Site2Controller_GetByIdThrowsIntAppiExc()
            {
                facilitiesServiceMock.Setup(gc => gc.GetSite2Async(It.IsAny<string>())).Throws<Exception>();

                await sitesController.GetHedmSiteByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SiteController_PostThrowsIntAppiExc()
            {
                await sitesController.PostSiteAsync(locationList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SiteController_PutThrowsIntAppiExc()
            {
                var result = await sitesController.PutSiteAsync("dhjdsk", locationList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SiteController_DeleteThrowsIntAppiExc()
            {
                var result = await sitesController.DeleteSiteAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }
        }
    }
}