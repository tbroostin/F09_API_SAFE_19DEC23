// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CountriesControllerTests
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

        private CountriesController countriesController;
        private IEnumerable<Domain.Base.Entities.Country> countries;
        private IEnumerable<Domain.Base.Entities.Country> countries2;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;


        private List<Dtos.Countries> countriesCollection;
       
        private List<Dtos.CountryIsoCodes> countryIsoCodesCollection;


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            countriesServiceMock = new Mock<ICountriesService>();
            countriesService = countriesServiceMock.Object;


            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            countries = BuildCountries();
            countries2 = BuildCountries2();
            countriesCollection = new List<Dtos.Countries>();
            foreach (var source in countries2)
            {
                var countryDto = new Ellucian.Colleague.Dtos.Countries
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    ISOCode = source.IsoAlpha3Code
                };
                countriesCollection.Add(countryDto);
            }

            countryIsoCodesCollection = new List<CountryIsoCodes>();
            var allPlaces = new List<Domain.Base.Entities.Place>()
                {
                    new Domain.Base.Entities.Place("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    { PlacesCountry = "USA", PlacesDesc = "United States" },
                    new Domain.Base.Entities.Place("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                    { PlacesCountry = "CAN", PlacesDesc = "Canada" },
                    new Domain.Base.Entities.Place("d2253ac7-9931-4560-b42f-1fccd43c952e")
                    { PlacesCountry = "RUS", PlacesDesc = "Russia" },
                };

            foreach (var source in allPlaces)
            {
                var countryIsoCodes = new Ellucian.Colleague.Dtos.CountryIsoCodes
                {
                    Id = source.Guid,
                    Title = source.PlacesDesc,
                    ISOCode = source.PlacesCountry,
                    Status = Dtos.EnumProperties.Status.Active
                };
                countryIsoCodesCollection.Add(countryIsoCodes);
            }

            countriesController = new CountriesController(countriesService, adapterRegistry, referenceDataRepository, logger)
            {
                Request = new HttpRequestMessage()
            };
            countriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            countriesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task CountriesController_ReturnsCountriesReasonDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(countries));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Dtos.Base.Country>);
            Assert.AreEqual(countries.Count(), countryDtos.Count());
        }

        [TestMethod]
        public async Task CountriesController_NullRepositoryResponse_ReturnsEmptyCountriesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.Country> nullCountryEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(nullCountryEntities));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Dtos.Base.Country>);
            Assert.AreEqual(0, countryDtos.Count());
        }

        [TestMethod]
        public async Task CountriesController_EmptyRepositoryResponse_ReturnsEmptyCountriesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.Country> emptyCountryEntities = new List<Domain.Base.Entities.Country>();
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(emptyCountryEntities));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Dtos.Base.Country>);
            Assert.AreEqual(0, countryDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Throws(new ApplicationException());
                var countryDtos = await countriesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        [TestMethod]
        public async Task CountriesController_GetCountries_ValidateFields_Nocache()
        {
            countriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            countriesServiceMock.Setup(x => x.GetCountriesAsync(false)).ReturnsAsync(countriesCollection);

            var sourceContexts = (await countriesController.GetCountriesAsync()).ToList();
            Assert.AreEqual(countriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = countriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CountriesController_GetCountries_ValidateFields_Cache()
        {
            countriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            countriesServiceMock.Setup(x => x.GetCountriesAsync(true)).ReturnsAsync(countriesCollection);

            var sourceContexts = (await countriesController.GetCountriesAsync()).ToList();
            Assert.AreEqual(countriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = countriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_KeyNotFoundException()
        {
            //
            countriesServiceMock.Setup(x => x.GetCountriesAsync(false))
                .Throws<KeyNotFoundException>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_PermissionsException()
        {

            countriesServiceMock.Setup(x => x.GetCountriesAsync(false))
                .Throws<PermissionsException>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_ArgumentException()
        {

         
            countriesServiceMock.Setup(x => x.GetCountriesAsync(false))
                .Throws<ArgumentException>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_RepositoryException()
        {

            countriesServiceMock.Setup(x => x.GetCountriesAsync(false))
                .Throws<RepositoryException>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_IntegrationApiException()
        {

            countriesServiceMock.Setup(x => x.GetCountriesAsync(false))
                .Throws<IntegrationApiException>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        public async Task CountriesController_GetCountriesByGuidAsync_ValidateFields()
        {
            var expected = countriesCollection.FirstOrDefault();
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await countriesController.GetCountriesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountries_Exception()
        {
            countriesServiceMock.Setup(x => x.GetCountriesAsync(false)).Throws<Exception>();
            await countriesController.GetCountriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuidAsync_Exception()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await countriesController.GetCountriesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_KeyNotFoundException()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_PermissionsException()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_ArgumentException()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_RepositoryException()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_IntegrationApiException()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountriesByGuid_Exception()
        {
            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await countriesController.GetCountriesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_PostCountriesAsync_Exception()
        {
            await countriesController.PostCountriesAsync(countriesCollection.FirstOrDefault());
        }

        [TestMethod]     
        public async Task CountriesController_PutCountriesAsync()
        {
            var expected = countriesCollection.FirstOrDefault();

            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            countriesServiceMock.Setup(x => x.PutCountriesAsync(expected.Id, It.IsAny<Countries>())).ReturnsAsync(expected);

            var actual   = await countriesController.PutCountriesAsync(expected.Id, expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");           
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_PutCountriesAsync_InvalidKey()
        {
            var expected = countriesCollection.FirstOrDefault();

            countriesServiceMock.Setup(x => x.PutCountriesAsync(expected.Id, It.IsAny<Countries>()))
                    .Throws<KeyNotFoundException>();

            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await countriesController.PutCountriesAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_PutCountriesAsync_RepositoryExcecption()
        {
            var expected = countriesCollection.FirstOrDefault();

            countriesServiceMock.Setup(x => x.GetCountriesByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<RepositoryException>();

            await countriesController.PutCountriesAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_DeleteCountriesAsync_Exception()
        {
            await countriesController.DeleteCountriesAsync(countriesCollection.FirstOrDefault().Id);
        }


        [TestMethod]
        public async Task CountriesController_GetCountryIsoCodes_ValidateFields_Nocache()
        {
            countriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false)).ReturnsAsync(countryIsoCodesCollection);

            var sourceContexts = (await countriesController.GetCountryIsoCodesAsync()).ToList();
            Assert.AreEqual(countryIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = countryIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "ISOCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CountriesController_GetCountryIsoCodes_ValidateFields_Cache()
        {
            countriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(true)).ReturnsAsync(countryIsoCodesCollection);

            var sourceContexts = (await countriesController.GetCountryIsoCodesAsync()).ToList();
            Assert.AreEqual(countryIsoCodesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = countryIsoCodesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "ISOCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_KeyNotFoundException()
        {
            //
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false))
                .Throws<KeyNotFoundException>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_PermissionsException()
        {

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false))
                .Throws<PermissionsException>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_ArgumentException()
        {

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false))
                .Throws<ArgumentException>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_RepositoryException()
        {

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false))
                .Throws<RepositoryException>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_IntegrationApiException()
        {

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false))
                .Throws<IntegrationApiException>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        public async Task CountriesController_GetCountryIsoCodesByGuidAsync_ValidateFields()
        {
            var expected = countryIsoCodesCollection.FirstOrDefault();
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await countriesController.GetCountryIsoCodesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.ISOCode, actual.ISOCode, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodes_Exception()
        {
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesAsync(false)).Throws<Exception>();
            await countriesController.GetCountryIsoCodesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuidAsync_Exception()
        {
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await countriesController.GetCountryIsoCodesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_KeyNotFoundException()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_PermissionsException()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;

            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_ArgumentException()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_RepositoryException()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_IntegrationApiException()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_GetCountryIsoCodesByGuid_Exception()
        {
            var expectedGuid = countryIsoCodesCollection.FirstOrDefault().Id;
            countriesServiceMock.Setup(x => x.GetCountryIsoCodesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await countriesController.GetCountryIsoCodesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_PostCountryIsoCodesAsync_Exception()
        {
            await countriesController.PostCountryIsoCodesAsync(countryIsoCodesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_PutCountryIsoCodesAsync_Exception()
        {
            var sourceContext = countryIsoCodesCollection.FirstOrDefault();
            await countriesController.PutCountryIsoCodesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_DeleteCountryIsoCodesAsync_Exception()
        {
            await countriesController.DeleteCountryIsoCodesAsync(countryIsoCodesCollection.FirstOrDefault().Id);
        }

        private IEnumerable<Domain.Base.Entities.Country> BuildCountries()
        {
            var countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("LIFE", "Life Experience", "iso1"),
                    new Domain.Base.Entities.Country("OTHER", "Other reason", "iso2")
                };

            return countries;
        }

        private IEnumerable<Domain.Base.Entities.Country> BuildCountries2()
        {
            var countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(), "UK", "United Kingdom", "", "GBR"),
                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(), "USA", "United States", "", "USA"),
                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(), "CA", "Canada", "", "CAN")
                };

            return countries;
        }
    }
}
