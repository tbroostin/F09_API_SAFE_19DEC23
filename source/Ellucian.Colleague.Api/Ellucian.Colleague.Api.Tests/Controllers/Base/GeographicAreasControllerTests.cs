//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class GeographicAreasControllerTests
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
        private Mock<ILogger> loggerMock;
        private Mock<IGeographicAreaService> geographicAreaServiceMock;

        private string geographicAreaId;

        private Dtos.GeographicArea expectedGeographicArea;
        private Dtos.GeographicArea testGeographicArea;
        private Dtos.GeographicArea actualGeographicArea;

        private GeographicAreasController geographicAreasController;


        public async Task<IHttpActionResult> getActualGeographicAreas()
        {
            //IEnumerable<GeographicArea> geographicAreaList = await geographicAreasController.GetGeographicAreasAsync();
            return (await geographicAreasController.GetGeographicAreasAsync(new Paging(0, 4)));
        }

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            geographicAreaServiceMock = new Mock<IGeographicAreaService>();

            geographicAreaId = "idc2935b-29e8-675f-907b-15a34da4f433";

            expectedGeographicArea = new Dtos.GeographicArea()
            {
                Id = "idc2935b-29e8-675f-907b-15a34da4f433",
                Code = "MIDW", 
                Title = "Midwestern US",
                Description = null,
                Type = new Dtos.GeographicAreaTypeProperty()
                {
                    category = Dtos.GeographicAreaTypeCategory.Governmental,
                    detail = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }
                }
            };

            testGeographicArea = new Dtos.GeographicArea();
            foreach (var property in typeof(Dtos.GeographicArea).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                property.SetValue(testGeographicArea, property.GetValue(expectedGeographicArea, null), null);
            }
            geographicAreaServiceMock.Setup<Task<Dtos.GeographicArea>>(s => s.GetGeographicAreaByGuidAsync(geographicAreaId)).Returns(Task.FromResult(testGeographicArea));

            geographicAreasController = new GeographicAreasController(geographicAreaServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            geographicAreasController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            geographicAreasController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            actualGeographicArea = await geographicAreasController.GetGeographicAreaByIdAsync(geographicAreaId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            geographicAreaServiceMock = null;
            geographicAreaId = null;
            expectedGeographicArea = null;
            testGeographicArea = null;
            actualGeographicArea = null;
            geographicAreasController = null;
        }

        [TestMethod]
        public void GeographicAreasTypeTest()
        {
            Assert.AreEqual(typeof(Dtos.GeographicArea), actualGeographicArea.GetType());
            Assert.AreEqual(expectedGeographicArea.GetType(), actualGeographicArea.GetType());
        }

        [TestMethod]
        public void NumberOfKnownPropertiesTest()
        {
            var geographicAreaProperties = typeof(Dtos.GeographicArea).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(7, geographicAreaProperties.Length);
        }
    }

    [TestClass]
    public class GeographicAreaController_GetAllTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private GeographicAreasController GeographicAreaController;
        private Mock<IGeographicAreaService> GeographicAreaServiceMock;
        private IGeographicAreaService GeographicAreaService;
        private IAdapterRegistry AdapterRegistry;
        private List<Ellucian.Colleague.Domain.Base.Entities.Chapter> allChaptersEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.County> allCountiesEntities;
        private List<Ellucian.Colleague.Domain.Base.Entities.ZipcodeXlat> allXipCodeXlatsEntities;
        ILogger logger = new Mock<ILogger>().Object;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            GeographicAreaServiceMock = new Mock<IGeographicAreaService>();
            GeographicAreaService = GeographicAreaServiceMock.Object;

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, logger);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Chapter, Dtos.GeographicArea>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);

            allChaptersEntities = new TestGeographicAreaRepository().GetChapters() as List<Ellucian.Colleague.Domain.Base.Entities.Chapter>;
            var GeographicAreasList = new List<Dtos.GeographicArea>();

            GeographicAreaController = new GeographicAreasController(GeographicAreaService, logger);
            GeographicAreaController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            GeographicAreaController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            foreach (var geographicArea in allChaptersEntities)
            {
                Dtos.GeographicArea target = ConvertChapterEntityToGeographicAreaDto(geographicArea);
                GeographicAreasList.Add(target);
            }

            var tuple = new Tuple<IEnumerable<Dtos.GeographicArea>, int>(GeographicAreasList, 4);
            GeographicAreaServiceMock.Setup(s => s.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tuple);
        }

        [TestCleanup]
        public void Cleanup()
        {
            GeographicAreaController = null;
            GeographicAreaService = null;
        }

        [TestMethod]
        public async Task ReturnsAllGeographicAreas()
        {
            var GeographicAreas = await GeographicAreaController.GetGeographicAreasAsync(new Paging(2, 0));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await GeographicAreas.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.GeographicArea> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.GeographicArea>;


            Assert.AreEqual(allChaptersEntities.Count, actuals.Count());
        }

        [TestMethod]
        public async Task GetGeographicAreasProperties()
        {
            var GeographicAreas = await GeographicAreaController.GetGeographicAreasAsync(new Paging(2, 0));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await GeographicAreas.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.GeographicArea> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.GeographicArea>;

            Dtos.GeographicArea ga = actuals.Where(a => a.Code == "BALT").FirstOrDefault();
            Ellucian.Colleague.Domain.Base.Entities.Chapter chp = allChaptersEntities.Where(a => a.Code == "BALT").FirstOrDefault();
            Assert.AreEqual(chp.Code, ga.Code);
            Assert.AreEqual(chp.Description, ga.Title);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an Chapter domain entity to its corresponding GeographicArea DTO
        /// </summary>
        /// <param name="source">Chapter domain entity</param>
        /// <returns>GeographicArea DTO</returns>
        private Dtos.GeographicArea ConvertChapterEntityToGeographicAreaDto(Chapter source)
        {
            var geographicArea = new Dtos.GeographicArea();
            geographicArea.Id = source.Guid;
            geographicArea.Code = source.Code;
            geographicArea.Title = source.Description;
            geographicArea.Description = null;
            geographicArea.Type = new Dtos.GeographicAreaTypeProperty()
            {
                category = Dtos.GeographicAreaTypeCategory.Fundraising,
                detail = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }
            };

            return geographicArea;
        }
    }

    [TestClass]
    public class GeographicAreasControllerTests2
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IGeographicAreaService> _geographicAreaServiceMock;
        private IGeographicAreaService _geographicAreaService;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private GeographicAreasController _geographicAreasController;

        private IEnumerable<Domain.Base.Entities.Chapter> _allChapters;
        private IEnumerable<Domain.Base.Entities.County> _allCounties;
        private IEnumerable<Domain.Base.Entities.ZipcodeXlat> _allZipCodeXlats;
        private List<Dtos.GeographicArea> _geographicAreasCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;

            _geographicAreaServiceMock = new Mock<IGeographicAreaService>();
            _geographicAreaService = _geographicAreaServiceMock.Object;
            _geographicAreasCollection = new List<Dtos.GeographicArea>();

            _allChapters = new TestGeographicAreaRepository().GetChapters();
            _allCounties = new TestGeographicAreaRepository().GetCounties();
            _allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();

            foreach (var source in _allChapters)
            {
                var geographicArea = new Ellucian.Colleague.Dtos.GeographicArea
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Fundraising,
                        detail = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }
                    }
                };

                _geographicAreasCollection.Add(geographicArea);
            }

            foreach (var source in _allCounties)
            {
                var geographicArea = new Ellucian.Colleague.Dtos.GeographicArea
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Governmental,
                        detail = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }
                    }
                };


                _geographicAreasCollection.Add(geographicArea);
            }

            foreach (var source in _allZipCodeXlats)
            {
                var geographicArea = new Ellucian.Colleague.Dtos.GeographicArea
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                    Type = new Dtos.GeographicAreaTypeProperty()
                    {
                        category = Dtos.GeographicAreaTypeCategory.Postal,
                        detail = new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() }
                    }
                };


                _geographicAreasCollection.Add(geographicArea);
            }

            _geographicAreasController = new GeographicAreasController(_geographicAreaService, _logger)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _geographicAreasController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _geographicAreasController = null;
            _refRepoMock = null;
            _refRepo = null;
            _geographicAreaService = null;
            _allCounties = null;
            _allZipCodeXlats = null;
            _allChapters = null;
            _geographicAreasCollection = null;
        }

        [TestMethod]
        public async Task GeographicAreasController_GetGeographicAreasAsync_ValidateFields_Nocache()
        {
            _geographicAreasController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue
                 {
                     NoCache = false,
                     Public = true
                 };

            var tuple = new Tuple<IEnumerable<Dtos.GeographicArea>, int>(_geographicAreasCollection, 4);

            _geographicAreaServiceMock.Setup(x => x.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(tuple);

            var geographicAreas = (await _geographicAreasController.GetGeographicAreasAsync(new Paging(4, 0)));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await geographicAreas.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.GeographicArea> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.GeographicArea>;


            Assert.AreEqual(_geographicAreasCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = _geographicAreasCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Code, actual.Code);
            }
        }

        [TestMethod]
        public async Task GeographicAreaAcademicCredentialsController_GetGeographicAreasAsync_ValidateFields_Cache()
        {
            _geographicAreasController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue
                 {
                     NoCache = true,
                     Public = true
                 };

            var tuple = new Tuple<IEnumerable<Dtos.GeographicArea>, int>(_geographicAreasCollection, 4);

            _geographicAreaServiceMock.Setup(x => x.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tuple);

            var geographicAreas = (await _geographicAreasController.GetGeographicAreasAsync(new Paging(4, 0)));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await geographicAreas.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.GeographicArea> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.GeographicArea>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.GeographicArea>;


            Assert.AreEqual(_geographicAreasCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = _geographicAreasCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Code, actual.Code);
            }
        }

        [TestMethod]
        public async Task GeographicAreasController_GetGeographicAreaByGuidAsync_ValidateFields()
        {
            var expected = _geographicAreasCollection.FirstOrDefault();
            _geographicAreaServiceMock.Setup(x => x.GetGeographicAreaByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await _geographicAreasController.GetGeographicAreaByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeographicAreasController_GetGeographicAreasAsync_Exception()
        {
            _geographicAreaServiceMock.Setup(x => x.GetGeographicAreasAsync(It.IsAny<int>(), It.IsAny<int>(), false)).Throws<Exception>();
            await _geographicAreasController.GetGeographicAreasAsync(new Paging(0, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeographicAreasController_GetGeographicAreaByGuidAsync_Exception()
        {
            _geographicAreaServiceMock.Setup(x => x.GetGeographicAreaByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _geographicAreasController.GetGeographicAreaByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeographicAreasController_PostGeographicArea()
        {
            await _geographicAreasController.PostGeographicAreaAsync(_geographicAreasCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeographicAreasController_PutGeographicArea()
        {
            var geographicArea = _geographicAreasCollection.FirstOrDefault();
            await _geographicAreasController.PutGeographicAreaAsync(geographicArea);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeographicAreasController_DeleteGeographicArea()
        {
            await _geographicAreasController.DeleteGeographicAreaAsync(_geographicAreasCollection.FirstOrDefault().Id);
        }
    }
}
