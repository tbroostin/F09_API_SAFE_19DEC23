//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RegionIsoCodesControllerTests
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

        private IReferenceDataRepository referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;

        private ICountriesService countriesService;
        private Mock<ICountriesService> countriesServiceMock;

        private IRegionIsoCodesService regionIsoCodesService;
        private Mock<IRegionIsoCodesService> regionIsoCodesServiceMock;

        private RegionIsoCodesController regionIsoCodesController;

        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        private List<Dtos.RegionIsoCodes> regionIsoCodesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            countriesServiceMock = new Mock<ICountriesService>();
            countriesService = countriesServiceMock.Object;
            regionIsoCodesServiceMock = new Mock<IRegionIsoCodesService>();
            regionIsoCodesService = regionIsoCodesServiceMock.Object;


            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;
            
            regionIsoCodesCollection = new List<RegionIsoCodes>();
            var allRegionPlaces = new List<Domain.Base.Entities.Place>()
                {
                    new Domain.Base.Entities.Place("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    { PlacesCountry = "USA", PlacesDesc = "Alaska", PlacesRegion = "US-AK"},
                    new Domain.Base.Entities.Place("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                    { PlacesCountry = "CAN", PlacesDesc = "Alberta", PlacesRegion = "CA-AB" },
                    new Domain.Base.Entities.Place("d2253ac7-9931-4560-b42f-1fccd43c952e")
                    { PlacesCountry = "RUS", PlacesDesc = "Adygeya", PlacesRegion = "RU-AD" },
                };

            foreach (var source in allRegionPlaces)
            {
                var regionIsoCodes = new Ellucian.Colleague.Dtos.RegionIsoCodes
                {
                    Id = source.Guid,
                    Title = source.PlacesDesc,
                    ISOCode = source.PlacesRegion,
                    Status = Dtos.EnumProperties.Status.Active
                };
                regionIsoCodesCollection.Add(regionIsoCodes);
            }

            regionIsoCodesController = new RegionIsoCodesController(regionIsoCodesService, logger)
            {
                Request = new HttpRequestMessage()
            };
            regionIsoCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            regionIsoCodesController = null;
        }


        [TestMethod]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_ValidateFields_Nocache()
        {
            regionIsoCodesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false)).ReturnsAsync(regionIsoCodesCollection);

            var sourceContexts = (await regionIsoCodesController.GetRegionIsoCodesAsync(null)).ToList();
            Assert.AreEqual(regionIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = regionIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "ISOCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_ValidateFields_Cache()
        {
            regionIsoCodesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), true)).ReturnsAsync(regionIsoCodesCollection);

            var sourceContexts = (await regionIsoCodesController.GetRegionIsoCodesAsync(null)).ToList();
            Assert.AreEqual(regionIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = regionIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "ISOCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_KeyNotFoundException()
        {            
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false))
                .Throws<KeyNotFoundException>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_PermissionsException()
        {

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false))
                .Throws<PermissionsException>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_ArgumentException()
        {

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false))
                .Throws<ArgumentException>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_RepositoryException()
        {

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false))
                .Throws<RepositoryException>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_IntegrationApiException()
        {

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false))
                .Throws<IntegrationApiException>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuidAsync_ValidateFields()
        {
            var expected = regionIsoCodesCollection.FirstOrDefault();
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.ISOCode, actual.ISOCode, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodes_Exception()
        {
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesAsync(It.IsAny<RegionIsoCodes>(), false)).Throws<Exception>();
            await regionIsoCodesController.GetRegionIsoCodesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuidAsync_Exception()
        {
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_KeyNotFoundException()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_PermissionsException()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;

            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_ArgumentException()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_RepositoryException()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_IntegrationApiException()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_GetRegionIsoCodesByGuid_Exception()
        {
            var expectedGuid = regionIsoCodesCollection.FirstOrDefault().Id;
            regionIsoCodesServiceMock.Setup(x => x.GetRegionIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await regionIsoCodesController.GetRegionIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_PostRegionIsoCodesAsync_Exception()
        {
            await regionIsoCodesController.PostRegionIsoCodesAsync(regionIsoCodesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_PutRegionIsoCodesAsync_Exception()
        {
            var sourceContext = regionIsoCodesCollection.FirstOrDefault();
            await regionIsoCodesController.PutRegionIsoCodesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RegionIsoCodesController_DeleteRegionIsoCodesAsync_Exception()
        {
            await regionIsoCodesController.DeleteRegionIsoCodesAsync(regionIsoCodesCollection.FirstOrDefault().Id);
        }
    }
}
