// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class CommodityUnitTypesControllerTest
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

        private Mock<IAdapterRegistry> AdapterRegistryMock;
        private IAdapterRegistry AdapterRegistry;
        private Mock<IColleagueFinanceReferenceDataRepository> ReferenceDataRepositoryMock;
        private IColleagueFinanceReferenceDataRepository ReferenceDataRepository;

        private Mock<ICommodityUnitTypesService> CommodityUnitTypeServiceMock;
        private ICommodityUnitTypesService CommodityUnitTypeService;
        private ILogger logger = new Mock<ILogger>().Object;

        private Mock<CommodityUnitTypesController> CommodityUnitTypesControllerMock;
        CommodityUnitTypesController CommodityUnitTypesController;
        private IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType> allCommodityUnitTypeEntities;
        private List<Dtos.CommodityUnitType> allCommodityUnitTypeDtos = new List<Dtos.CommodityUnitType>();
        private string commodityUnitTypeGuid = "884a59d1-20e5-43af-94e3-f1504230bbbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            ReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            ReferenceDataRepository = ReferenceDataRepositoryMock.Object;

            AdapterRegistryMock = new Mock<IAdapterRegistry>();
            AdapterRegistry = AdapterRegistryMock.Object;

            CommodityUnitTypeServiceMock = new Mock<ICommodityUnitTypesService>();
            CommodityUnitTypeService = CommodityUnitTypeServiceMock.Object;

            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType, Ellucian.Colleague.Dtos.CommodityUnitType>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);
            AdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.CommodityUnitType, Dtos.CommodityUnitType>()).Returns(testAdapter);

            allCommodityUnitTypeEntities = new TestColleagueFinanceReferenceDataRepository().GetCommodityUnitTypesAsync(false).Result;

            Mapper.CreateMap<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType, Dtos.CommodityUnitType>();
            foreach(var gradeChangeReason in allCommodityUnitTypeEntities)
            {
                Dtos.CommodityUnitType target = Mapper.Map<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityUnitType, Dtos.CommodityUnitType>(gradeChangeReason);
                target.Id = gradeChangeReason.Guid;
                target.Title = gradeChangeReason.Description;

                allCommodityUnitTypeDtos.Add(target);
            }

            ReferenceDataRepositoryMock.Setup(x => x.GetCommodityUnitTypesAsync(false)).ReturnsAsync(allCommodityUnitTypeEntities);

            CommodityUnitTypesControllerMock = new Mock<Api.Controllers.ColleagueFinance.CommodityUnitTypesController>();

            CommodityUnitTypesController = new CommodityUnitTypesController(AdapterRegistry, CommodityUnitTypeService, logger);
            CommodityUnitTypesController.Request = new HttpRequestMessage();
            CommodityUnitTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            CommodityUnitTypesController = null;
            ReferenceDataRepository = null;
            allCommodityUnitTypeEntities = null;
        }

        [TestMethod]
        public async Task CommodityUnitTypeController_GetCommodityUnitTypesAsync()
        {
            CommodityUnitTypeServiceMock.Setup(x => x.GetCommodityUnitTypesAsync(false)).ReturnsAsync(allCommodityUnitTypeDtos);

            var allCommodityUnitTypes = (await CommodityUnitTypesController.GetCommodityUnitTypesAsync()).ToList();
            Assert.AreEqual(allCommodityUnitTypeDtos.Count, allCommodityUnitTypes.Count);

            for(int i = 0; i < allCommodityUnitTypes.Count; i++)
            {
                var expected = allCommodityUnitTypeDtos[i];
                var actual = allCommodityUnitTypes[i];

                Assert.AreEqual(expected.Title, actual.Title);
            }
        }

        [TestMethod]
        public async Task CommodityUnitTypeController_GetCommodityUnitTypeById()
        {
            var gradeChangeReason = allCommodityUnitTypeDtos.Where(i => i.Id == commodityUnitTypeGuid).FirstOrDefault();

            CommodityUnitTypeServiceMock.Setup(x => x.GetCommodityUnitTypeByIdAsync(commodityUnitTypeGuid)).ReturnsAsync(gradeChangeReason);

            var expected = (await CommodityUnitTypesController.GetCommodityUnitTypeByIdAsync(commodityUnitTypeGuid));

            Assert.AreEqual(expected.Code, allCommodityUnitTypeDtos[0].Code);
            Assert.AreEqual(expected.Description, allCommodityUnitTypeDtos[0].Description);
            Assert.AreEqual(expected.Id, allCommodityUnitTypeDtos[0].Id);
            Assert.AreEqual(expected.Title, allCommodityUnitTypeDtos[0].Title);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityUnitTypesController_GetCommodityUnitTypesAsync_Exception()
        {
            CommodityUnitTypeServiceMock.Setup(x => x.GetCommodityUnitTypesAsync(false)).Throws<Exception>();
            await CommodityUnitTypesController.GetCommodityUnitTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityUnitTypesController_GetCommodityUnitTypeByGuidAsync_Exception()
        {
            CommodityUnitTypeServiceMock.Setup(x => x.GetCommodityUnitTypeByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await CommodityUnitTypesController.GetCommodityUnitTypeByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityUnitTypeController_PostThrowsIntAppiExc()
        {
            var result = await CommodityUnitTypesController.PostCommodityUnitTypeAsync(allCommodityUnitTypeDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityUnitTypeController_PustThrowsIntAppiExc()
        {
            var result = await CommodityUnitTypesController.PutCommodityUnitTypeAsync( commodityUnitTypeGuid, allCommodityUnitTypeDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityUnitTypeController_DeleteThrowsIntAppiExc()
        {
            await CommodityUnitTypesController.DeleteCommodityUnitTypeAsync(commodityUnitTypeGuid);
        }
    }
}
