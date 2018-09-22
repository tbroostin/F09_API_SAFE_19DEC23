// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class BargainingUnitsControllerTests
    {
        [TestClass]
        public class BargainingUnitsControllerGet
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

            private BargainingUnitsController BargainingUnitsController;
            private Mock<IHumanResourcesReferenceDataRepository> ReferenceRepositoryMock;
            private IHumanResourcesReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.BargainingUnit> allBargainingUnitEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IBargainingUnitsService> bargainingUnitsServiceMock;
            private IBargainingUnitsService bargainingUnitsService;
            List<BargainingUnit> BargainingUnitList;
            private string bargainingUnitsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.BargainingUnit, BargainingUnit>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                bargainingUnitsServiceMock = new Mock<IBargainingUnitsService>();
                bargainingUnitsService = bargainingUnitsServiceMock.Object;

                allBargainingUnitEntities = new TestBargainingUnitsRepository().GetBargainingUnits();
                BargainingUnitList = new List<BargainingUnit>();

                BargainingUnitsController = new BargainingUnitsController(bargainingUnitsService, logger);
                BargainingUnitsController.Request = new HttpRequestMessage();
                BargainingUnitsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var denomination in allBargainingUnitEntities)
                {
                    BargainingUnit target = ConvertBargainingUnitEntityToDto(denomination);
                    BargainingUnitList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(allBargainingUnitEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BargainingUnitsController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetBargainingUnitsByGuidAsync_Validate()
            {
                var thisBargainingUnit = BargainingUnitList.Where(m => m.Id == bargainingUnitsGuid).FirstOrDefault();

                bargainingUnitsServiceMock.Setup(x => x.GetBargainingUnitsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisBargainingUnit);

                var bargainingUnit = await BargainingUnitsController.GetBargainingUnitByIdAsync(bargainingUnitsGuid);
                Assert.AreEqual(thisBargainingUnit.Id, bargainingUnit.Id);
                Assert.AreEqual(thisBargainingUnit.Code, bargainingUnit.Code);
                Assert.AreEqual(thisBargainingUnit.Description, bargainingUnit.Description);
            }

            [TestMethod]
            public async Task BargainingUnitsController_GetHedmAsync()
            {
                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(BargainingUnitList);

                var result = await BargainingUnitsController.GetBargainingUnitsAsync();
                Assert.AreEqual(result.Count(), allBargainingUnitEntities.Count());

                int count = allBargainingUnitEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = BargainingUnitList[i];
                    var actual = allBargainingUnitEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task BargainingUnitsController_GetHedmAsync_CacheControlNotNull()
            {
                BargainingUnitsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(BargainingUnitList);

                var result = await BargainingUnitsController.GetBargainingUnitsAsync();
                Assert.AreEqual(result.Count(), allBargainingUnitEntities.Count());

                int count = allBargainingUnitEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = BargainingUnitList[i];
                    var actual = allBargainingUnitEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task BargainingUnitsController_GetHedmAsync_NoCache()
            {
                BargainingUnitsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                BargainingUnitsController.Request.Headers.CacheControl.NoCache = true;

                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(BargainingUnitList);

                var result = await BargainingUnitsController.GetBargainingUnitsAsync();
                Assert.AreEqual(result.Count(), allBargainingUnitEntities.Count());

                int count = allBargainingUnitEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = BargainingUnitList[i];
                    var actual = allBargainingUnitEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task BargainingUnitsController_GetHedmAsync_Cache()
            {
                BargainingUnitsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                BargainingUnitsController.Request.Headers.CacheControl.NoCache = false;

                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(BargainingUnitList);

                var result = await BargainingUnitsController.GetBargainingUnitsAsync();
                Assert.AreEqual(result.Count(), allBargainingUnitEntities.Count());

                int count = allBargainingUnitEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = BargainingUnitList[i];
                    var actual = allBargainingUnitEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task BargainingUnitsController_GetByIdHedmAsync()
            {
                BargainingUnitsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                BargainingUnitsController.Request.Headers.CacheControl.NoCache = true;

                var thisBargainingUnit = BargainingUnitList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                bargainingUnitsServiceMock.Setup(x => x.GetBargainingUnitsByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisBargainingUnit);

                var bargainingUnit = await BargainingUnitsController.GetBargainingUnitByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisBargainingUnit.Id, bargainingUnit.Id);
                Assert.AreEqual(thisBargainingUnit.Code, bargainingUnit.Code);
                Assert.AreEqual(thisBargainingUnit.Description, bargainingUnit.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_GetThrowsIntAppiExc()
            {
                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsAsync(It.IsAny<bool>())).Throws<Exception>();

                await BargainingUnitsController.GetBargainingUnitsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_GetByIdThrowsIntAppiExc_NullId()
            {
                await BargainingUnitsController.GetBargainingUnitByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_GetByIdThrowsIntAppiExc()
            {
                bargainingUnitsServiceMock.Setup(gc => gc.GetBargainingUnitsByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await BargainingUnitsController.GetBargainingUnitByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_PostThrowsIntAppiExc()
            {
                await BargainingUnitsController.PostBargainingUnitAsync(BargainingUnitList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_PutThrowsIntAppiExc()
            {
                var result = await BargainingUnitsController.PutBargainingUnitAsync(BargainingUnitList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BargainingUnitsController_DeleteThrowsIntAppiExc()
            {
                await BargainingUnitsController.DeleteBargainingUnitAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a BargainingUnit domain entity to its corresponding BargainingUnit DTO
            /// </summary>
            /// <param name="source">BargainingUnit domain entity</param>
            /// <returns>BargainingUnit DTO</returns>
            private Ellucian.Colleague.Dtos.BargainingUnit ConvertBargainingUnitEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.BargainingUnit source)
            {
                var bargainingUnit = new Ellucian.Colleague.Dtos.BargainingUnit();
                bargainingUnit.Id = source.Guid;
                bargainingUnit.Code = source.Code;
                bargainingUnit.Title = source.Description;
                bargainingUnit.Description = null;

                return bargainingUnit;
            }
        }
    }
}