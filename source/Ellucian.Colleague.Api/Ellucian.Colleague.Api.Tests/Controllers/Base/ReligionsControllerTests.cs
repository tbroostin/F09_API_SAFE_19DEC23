// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ReligionsControllerTests
    {
        [TestClass]
        public class ReligionsControllerGet
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

            private ReligionsController ReligionsController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Denomination> allDenominationEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<Religion> ReligionList;
            private string religionsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Denomination, Religion>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allDenominationEntities = new TestReligionRepository().GetDenominations();
                ReligionList = new List<Religion>();

                ReligionsController = new ReligionsController(AdapterRegistry, ReferenceRepository, demographicsService, logger);
                ReligionsController.Request = new HttpRequestMessage();
                ReligionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var denomination in allDenominationEntities)
                {
                    Religion target = ConvertDenominationEntityToDto(denomination);
                    ReligionList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetDenominationsAsync(It.IsAny<bool>())).ReturnsAsync(allDenominationEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                ReligionsController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetReligionsByGuidAsync_Validate()
            {
                var thisReligion = ReligionList.Where(m => m.Id == religionsGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetReligionByIdAsync(It.IsAny<string>())).ReturnsAsync(thisReligion);

                var religion = await ReligionsController.GetReligionByIdAsync(religionsGuid);
                Assert.AreEqual(thisReligion.Id, religion.Id);
                Assert.AreEqual(thisReligion.Code, religion.Code);
                Assert.AreEqual(thisReligion.Description, religion.Description);
            }

            [TestMethod]
            public async Task ReligionsController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetReligionsAsync(It.IsAny<bool>())).ReturnsAsync(ReligionList);

                var result = await ReligionsController.GetReligionsAsync();
                Assert.AreEqual(result.Count(), allDenominationEntities.Count());

                int count = allDenominationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = ReligionList[i];
                    var actual = allDenominationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task ReligionsController_GetHedmAsync_CacheControlNotNull()
            {
                ReligionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetReligionsAsync(It.IsAny<bool>())).ReturnsAsync(ReligionList);

                var result = await ReligionsController.GetReligionsAsync();
                Assert.AreEqual(result.Count(), allDenominationEntities.Count());

                int count = allDenominationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = ReligionList[i];
                    var actual = allDenominationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task ReligionsController_GetHedmAsync_NoCache()
            {
                ReligionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                ReligionsController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetReligionsAsync(It.IsAny<bool>())).ReturnsAsync(ReligionList);

                var result = await ReligionsController.GetReligionsAsync();
                Assert.AreEqual(result.Count(), allDenominationEntities.Count());

                int count = allDenominationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = ReligionList[i];
                    var actual = allDenominationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task ReligionsController_GetHedmAsync_Cache()
            {
                ReligionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                ReligionsController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetReligionsAsync(It.IsAny<bool>())).ReturnsAsync(ReligionList);

                var result = await ReligionsController.GetReligionsAsync();
                Assert.AreEqual(result.Count(), allDenominationEntities.Count());

                int count = allDenominationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = ReligionList[i];
                    var actual = allDenominationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task ReligionsController_GetByIdHedmAsync()
            {
                ReligionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                ReligionsController.Request.Headers.CacheControl.NoCache = true;

                var thisReligion = ReligionList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetReligionByIdAsync(It.IsAny<string>())).ReturnsAsync(thisReligion);

                var religion = await ReligionsController.GetReligionByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisReligion.Id, religion.Id);
                Assert.AreEqual(thisReligion.Code, religion.Code);
                Assert.AreEqual(thisReligion.Description, religion.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetReligionsAsync(It.IsAny<bool>())).Throws<Exception>();

                await ReligionsController.GetReligionsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetReligionByIdAsync(It.IsAny<string>())).Throws<Exception>();

                await ReligionsController.GetReligionByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_GetByIdThrowsIntAppiExc_NullId()
            {
                await ReligionsController.GetReligionByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_PostThrowsIntAppiExc()
            {
                await ReligionsController.PostReligionsAsync(ReligionList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_PutThrowsIntAppiExc()
            {
                var result = await ReligionsController.PutReligionsAsync(ReligionList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ReligionsController_DeleteThrowsIntAppiExc()
            {
                await ReligionsController.DeleteReligionsAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a Denomination domain entity to its corresponding Religion DTO
            /// </summary>
            /// <param name="source">Denomination domain entity</param>
            /// <returns>Religion DTO</returns>
            private Ellucian.Colleague.Dtos.Religion ConvertDenominationEntityToDto(Ellucian.Colleague.Domain.Base.Entities.Denomination source)
            {
                var religion = new Ellucian.Colleague.Dtos.Religion();
                religion.Id = source.Guid;
                religion.Code = source.Code;
                religion.Title = source.Description;
                religion.Description = null;

                return religion;
            }
        }
    }
}