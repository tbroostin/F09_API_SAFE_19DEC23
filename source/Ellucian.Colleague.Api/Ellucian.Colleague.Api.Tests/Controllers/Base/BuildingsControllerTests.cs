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
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{

    [TestClass]
    public class BuildingControllerTests
    {
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

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private IReferenceDataRepository referenceDataRepository;
        private Mock<IFacilitiesService> facilitiesServiceMock;
        private IFacilitiesService facilitiesService;
        private ILogger logger = new Mock<ILogger>().Object;

        private BuildingsController buildingsController;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Building> allBuildingEntities;
        private List<Dtos.Building2> allBuilding2Dtos = new List<Dtos.Building2>();
        private List<Dtos.Building3> allBuilding3Dtos = new List<Dtos.Building3>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            facilitiesServiceMock = new Mock<IFacilitiesService>();
            facilitiesService = facilitiesServiceMock.Object;

            allBuildingEntities = new TestBuildingRepository().Get();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Building, Dtos.Building2>();
            foreach (var building in allBuildingEntities)
            {
                Dtos.Building2 target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.Building, Dtos.Building2>(building);
                target.Id = building.Guid;
                allBuilding2Dtos.Add(target);
            }

            Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Building, Dtos.Building3>();
            foreach (var building in allBuildingEntities)
            {
                Dtos.Building3 target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.Building, Dtos.Building3>(building);
                target.Id = building.Guid;
                allBuilding3Dtos.Add(target);
            }

            buildingsController = new BuildingsController(adapterRegistry, referenceDataRepository, facilitiesService, logger);
            buildingsController.Request = new HttpRequestMessage();
            buildingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            buildingsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task GetHedmBuildings2Async_ValidateFields()
        {
            facilitiesServiceMock.Setup(x => x.GetBuildings2Async(It.IsAny<bool>())).ReturnsAsync(allBuilding2Dtos);

            var buildings = (await buildingsController.GetHedmBuildings2Async()).ToList();
            Assert.AreEqual(allBuilding2Dtos.Count, buildings.Count);
            for (int i = 0; i < buildings.Count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = buildings[i];
                Assert.AreEqual(expected.Id, actual.Id, "Guid, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Description, actual.Description, "Desc, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task GetHedmBuildingByIdAsync_ValidateFields()
        {
            var expected = allBuilding2Dtos.FirstOrDefault();
            facilitiesServiceMock.Setup(x => x.GetBuilding2Async(expected.Id)).ReturnsAsync(expected);

            var actual = (await buildingsController.GetHedmBuildingByIdAsync(expected.Id));

            Assert.AreEqual(expected.Id, actual.Id, "Guid");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        public async Task BuildingsController_GetHedmAsync_CacheControlNotNull()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            facilitiesServiceMock.Setup(gc => gc.GetBuildings2Async(It.IsAny<bool>())).ReturnsAsync(allBuilding2Dtos);

            var building = await buildingsController.GetHedmBuildings2Async();
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task BuildingsController_GetHedmAsync_NoCache()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            buildingsController.Request.Headers.CacheControl.NoCache = true;

            facilitiesServiceMock.Setup(gc => gc.GetBuildings2Async(It.IsAny<bool>())).ReturnsAsync(allBuilding2Dtos);

            var building = await buildingsController.GetHedmBuildings2Async();
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task BuildingsController_GetHedmAsync_Cache()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            buildingsController.Request.Headers.CacheControl.NoCache = false;

            facilitiesServiceMock.Setup(gc => gc.GetBuildings2Async(It.IsAny<bool>())).ReturnsAsync(allBuilding2Dtos);

            var building = await buildingsController.GetHedmBuildings2Async();
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Buildings2Controller_GetThrowsIntAppiExc()
        {
            facilitiesServiceMock.Setup(gc => gc.GetBuildings2Async(It.IsAny<bool>())).Throws<Exception>();

            await buildingsController.GetHedmBuildings2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Building2Controller_GetByIdThrowsIntAppiExc()
        {
            facilitiesServiceMock.Setup(gc => gc.GetBuilding2Async(It.IsAny<string>())).Throws<Exception>();

            await buildingsController.GetHedmBuildingByIdAsync("sdjfh");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuildingController_PostThrowsIntAppiExc()
        {
            await buildingsController.PostBuildingAsync(allBuilding2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuildingController_PutThrowsIntAppiExc()
        {
            var result = await buildingsController.PutBuildingAsync("dhjdsk", allBuilding2Dtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuildingController_DeleteThrowsIntAppiExc()
        {
            var result = await buildingsController.DeleteBuildingAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
        }


        [TestMethod]
        public async Task GetHedmBuildings3Async_ValidateFields()
        {
            facilitiesServiceMock.Setup(x => x.GetBuildings3Async(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(allBuilding3Dtos);

            var buildings = (await buildingsController.GetHedmBuildings3Async(null)).ToList();
            Assert.AreEqual(allBuilding3Dtos.Count, buildings.Count);
            for (int i = 0; i < buildings.Count; i++)
            {
                var expected = allBuilding3Dtos[i];
                var actual = buildings[i];
                Assert.AreEqual(expected.Id, actual.Id, "Guid, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Description, actual.Description, "Desc, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task GetHedmBuildingById2Async_ValidateFields()
        {
            var expected = allBuilding3Dtos.FirstOrDefault();
            facilitiesServiceMock.Setup(x => x.GetBuilding3Async(expected.Id)).ReturnsAsync(expected);

            var actual = (await buildingsController.GetHedmBuildingById2Async(expected.Id));

            Assert.AreEqual(expected.Id, actual.Id, "Guid");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }


        [TestMethod]
        public async Task BuildingsController_GetHedm2Async_CacheControlNotNull()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            facilitiesServiceMock.Setup(gc => gc.GetBuildings3Async(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(allBuilding3Dtos);

            var building = await buildingsController.GetHedmBuildings3Async(null);
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }


        [TestMethod]
        public async Task BuildingsController_GetHedm2Async_NoCache()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            buildingsController.Request.Headers.CacheControl.NoCache = true;

            facilitiesServiceMock.Setup(gc => gc.GetBuildings3Async(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(allBuilding3Dtos);

            var building = await buildingsController.GetHedmBuildings3Async(null);
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding2Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task BuildingsController_GetHedm2Async_Cache()
        {
            buildingsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            buildingsController.Request.Headers.CacheControl.NoCache = false;

            facilitiesServiceMock.Setup(gc => gc.GetBuildings3Async(It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(allBuilding3Dtos);

            var building = await buildingsController.GetHedmBuildings3Async(null);
            Assert.AreEqual(building.Count(), allBuildingEntities.Count());

            int count = allBuildingEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = allBuilding3Dtos[i];
                var actual = allBuildingEntities.ToList()[i];

                Assert.AreEqual(expected.Id, actual.Guid);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task Buildings3Controller_GetThrowsIntAppiExc()
        //{
        //    facilitiesServiceMock.Setup(gc => gc.GetBuildings3Async(It.IsAny<bool>(), "")).Throws<Exception>();

        //    Ellucian.Web.Http.Models.QueryStringFilter criteria = new Web.Http.Models.QueryStringFilter("mapVisibility", "{'mapVisibility':''}");
        //    await buildingsController.GetHedmBuildings3Async(null);
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task Building3Controller_GetByIdThrowsIntAppiExc()
        {
            facilitiesServiceMock.Setup(gc => gc.GetBuilding3Async(It.IsAny<string>())).Throws<Exception>();

            await buildingsController.GetHedmBuildingById2Async("sdjfh");
        }
    }
}