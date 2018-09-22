// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RacesControllerTests
    {
        [TestClass]
        public class RacesControllerGet
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

            private RacesController RacesController;
            private Mock<IReferenceDataRepository> RaceRepositoryMock;
            private IReferenceDataRepository RaceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Race> allRaceEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<Race2> RaceList;
            private string racesGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                RaceRepositoryMock = new Mock<IReferenceDataRepository>();
                RaceRepository = RaceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Race, Race2>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allRaceEntities = new TestRaceRepository().Get();
                RaceList = new List<Race2>();

                RacesController = new RacesController(AdapterRegistry, RaceRepository, demographicsService, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Race, Race2>();
                RacesController.Request = new HttpRequestMessage();
                RacesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var races in allRaceEntities)
                {
                    Race2 target = ConvertRaceEntitytoRaceDto(races);
                    RaceList.Add(target);
                }
                RaceRepositoryMock.Setup(x => x.RacesAsync()).ReturnsAsync(allRaceEntities);
                RaceRepositoryMock.Setup(x => x.GetRacesAsync(false)).ReturnsAsync(allRaceEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                RacesController = null;
                RaceRepository = null;
            }

            [TestMethod]
            public async Task ReturnsAllRacesAsync()
            {
                var Races = await RacesController.GetAsync();
                Assert.AreEqual(Races.Count(), allRaceEntities.Count());
            }

            [TestMethod]
            public async Task GetRacesByGuidAsync_Validate()
            {
                var thisRace = RaceList.Where(m => m.Id == racesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetRaceById2Async(It.IsAny<string>())).ReturnsAsync(thisRace);

                var race = await RacesController.GetRaceById2Async(racesGuid);
                Assert.AreEqual(thisRace.Id, race.Id);
                Assert.AreEqual(thisRace.Code, race.Code);
                Assert.AreEqual(thisRace.Description, race.Description);
                //Assert.AreEqual(thisRace.RaceReporting[0].RaceReportingCountry.CountryCode, race.RaceReporting[0].RaceReportingCountry.CountryCode);
                //Assert.AreEqual(thisRace.RaceReporting[0].RaceReportingCountry.ReportingRacialCategory, race.RaceReporting[0].RaceReportingCountry.ReportingRacialCategory);
            }

            [TestMethod]
            public async Task RacesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetRaces2Async(It.IsAny<bool>())).ReturnsAsync(RaceList);

                var result = await RacesController.GetRaces2Async();
                Assert.AreEqual(result.Count(), allRaceEntities.Count());

                int count = allRaceEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RaceList[i];
                    var actual = allRaceEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RacesController_GetHedmAsync_CacheControlNotNull()
            {
                RacesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetRaces2Async(It.IsAny<bool>())).ReturnsAsync(RaceList);

                var result = await RacesController.GetRaces2Async();
                Assert.AreEqual(result.Count(), allRaceEntities.Count());

                int count = allRaceEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RaceList[i];
                    var actual = allRaceEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RacesController_GetHedmAsync_NoCache()
            {
                RacesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                RacesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetRaces2Async(It.IsAny<bool>())).ReturnsAsync(RaceList);

                var result = await RacesController.GetRaces2Async();
                Assert.AreEqual(result.Count(), allRaceEntities.Count());

                int count = allRaceEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RaceList[i];
                    var actual = allRaceEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RacesController_GetHedmAsync_Cache()
            {
                RacesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                RacesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetRaces2Async(It.IsAny<bool>())).ReturnsAsync(RaceList);

                var result = await RacesController.GetRaces2Async();
                Assert.AreEqual(result.Count(), allRaceEntities.Count());

                int count = allRaceEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = RaceList[i];
                    var actual = allRaceEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task RacesController_GetByIdHedmAsync()
            {
                var thisRace = RaceList.Where(m => m.Id == "87ec6f69-9b16-4ed5-8954-59067f0318ec").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetRaceById2Async(It.IsAny<string>())).ReturnsAsync(thisRace);

                var race = await RacesController.GetRaceById2Async("87ec6f69-9b16-4ed5-8954-59067f0318ec");
                Assert.AreEqual(thisRace.Id, race.Id);
                Assert.AreEqual(thisRace.Code, race.Code);
                Assert.AreEqual(thisRace.Description, race.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RacesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetRaces2Async(It.IsAny<bool>())).Throws<Exception>();

                await RacesController.GetRaces2Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RacesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetRaceById2Async(It.IsAny<string>())).Throws<Exception>();

                await RacesController.GetRaceById2Async("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RacesController_PostThrowsIntAppiExc()
            {
                RacesController.PostRaces(RaceList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RacesController_PutThrowsIntAppiExc()
            {
                var result = RacesController.PutRaces(RaceList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void RacesController_DeleteThrowsIntAppiExc()
            {
                RacesController.DeleteRaces("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a Race domain entity to its corresponding Race DTO
            /// </summary>
            /// <param name="source">Race domain entity</param>
            /// <returns>Race2 DTO</returns>
            private Dtos.Race2 ConvertRaceEntitytoRaceDto(Domain.Base.Entities.Race source)
            {
                var race = new Dtos.Race2();
                race.Id = source.Guid;
                race.Code = source.Code;
                race.Title = source.Description;
                race.Description = null;
                race.RaceReporting = new List<RaceReporting>() 
                {
                   new RaceReporting()
                   {
                    RaceReportingCountry = new RaceReportingCountry()
                    {
                        CountryCode = CountryCodeType.USA,
                        ReportingRacialCategory = ConvertRaceTypeDomainEnumToReportingRacialCategoryDtoEnum(source.Type)
                    }
                   }
                };
                return race;
            }

            private Ellucian.Colleague.Dtos.EnumProperties.ReportingRacialCategory? ConvertRaceTypeDomainEnumToReportingRacialCategoryDtoEnum(Ellucian.Colleague.Domain.Base.Entities.RaceType? source)
            {
                switch (source)
                {
                    case Domain.Base.Entities.RaceType.AmericanIndian:
                        return ReportingRacialCategory.AmericanIndianOrAlaskaNative;
                    case Domain.Base.Entities.RaceType.Asian:
                        return ReportingRacialCategory.Asian;
                    case Domain.Base.Entities.RaceType.Black:
                        return ReportingRacialCategory.BlackOrAfricanAmerican;
                    case Domain.Base.Entities.RaceType.PacificIslander:
                        return ReportingRacialCategory.HawaiianOrPacificIslander;
                    case Domain.Base.Entities.RaceType.White:
                        return ReportingRacialCategory.White;
                    default:
                        return null;
                }
            }
        }
    }
}