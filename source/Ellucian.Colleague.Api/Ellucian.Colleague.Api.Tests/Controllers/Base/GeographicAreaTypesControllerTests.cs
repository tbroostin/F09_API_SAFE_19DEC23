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
    public class GeographicAreaTypesControllerTests
    {
        [TestClass]
        public class GeographicAreaTypesControllerGet
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

            private GeographicAreaTypesController GeographicAreaTypesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;   
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<GeographicAreaType> GeographicAreaTypeList;
            private string geographicAreaTypesGuid = "87ec6f69-9b16-4ed5-8954-59067f0318ec";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.GeographicAreaType, GeographicAreaType>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allGeographicAreaTypeEntities = new TestGeographicAreaTypeRepository().Get();
                GeographicAreaTypeList = new List<GeographicAreaType>();

                GeographicAreaTypesController = new GeographicAreaTypesController(demographicsService, logger);
                GeographicAreaTypesController.Request = new HttpRequestMessage();
                GeographicAreaTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var geographicAreaType in allGeographicAreaTypeEntities)
                {
                    GeographicAreaType target = ConvertGeographicAreaTypeEntityToDto(geographicAreaType);
                    GeographicAreaTypeList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allGeographicAreaTypeEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                GeographicAreaTypesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetGeographicAreaTypesByGuidAsync_Validate()
            {
                var thisGeographicAreaType = GeographicAreaTypeList.Where(m => m.Id == geographicAreaTypesGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetGeographicAreaTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisGeographicAreaType);

                var geographicAreaType = await GeographicAreaTypesController.GetGeographicAreaTypeByIdAsync(geographicAreaTypesGuid);
                Assert.AreEqual(thisGeographicAreaType.Id, geographicAreaType.Id);
                Assert.AreEqual(thisGeographicAreaType.Code, geographicAreaType.Code);
                Assert.AreEqual(thisGeographicAreaType.Description, geographicAreaType.Description);
                Assert.AreEqual(thisGeographicAreaType.geographicAreaTypeCategory, geographicAreaType.geographicAreaTypeCategory);
            }

            [TestMethod]
            public async Task GeographicAreaTypesController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(GeographicAreaTypeList);

                var result = await GeographicAreaTypesController.GetGeographicAreaTypesAsync();
                Assert.AreEqual(result.Count(), allGeographicAreaTypeEntities.Count());

                int count = allGeographicAreaTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = GeographicAreaTypeList[i];
                    var actual = allGeographicAreaTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task GeographicAreaTypesController_GetHedmAsync_CacheControlNotNull()
            {
                GeographicAreaTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(GeographicAreaTypeList);

                var result = await GeographicAreaTypesController.GetGeographicAreaTypesAsync();
                Assert.AreEqual(result.Count(), allGeographicAreaTypeEntities.Count());

                int count = allGeographicAreaTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = GeographicAreaTypeList[i];
                    var actual = allGeographicAreaTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task GeographicAreaTypesController_GetHedmAsync_NoCache()
            {
                GeographicAreaTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                GeographicAreaTypesController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(GeographicAreaTypeList);

                var result = await GeographicAreaTypesController.GetGeographicAreaTypesAsync();
                Assert.AreEqual(result.Count(), allGeographicAreaTypeEntities.Count());

                int count = allGeographicAreaTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = GeographicAreaTypeList[i];
                    var actual = allGeographicAreaTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task GeographicAreaTypesController_GetHedmAsync_Cache()
            {
                GeographicAreaTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                GeographicAreaTypesController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypesAsync(It.IsAny<bool>())).ReturnsAsync(GeographicAreaTypeList);

                var result = await GeographicAreaTypesController.GetGeographicAreaTypesAsync();
                Assert.AreEqual(result.Count(), allGeographicAreaTypeEntities.Count());

                int count = allGeographicAreaTypeEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = GeographicAreaTypeList[i];
                    var actual = allGeographicAreaTypeEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task GeographicAreaTypesController_GetByIdHedmAsync()
            {
                var thisGeographicAreaType = GeographicAreaTypeList.Where(m => m.Id == "87ec6f69-9b16-4ed5-8954-59067f0318ec").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetGeographicAreaTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisGeographicAreaType);

                var geographicAreaType = await GeographicAreaTypesController.GetGeographicAreaTypeByIdAsync("87ec6f69-9b16-4ed5-8954-59067f0318ec");
                Assert.AreEqual(thisGeographicAreaType.Id, geographicAreaType.Id);
                Assert.AreEqual(thisGeographicAreaType.Code, geographicAreaType.Code);
                Assert.AreEqual(thisGeographicAreaType.Description, geographicAreaType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypesAsync(It.IsAny<bool>())).Throws<Exception>();

                await GeographicAreaTypesController.GetGeographicAreaTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await GeographicAreaTypesController.GetGeographicAreaTypeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_GetByIdThrowsKeyNotFoundException()
            {
                demographicsServiceMock.Setup(gc => gc.GetGeographicAreaTypeByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await GeographicAreaTypesController.GetGeographicAreaTypeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_PostThrowsIntAppiExc()
            {
                await GeographicAreaTypesController.PostGeographicAreaTypeAsync(GeographicAreaTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_PutThrowsIntAppiExc()
            {
                var result = await GeographicAreaTypesController.PutGeographicAreaTypeAsync(GeographicAreaTypeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeographicAreaTypesController_DeleteThrowsIntAppiExc()
            {
                await GeographicAreaTypesController.DeleteGeographicAreaTypeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a GeographicAreaType domain entity to its corresponding GeographicAreaType DTO
            /// </summary>
            /// <param name="source">GeographicAreaType domain entity</param>
            /// <returns>GeographicAreaType DTO</returns>
            private Ellucian.Colleague.Dtos.GeographicAreaType ConvertGeographicAreaTypeEntityToDto(Ellucian.Colleague.Domain.Base.Entities.GeographicAreaType source)
            {
                var geographicAreaType = new Ellucian.Colleague.Dtos.GeographicAreaType();
                geographicAreaType.Id = source.Guid;
                geographicAreaType.Code = source.Code;
                geographicAreaType.Title = source.Description;
                geographicAreaType.Description = null;

                return geographicAreaType;
            }
        }
    }
}