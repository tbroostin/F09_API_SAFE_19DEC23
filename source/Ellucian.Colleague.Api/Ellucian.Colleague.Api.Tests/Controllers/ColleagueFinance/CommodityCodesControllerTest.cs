// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
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
    public class CommodityCodesControllerTest
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

        private Mock<ICommodityCodesService> CommodityCodeServiceMock;
        private ICommodityCodesService CommodityCodeService;
        private ILogger logger = new Mock<ILogger>().Object;

        private Mock<CommodityCodesController> CommodityCodesControllerMock;
        CommodityCodesController CommodityCodesController;
        private IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode> allCommodityCodeEntities;
        private List<Dtos.CommodityCode> allCommodityCodeDtos = new List<Dtos.CommodityCode>();
        private List<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode> allCFCommodityCodeDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>();
        private string commodityCodeGuid = "884a59d1-20e5-43af-94e3-f1504230bbbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            ReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            ReferenceDataRepository = ReferenceDataRepositoryMock.Object;

            AdapterRegistryMock = new Mock<IAdapterRegistry>();
            AdapterRegistry = AdapterRegistryMock.Object;

            CommodityCodeServiceMock = new Mock<ICommodityCodesService>();
            CommodityCodeService = CommodityCodeServiceMock.Object;

            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode, Ellucian.Colleague.Dtos.CommodityCode>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);
            AdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.CommodityCode, Dtos.CommodityCode>()).Returns(testAdapter);

            allCommodityCodeEntities = new TestColleagueFinanceReferenceDataRepository().GetCommodityCodesAsync(false).Result;

            Mapper.CreateMap<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode, Dtos.CommodityCode>();
            foreach(var gradeChangeReason in allCommodityCodeEntities)
            {
                Dtos.CommodityCode target = Mapper.Map<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CommodityCode, Dtos.CommodityCode>(gradeChangeReason);
                target.Id = gradeChangeReason.Guid;
                target.Title = gradeChangeReason.Description;

                allCommodityCodeDtos.Add(target);
            }

            ReferenceDataRepositoryMock.Setup(x => x.GetCommodityCodesAsync(false)).ReturnsAsync(allCommodityCodeEntities);

            CommodityCodesControllerMock = new Mock<Api.Controllers.ColleagueFinance.CommodityCodesController>();

            CommodityCodesController = new CommodityCodesController(AdapterRegistry, CommodityCodeService, logger);
            CommodityCodesController.Request = new HttpRequestMessage();
            CommodityCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            CommodityCodesController = null;
            ReferenceDataRepository = null;
            allCommodityCodeEntities = null;
        }

        [TestMethod]
        public async Task CommodityCodeController_GetCommodityCodesAsync()
        {
            CommodityCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            CommodityCodesController.Request.Headers.CacheControl.NoCache = true;

            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodesAsync(true)).ReturnsAsync(allCommodityCodeDtos);

            var allCommodityCodes = (await CommodityCodesController.GetCommodityCodesAsync()).ToList();
            Assert.AreEqual(allCommodityCodeDtos.Count, allCommodityCodes.Count);

            for(int i = 0; i < allCommodityCodes.Count; i++)
            {
                var expected = allCommodityCodeDtos[i];
                var actual = allCommodityCodes[i];

                Assert.AreEqual(expected.Title, actual.Title);
            }
        }

        [TestMethod]
        public async Task CommodityCodeController_GetCommodityCodeById()
        {
            CommodityCodesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            CommodityCodesController.Request.Headers.CacheControl.NoCache = true;

            var gradeChangeReason = allCommodityCodeDtos.Where(i => i.Id == commodityCodeGuid).FirstOrDefault();

            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodeByIdAsync(commodityCodeGuid)).ReturnsAsync(gradeChangeReason);

            var expected = (await CommodityCodesController.GetCommodityCodeByIdAsync(commodityCodeGuid));

            Assert.AreEqual(expected.Code, allCommodityCodeDtos[0].Code);
            Assert.AreEqual(expected.Description, allCommodityCodeDtos[0].Description);
            Assert.AreEqual(expected.Id, allCommodityCodeDtos[0].Id);
            Assert.AreEqual(expected.Title, allCommodityCodeDtos[0].Title);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodesAsync_Exception()
        {
            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodesAsync(false)).Throws<Exception>();
            await CommodityCodesController.GetCommodityCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeByGuidAsync_Exception()
        {
            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodeByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await CommodityCodesController.GetCommodityCodeByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeByGuidAsync_KeyNotFoundException()
        {
            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodeByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await CommodityCodesController.GetCommodityCodeByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeByGuidAsync_WithId_Exception()
        {
            CommodityCodeServiceMock.Setup(x => x.GetCommodityCodeByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await CommodityCodesController.GetCommodityCodeByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodeController_PostThrowsIntAppiExc()
        {
            var result = await CommodityCodesController.PostCommodityCodeAsync(allCommodityCodeDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodeController_PustThrowsIntAppiExc()
        {
            var result = await CommodityCodesController.PutCommodityCodeAsync( commodityCodeGuid, allCommodityCodeDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodeController_DeleteThrowsIntAppiExc()
        {
            await CommodityCodesController.DeleteCommodityCodeAsync(commodityCodeGuid);
        }

        [TestMethod]
        public async Task CommodityCodeController_GetAllCommodityCodesAsync()
        {

            CommodityCodeServiceMock.Setup(x => x.GetAllCommodityCodesAsync()).ReturnsAsync(allCFCommodityCodeDtos);

            var allCommodityCodes = (await CommodityCodesController.GetAllCommodityCodesAsync()).ToList();
            Assert.AreEqual(allCFCommodityCodeDtos.Count, allCommodityCodes.Count);
        }
    }

    #region GetCommodityCodeAsync Tests

    [TestClass]
    public class GetCommodityCodeAsyncTests
    {
        public TestContext TestContext { get; set; }

        private Mock<ICommodityCodesService> _commodityCodesServiceMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
               
        private CommodityCodesController _commodityCodesController;

        private string commodityCode;        
        private Dtos.ColleagueFinance.ProcurementCommodityCode resultDto;        

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _commodityCodesServiceMock = new Mock<ICommodityCodesService>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            
            commodityCode = "100900";            

            BuildData();
            _commodityCodesServiceMock.Setup(s => s.GetCommodityCodeByCodeAsync(commodityCode)).ReturnsAsync(resultDto);
            _commodityCodesController = new CommodityCodesController(_adapterRegistryMock.Object, _commodityCodesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            resultDto = new Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode
            {
                Code = commodityCode,
                DefaultDescFlag = true,
                Description = "Robotics Kit",
                FixedAssetsFlag = "S",
                Price = 10,
                TaxCodes = new List<string>()
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            _commodityCodesController = null;
            _loggerMock = null;
            _commodityCodesServiceMock = null;
            resultDto = null;
        }

        [TestMethod]
        public async Task CommodityCodesController_GetCommodityCodeAsync_Success()
        {
            var actualDto = await _commodityCodesController.GetCommodityCodeAsync(commodityCode);
            Assert.IsNotNull(actualDto);

            Assert.AreEqual(resultDto.Code, actualDto.Code);
            Assert.AreEqual(resultDto.DefaultDescFlag, actualDto.DefaultDescFlag);
            Assert.AreEqual(resultDto.Description, actualDto.Description);
            Assert.AreEqual(resultDto.FixedAssetsFlag, actualDto.FixedAssetsFlag);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeAsync_NullCriteria()
        {
            var actualDto = await _commodityCodesController.GetCommodityCodeAsync(null);
            Assert.IsNull(actualDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeAsync_EmptyCriteria()
        {
            var actualDto = await _commodityCodesController.GetCommodityCodeAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeAsync_ArgumentNullException()
        {
            _commodityCodesServiceMock.Setup(s => s.GetCommodityCodeByCodeAsync(null)).Throws(new ArgumentNullException());
            _commodityCodesController = new CommodityCodesController(_adapterRegistryMock.Object,_commodityCodesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _commodityCodesController.GetCommodityCodeAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeAsync_Exception()
        {
            _commodityCodesServiceMock.Setup(s => s.GetCommodityCodeByCodeAsync(It.IsAny<string>())).Throws(new Exception());
            _commodityCodesController = new CommodityCodesController(_adapterRegistryMock.Object, _commodityCodesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _commodityCodesController.GetCommodityCodeAsync(commodityCode);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommodityCodesController_GetCommodityCodeAsync_KeyNotFoundException()
        {
            _commodityCodesServiceMock.Setup(s => s.GetCommodityCodeByCodeAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
            _commodityCodesController = new CommodityCodesController(_adapterRegistryMock.Object, _commodityCodesServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _commodityCodesController.GetCommodityCodeAsync(commodityCode);
        }
    }

    #endregion
}
