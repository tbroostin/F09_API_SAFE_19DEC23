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
    public class PrivacyStatusesControllerTests
    {
        [TestClass]
        public class PrivacyStatusesControllerGet
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

            private PrivacyStatusesController PrivacyStatusesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PrivacyStatus> allPrivacyStatusEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<PrivacyStatus> PrivacyStatusList;
            private string privacyStatusesGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PrivacyStatus, PrivacyStatus>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allPrivacyStatusEntities = new TestPrivacyStatusRepository().Get();
                PrivacyStatusList = new List<PrivacyStatus>();

                PrivacyStatusesController = new PrivacyStatusesController(demographicsService, logger);
                PrivacyStatusesController.Request = new HttpRequestMessage();
                PrivacyStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var privacyStatus in allPrivacyStatusEntities)
                {
                    PrivacyStatus target = ConvertPrivacyStatusEntityToDto(privacyStatus);
                    PrivacyStatusList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allPrivacyStatusEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                PrivacyStatusesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetPrivacyStatusesByGuidAsync_Validate()
            {
                var thisPrivacyStatus = PrivacyStatusList.Where(m => m.Id == privacyStatusesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetPrivacyStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPrivacyStatus);

                var privacyStatus = await PrivacyStatusesController.GetPrivacyStatusByIdAsync(privacyStatusesGuid);
                Assert.AreEqual(thisPrivacyStatus.Id, privacyStatus.Id);
                Assert.AreEqual(thisPrivacyStatus.Code, privacyStatus.Code);
                Assert.AreEqual(thisPrivacyStatus.Description, privacyStatus.Description);
                Assert.AreEqual(thisPrivacyStatus.privacyStatusType, privacyStatus.privacyStatusType);
            }

            [TestMethod]
            public async Task PrivacyStatusesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PrivacyStatusList);

                var result = await PrivacyStatusesController.GetPrivacyStatusesAsync();
                Assert.AreEqual(result.Count(), allPrivacyStatusEntities.Count());

                int count = allPrivacyStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PrivacyStatusList[i];
                    var actual = allPrivacyStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PrivacyStatusesController_GetHedmAsync_CacheControlNotNull()
            {
                PrivacyStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PrivacyStatusList);

                var result = await PrivacyStatusesController.GetPrivacyStatusesAsync();
                Assert.AreEqual(result.Count(), allPrivacyStatusEntities.Count());

                int count = allPrivacyStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PrivacyStatusList[i];
                    var actual = allPrivacyStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PrivacyStatusesController_GetHedmAsync_NoCache()
            {
                PrivacyStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PrivacyStatusesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PrivacyStatusList);

                var result = await PrivacyStatusesController.GetPrivacyStatusesAsync();
                Assert.AreEqual(result.Count(), allPrivacyStatusEntities.Count());

                int count = allPrivacyStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PrivacyStatusList[i];
                    var actual = allPrivacyStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PrivacyStatusesController_GetHedmAsync_Cache()
            {
                PrivacyStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PrivacyStatusesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PrivacyStatusList);

                var result = await PrivacyStatusesController.GetPrivacyStatusesAsync();
                Assert.AreEqual(result.Count(), allPrivacyStatusEntities.Count());

                int count = allPrivacyStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PrivacyStatusList[i];
                    var actual = allPrivacyStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PrivacyStatusesController_GetByIdHedmAsync()
            {
                var thisPrivacyStatus = PrivacyStatusList.Where(m => m.Id == "87ec6f69-9b16-4ed5-8954-59067f0318ec").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetPrivacyStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPrivacyStatus);

                var privacyStatus = await PrivacyStatusesController.GetPrivacyStatusByIdAsync("87ec6f69-9b16-4ed5-8954-59067f0318ec");
                Assert.AreEqual(thisPrivacyStatus.Id, privacyStatus.Id);
                Assert.AreEqual(thisPrivacyStatus.Code, privacyStatus.Code);
                Assert.AreEqual(thisPrivacyStatus.Description, privacyStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PrivacyStatusesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusesAsync(It.IsAny<bool>())).Throws<Exception>();

                await PrivacyStatusesController.GetPrivacyStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PrivacyStatusesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetPrivacyStatusByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await PrivacyStatusesController.GetPrivacyStatusByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PrivacyStatusesController_PostThrowsIntAppiExc()
            {
                await PrivacyStatusesController.PostPrivacyStatusAsync(PrivacyStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PrivacyStatusesController_PutThrowsIntAppiExc()
            {
                var result = await PrivacyStatusesController.PutPrivacyStatusAsync(PrivacyStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PrivacyStatusesController_DeleteThrowsIntAppiExc()
            {
                await PrivacyStatusesController.DeletePrivacyStatusAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a PrivacyStatus domain entity to its corresponding PrivacyStatus DTO
            /// </summary>
            /// <param name="source">PrivacyStatus domain entity</param>
            /// <returns>PrivacyStatus DTO</returns>
            private Ellucian.Colleague.Dtos.PrivacyStatus ConvertPrivacyStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.PrivacyStatus source)
            {
                var privacyStatus = new Ellucian.Colleague.Dtos.PrivacyStatus();
                privacyStatus.Id = source.Guid;
                privacyStatus.Code = source.Code;
                privacyStatus.Title = source.Description;
                privacyStatus.Description = null;

                return privacyStatus;
            }
        }
    }
}