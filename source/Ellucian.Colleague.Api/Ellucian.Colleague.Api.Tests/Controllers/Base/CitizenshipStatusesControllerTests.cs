// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
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
    public class CitizenshipStatusesControllerTests
    {
        [TestClass]
        public class CitizenshipStatusesControllerGet
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

            private CitizenshipStatusesController CitizenshipStatusesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> allCitizenshipStatusEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<CitizenshipStatus> CitizenshipStatusList;
            private string citizenshipStatusesGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus, CitizenshipStatus>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allCitizenshipStatusEntities = new TestCitizenshipStatusRepository().Get();
                CitizenshipStatusList = new List<CitizenshipStatus>();

                CitizenshipStatusesController = new CitizenshipStatusesController(demographicsService, logger);
                CitizenshipStatusesController.Request = new HttpRequestMessage();
                CitizenshipStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var races in allCitizenshipStatusEntities)
                {
                    CitizenshipStatus target = ConvertCitizenshipStatusEntityToDto(races);
                    CitizenshipStatusList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatusEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                CitizenshipStatusesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetCitizenshipStatusesByGuidAsync_Validate()
            {
                var thisCitizenshipStatus = CitizenshipStatusList.Where(m => m.Id == citizenshipStatusesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetCitizenshipStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisCitizenshipStatus);

                var citizenshipStatus = await CitizenshipStatusesController.GetCitizenshipStatusByIdAsync(citizenshipStatusesGuid);
                Assert.AreEqual(thisCitizenshipStatus.Id, citizenshipStatus.Id);
                Assert.AreEqual(thisCitizenshipStatus.Code, citizenshipStatus.Code);
                Assert.AreEqual(thisCitizenshipStatus.Description, citizenshipStatus.Description);
                Assert.AreEqual(thisCitizenshipStatus.citizenshipStatusType, citizenshipStatus.citizenshipStatusType);
            }

            [TestMethod]
            public async Task CitizenshipStatusesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(CitizenshipStatusList);

                var result = await CitizenshipStatusesController.GetCitizenshipStatusesAsync();
                Assert.AreEqual(result.Count(), allCitizenshipStatusEntities.Count());

                int count = allCitizenshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CitizenshipStatusList[i];
                    var actual = allCitizenshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CitizenshipStatusesController_GetHedmAsync_CacheControlNotNull()
            {
                CitizenshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(CitizenshipStatusList);

                var result = await CitizenshipStatusesController.GetCitizenshipStatusesAsync();
                Assert.AreEqual(result.Count(), allCitizenshipStatusEntities.Count());

                int count = allCitizenshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CitizenshipStatusList[i];
                    var actual = allCitizenshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CitizenshipStatusesController_GetHedmAsync_NoCache()
            {
                CitizenshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CitizenshipStatusesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(CitizenshipStatusList);

                var result = await CitizenshipStatusesController.GetCitizenshipStatusesAsync();
                Assert.AreEqual(result.Count(), allCitizenshipStatusEntities.Count());

                int count = allCitizenshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CitizenshipStatusList[i];
                    var actual = allCitizenshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CitizenshipStatusesController_GetHedmAsync_Cache()
            {
                CitizenshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                CitizenshipStatusesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(CitizenshipStatusList);

                var result = await CitizenshipStatusesController.GetCitizenshipStatusesAsync();
                Assert.AreEqual(result.Count(), allCitizenshipStatusEntities.Count());

                int count = allCitizenshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = CitizenshipStatusList[i];
                    var actual = allCitizenshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task CitizenshipStatusesController_GetByIdHedmAsync()
            {
                var thisCitizenshipStatus = CitizenshipStatusList.Where(m => m.Id == "87ec6f69-9b16-4ed5-8954-59067f0318ec").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetCitizenshipStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisCitizenshipStatus);

                var citizenshipStatus = await CitizenshipStatusesController.GetCitizenshipStatusByIdAsync("87ec6f69-9b16-4ed5-8954-59067f0318ec");
                Assert.AreEqual(thisCitizenshipStatus.Id, citizenshipStatus.Id);
                Assert.AreEqual(thisCitizenshipStatus.Code, citizenshipStatus.Code);
                Assert.AreEqual(thisCitizenshipStatus.Description, citizenshipStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusesAsync(It.IsAny<bool>())).Throws<Exception>();

                await CitizenshipStatusesController.GetCitizenshipStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await CitizenshipStatusesController.GetCitizenshipStatusByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_GetByIdThrowsKeyNotFoundException()
            {
                demographicsServiceMock.Setup(gc => gc.GetCitizenshipStatusByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await CitizenshipStatusesController.GetCitizenshipStatusByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_PostThrowsIntAppiExc()
            {
                await CitizenshipStatusesController.PostCitizenshipStatusAsync(CitizenshipStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_PutThrowsIntAppiExc()
            {
                var result = await CitizenshipStatusesController.PutCitizenshipStatusAsync(CitizenshipStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CitizenshipStatusesController_DeleteThrowsIntAppiExc()
            {
                await CitizenshipStatusesController.DeleteCitizenshipStatusAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a CitizenshipStatus domain entity to its corresponding CitizenshipStatus DTO
            /// </summary>
            /// <param name="source">CitizenshipStatus domain entity</param>
            /// <returns>CitizenshipStatus DTO</returns>
            private Ellucian.Colleague.Dtos.CitizenshipStatus ConvertCitizenshipStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus source)
            {
                var citizenshipStatus = new Ellucian.Colleague.Dtos.CitizenshipStatus();
                citizenshipStatus.Id = source.Guid;
                citizenshipStatus.Code = source.Code;
                citizenshipStatus.Title = source.Description;
                citizenshipStatus.Description = null;

                return citizenshipStatus;
            }
        }
    }
}