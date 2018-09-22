// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class MaritalStatusesControllerTests
    {
        [TestClass]
        public class MaritalStatusesControllerGet
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

            private MaritalStatusesController MaritalStatusesController;
            private Mock<IReferenceDataRepository> MaritalStatusRepositoryMock;
            private IReferenceDataRepository MaritalStatusRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.MaritalStatus> allMaritalStatusEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<MaritalStatus2> MaritalStatusList;
            private string maritalStatusesGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                MaritalStatusRepositoryMock = new Mock<IReferenceDataRepository>();
                MaritalStatusRepository = MaritalStatusRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.MaritalStatus, MaritalStatus2>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allMaritalStatusEntities = new TestMaritalStatusRepository().Get();
                MaritalStatusList = new List<MaritalStatus2>();

                MaritalStatusesController = new MaritalStatusesController(AdapterRegistry, MaritalStatusRepository, demographicsService, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.MaritalStatus, MaritalStatus2>();
                MaritalStatusesController.Request = new HttpRequestMessage();
                MaritalStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var maritalStatus in allMaritalStatusEntities)
                {
                    MaritalStatus2 target = ConvertMaritalStatusEntitytoMaritalStatusDto(maritalStatus);
                    MaritalStatusList.Add(target);
                }
                MaritalStatusRepositoryMock.Setup(x => x.MaritalStatusesAsync()).ReturnsAsync(allMaritalStatusEntities);
                MaritalStatusRepositoryMock.Setup(x => x.GetMaritalStatusesAsync(false)).ReturnsAsync(allMaritalStatusEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MaritalStatusesController = null;
                MaritalStatusRepository = null;
            }

            [TestMethod]
            public async Task ReturnsAllMaritalStatusesAsync()
            {
                var maritalStatuses = await MaritalStatusesController.GetAsync();
                Assert.AreEqual(maritalStatuses.Count(), allMaritalStatusEntities.Count());
            }

            [TestMethod]
            public async Task GetMaritalStatusesByGuidAsync_Validate()
            {
                var thisMaritalStatus = MaritalStatusList.Where(m => m.Id == maritalStatusesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetMaritalStatusById2Async(It.IsAny<string>())).ReturnsAsync(thisMaritalStatus);

                var maritalStatus = await MaritalStatusesController.GetMaritalStatusById2Async(maritalStatusesGuid);
                Assert.AreEqual(thisMaritalStatus.Id, maritalStatus.Id);
                Assert.AreEqual(thisMaritalStatus.Code, maritalStatus.Code);
                Assert.AreEqual(thisMaritalStatus.Description, maritalStatus.Description);
                Assert.AreEqual(thisMaritalStatus.StatusType, maritalStatus.StatusType);
            }

            [TestMethod]
            public async Task MaritalStatusesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetMaritalStatuses2Async(It.IsAny<bool>())).ReturnsAsync(MaritalStatusList);

                var result = await MaritalStatusesController.GetMaritalStatuses2Async();
                Assert.AreEqual(result.Count(), allMaritalStatusEntities.Count());

                int count = allMaritalStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = MaritalStatusList[i];
                    var actual = allMaritalStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task MaritalStatusesController_GetHedmAsync_CacheControlNotNull()
            {
                MaritalStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetMaritalStatuses2Async(It.IsAny<bool>())).ReturnsAsync(MaritalStatusList);

                var result = await MaritalStatusesController.GetMaritalStatuses2Async();
                Assert.AreEqual(result.Count(), allMaritalStatusEntities.Count());

                int count = allMaritalStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = MaritalStatusList[i];
                    var actual = allMaritalStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task MaritalStatusesController_GetHedmAsync_NoCache()
            {
                MaritalStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                MaritalStatusesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetMaritalStatuses2Async(It.IsAny<bool>())).ReturnsAsync(MaritalStatusList);

                var result = await MaritalStatusesController.GetMaritalStatuses2Async();
                Assert.AreEqual(result.Count(), allMaritalStatusEntities.Count());

                int count = allMaritalStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = MaritalStatusList[i];
                    var actual = allMaritalStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task MaritalStatusesController_GetHedmAsync_Cache()
            {
                MaritalStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                MaritalStatusesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetMaritalStatuses2Async(It.IsAny<bool>())).ReturnsAsync(MaritalStatusList);

                var result = await MaritalStatusesController.GetMaritalStatuses2Async();
                Assert.AreEqual(result.Count(), allMaritalStatusEntities.Count());

                int count = allMaritalStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = MaritalStatusList[i];
                    var actual = allMaritalStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task MaritalStatusesController_GetByIdHedmAsync()
            {
                var thisMaritalStatus = MaritalStatusList.Where(m => m.Id == "87ec6f69-9b16-4ed5-8954-59067f0318ec").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetMaritalStatusById2Async(It.IsAny<string>())).ReturnsAsync(thisMaritalStatus);

                var race = await MaritalStatusesController.GetMaritalStatusById2Async("87ec6f69-9b16-4ed5-8954-59067f0318ec");
                Assert.AreEqual(thisMaritalStatus.Id, race.Id);
                Assert.AreEqual(thisMaritalStatus.Code, race.Code);
                Assert.AreEqual(thisMaritalStatus.Description, race.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task MaritalStatusesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetMaritalStatuses2Async(It.IsAny<bool>())).Throws<Exception>();

                await MaritalStatusesController.GetMaritalStatuses2Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task MaritalStatusesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetMaritalStatusById2Async(It.IsAny<string>())).Throws<Exception>();

                await MaritalStatusesController.GetMaritalStatusById2Async("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task MaritalStatusesController_PostThrowsIntAppiExc()
            {
                await MaritalStatusesController.PostMaritalStatusesAsync(MaritalStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RacesController_PutThrowsIntAppiExc()
            {
                var result = await MaritalStatusesController.PutMaritalStatusesAsync(maritalStatusesGuid, MaritalStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task MaritalStatusesController_DeleteThrowsIntAppiExc()
            {
                await MaritalStatusesController.DeleteMaritalStatusesAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a Marital Status domain entity to its corresponding Marital Status DTO
            /// </summary>
            /// <param name="source">Marital Status domain entity</param>
            /// <returns>MaritalStatus2 DTO</returns>
            private Dtos.MaritalStatus2 ConvertMaritalStatusEntitytoMaritalStatusDto(Domain.Base.Entities.MaritalStatus source)
            {
                var maritalStatus = new Dtos.MaritalStatus2();
                maritalStatus.Id = source.Guid;
                maritalStatus.Code = source.Code;
                maritalStatus.Title = source.Description;
                maritalStatus.Description = null;
                return maritalStatus;
            }
        }
    }
}