//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CountriesServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class CountriesServiceTests_GET
        {
            private const string countriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string countriesCode = "US";
            private List<Countries> _countriesDtoCollection;
            private List<Domain.Base.Entities.Country> _countriesEntityCollection;
            private CountriesService _countriesService;

            private Mock<ICountryRepository> _countryRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;

            [TestInitialize]
            public async void Initialize()
            {
                _countryRepositoryMock = new Mock<ICountryRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

                _countriesEntityCollection = new List<Domain.Base.Entities.Country>()
                {

                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(),"US","United States","US", "USA",false),
                    new Domain.Base.Entities.Country(countriesGuid, "CA","Canada","CA","CAN", false),
                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(), "MX","Mexico","MX","MEX", false),

                };
                _countriesDtoCollection = new List<Dtos.Countries>();
                foreach (var source in _countriesEntityCollection)
                {
                    var countries = new Ellucian.Colleague.Dtos.Countries
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        ISOCode = source.IsoCode
                    };
                    _countriesDtoCollection.Add(countries);
                }

                _countryRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_countriesEntityCollection);

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(_countriesEntityCollection.FirstOrDefault(c => c.Guid.Equals(countriesGuid)));

                _countriesService = new CountriesService(_referenceDataRepositoryMock.Object,
                    _adapterRegistryMock.Object,
                     _currentUserFactoryMock.Object,
                    _roleRepositoryMock.Object,
                    _countryRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _countriesService = null;
                _countriesDtoCollection = null;
                _countryRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CountriesService_GetCountriesAsync()
            {
                var results = await _countriesService.GetCountriesAsync(true);
                Assert.IsTrue(results is IEnumerable<Countries>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CountriesService_GetCountriesAsync_Count()
            {
                var results = await _countriesService.GetCountriesAsync(true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CountriesService_GetCountriesAsync_Properties()
            {
                var result =
                    (await _countriesService.GetCountriesAsync(true)).FirstOrDefault(x => x.Code == countriesCode);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);

            }

            [TestMethod]
            public async Task CountriesService_GetCountriesAsync_Expected()
            {
                var expectedResults = _countriesDtoCollection.FirstOrDefault(c => c.Id == countriesGuid);
                var actualResult =
                    (await _countriesService.GetCountriesAsync(true)).FirstOrDefault(x => x.Id == countriesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
                Assert.AreEqual(expectedResults.Code, actualResult.Code);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CountriesService_GetCountriesAsync_RepositoryException_RepoError()
            {
                _countryRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await _countriesService.GetCountriesAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CountriesService_GetCountriesByGuidAsync_Empty()
            {
                await _countriesService.GetCountriesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CountriesService_GetCountriesByGuidAsync_Null()
            {
                await _countriesService.GetCountriesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_GetCountriesByGuidAsync_InvalidId()
            {
                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _countriesService.GetCountriesByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CountriesService_GetCountriesByGuidAsync_RepositoryException_RepoError()
            {
                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(It.IsAny<string>()))
                    .Throws<RepositoryException>();

                await _countriesService.GetCountriesByGuidAsync("99");
            }

            [TestMethod]
            public async Task CountriesService_GetCountriesByGuidAsync_Expected()
            {
                var expectedResults =
                    _countriesDtoCollection.First(c => c.Id == countriesGuid);
                var actualResult =
                    await _countriesService.GetCountriesByGuidAsync(countriesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
                Assert.AreEqual(expectedResults.Code, actualResult.Code);

            }

            [TestMethod]
            public async Task CountriesService_GetCountriesByGuidAsync_Properties()
            {
                var result =
                    await _countriesService.GetCountriesByGuidAsync(countriesGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }
        }

        [TestClass]
        public class CountriesServiceTests_PUT : CurrentUserSetup
        {
            private const string countriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string countriesCode = "US";
            private List<Countries> _countriesDtoCollection;
            private List<Domain.Base.Entities.Country> _countriesEntityCollection;
            private CountriesService _countriesService;

            private Mock<ICountryRepository> _countryRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IRoleRepository _roleRepository;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                _countryRepositoryMock = new Mock<ICountryRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock permissions               
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdateCountry));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });


                _countriesEntityCollection = new List<Domain.Base.Entities.Country>()
                {
                     new Domain.Base.Entities.Country(Guid.NewGuid().ToString(),"US","United States","US", "USA",false),
                    new Domain.Base.Entities.Country(countriesGuid, "CA","Canada","CA","CAN", false),
                    new Domain.Base.Entities.Country(Guid.NewGuid().ToString(), "MX","Mexico","MX","MEX", false),
                 };

                _countriesDtoCollection = new List<Dtos.Countries>();
                foreach (var source in _countriesEntityCollection)
                {
                    var countries = new Ellucian.Colleague.Dtos.Countries
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        ISOCode = source.IsoCode
                    };
                    _countriesDtoCollection.Add(countries);
                }

                _countryRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_countriesEntityCollection);

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(_countriesEntityCollection.FirstOrDefault(c => c.Guid.Equals(countriesGuid)));

                _countriesService = new CountriesService(_referenceDataRepositoryMock.Object,
                    _adapterRegistryMock.Object,
                     _currentUserFactory,
                    _roleRepositoryMock.Object,
                    _countryRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _countriesService = null;
                _countriesDtoCollection = null;
                _countryRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CountriesService_PutCountriesAsync_Properties()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(currencyEntity);

                var result =
                    await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);

                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }

            [TestMethod]
            public async Task CountriesService_PutCountriesAsync_Response()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(currencyEntity);

                var result =
                    await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);

                Assert.AreEqual(currencyDto.Id, result.Id, "id");
                Assert.AreEqual(currencyDto.Code, result.Code, "code");
                Assert.AreEqual(currencyDto.Title, result.Title, "title");

            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_ChangedCode()
            {
                var existing = _countriesEntityCollection.FirstOrDefault(c => c.Guid.Equals(countriesGuid));

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                currencyDto.Code = "changed";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_ChangedTitle()
            {
                var existing = _countriesEntityCollection.FirstOrDefault(c => c.Guid.Equals(countriesGuid));

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                currencyDto.Title = "changed";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_InvalidISOCode()
            {
                var existing = _countriesEntityCollection.FirstOrDefault(c => c.Guid.Equals(countriesGuid));

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                currencyDto.ISOCode = "1234";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_MissingGuid()
            {
      
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                currencyDto.Id = "";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_GuidNotFound()
            {
                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                    .ReturnsAsync(null);

                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CountriesService_PutCountriesAsync_ArgumentNullException_EmptyGuid()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                await _countriesService.PutCountriesAsync("", currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CountriesService_PutCountriesAsync_ArgumentNullException_EmptyBody()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                await _countriesService.PutCountriesAsync(currencyDto.Id, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_RepoError()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>()))
                    .ThrowsAsync(new RepositoryException());

                var result =
                    await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CountriesService_PutCountriesAsync_PermissionsException()
            {
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                facultyRole.RemovePermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdateCurrency));
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(currencyEntity);

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_EmptyCode()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(currencyEntity);

                currencyDto.Code = "";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_IntegrationApiException_EmptyTitle()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(currencyEntity);

                currencyDto.Title = "";

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_PutCountriesAsync_ExistingRecordNotFound()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                _countryRepositoryMock.Setup(repo => repo.GetCountryByGuidAsync(countriesGuid))
                   .ThrowsAsync(new KeyNotFoundException());


                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>()))
                    .ReturnsAsync(currencyEntity);

                var result =
                    await _countriesService.PutCountriesAsync("invalid", currencyDto);

            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CountriesService_PutCountriesAsync_UpdateCountryersionAsync_null()
            {
                var currencyDto = _countriesDtoCollection.FirstOrDefault(x => x.Id == countriesGuid);

                var currencyEntity = _countriesEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid);
                _countryRepositoryMock.Setup(repo => repo.UpdateCountryAsync(It.IsAny<Domain.Base.Entities.Country>())).ReturnsAsync(null);

                await _countriesService.PutCountriesAsync(countriesGuid, currencyDto);
            }
        }

        [TestClass]
        public class CountriesServiceTests_CountryIsoCodes
        {
            private const string countriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string countriesCode = "US";
            private List<CountryIsoCodes> _countryIsoDtoCollection;
            private List<Domain.Base.Entities.Place> _placeEntityCollection;
            private CountriesService _countriesService;

            private Mock<ICountryRepository> _countryRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IReferenceDataRepository> _referenceRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                _countryRepositoryMock = new Mock<ICountryRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();

                _placeEntityCollection = new List<Domain.Base.Entities.Place>()
                {
                    new Domain.Base.Entities.Place(Guid.NewGuid().ToString()) { PlacesCountry =  "MX", PlacesDesc = "Mexico" } ,
                    new Domain.Base.Entities.Place(countriesGuid) { PlacesCountry = "US", PlacesDesc = "United States" },
                    new Domain.Base.Entities.Place(Guid.NewGuid().ToString()) { PlacesCountry = "CD",  PlacesDesc = "Canada" }
                };
                _countryIsoDtoCollection = new List<Dtos.CountryIsoCodes>();
                foreach (var source in _placeEntityCollection)
                {
                    var countries = new Ellucian.Colleague.Dtos.CountryIsoCodes
                    {
                        Id = source.Guid,
                        Title = source.PlacesDesc,
                        ISOCode = source.PlacesCountry,
                        Status = (!string.IsNullOrEmpty(source.PlacesInactive) && source.PlacesInactive.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        ? Status.Inactive : Status.Active
                    };
                    _countryIsoDtoCollection.Add(countries);
                }

                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_placeEntityCollection);

                _referenceRepositoryMock.Setup(repo => repo.GetPlaceByGuidAsync(countriesGuid))
                   .ReturnsAsync(_placeEntityCollection.FirstOrDefault(x => x.Guid == countriesGuid));

                _countriesService = new CountriesService(_referenceRepositoryMock.Object,
                     _adapterRegistryMock.Object,
                     _currentUserFactoryMock.Object,
                    _roleRepositoryMock.Object,
                    _countryRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _countriesService = null;
                _countryIsoDtoCollection = null;
                _countryRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesAsync()
            {
                var results = await _countriesService.GetCountryIsoCodesAsync(true);
                Assert.IsTrue(results is IEnumerable<CountryIsoCodes>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesAsync_Count()
            {
                var results = await _countriesService.GetCountryIsoCodesAsync(true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesAsync_Properties()
            {
                var result =
                    (await _countriesService.GetCountryIsoCodesAsync(true)).FirstOrDefault(x => x.ISOCode == countriesCode);
                Assert.IsNotNull(result.Id);

                Assert.IsNotNull(result.ISOCode);

            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesAsync_Expected()
            {
                var expectedResults = _countryIsoDtoCollection.FirstOrDefault(c => c.Id == countriesGuid);
                var actualResult =
                    (await _countriesService.GetCountryIsoCodesAsync(true)).FirstOrDefault(x => x.Id == countriesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CountriesService_GetCountryIsoCodesAsync_RepositoryException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new RepositoryException());

                var expectedResults = _countryIsoDtoCollection.FirstOrDefault(c => c.Id == countriesGuid);
                var actualResult =
                    (await _countriesService.GetCountryIsoCodesAsync(true)).FirstOrDefault(x => x.Id == countriesGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_GetCountryIsoCodesAsync_KeyNotFoundException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new KeyNotFoundException());

                var expectedResults = _countryIsoDtoCollection.FirstOrDefault(c => c.Id == countriesGuid);
                var actualResult =
                    (await _countriesService.GetCountryIsoCodesAsync(true)).FirstOrDefault(x => x.Id == countriesGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CountriesService_GetCountryIsoCodesAsync_InvalidOperationException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new InvalidOperationException());

                var expectedResults = _countryIsoDtoCollection.FirstOrDefault(c => c.Id == countriesGuid);
                var actualResult =
                    (await _countriesService.GetCountryIsoCodesAsync(true)).FirstOrDefault(x => x.Id == countriesGuid);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_GetCountryIsoCodesByGuidAsync_Empty()
            {
                await _countriesService.GetCountryIsoCodesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_GetCountryIsoCodesByGuidAsync_Null()
            {
                await _countriesService.GetCountryIsoCodesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CountriesService_GetCountryIsoCodesByGuidAsync_Invalid()
            {
                await _countriesService.GetCountryIsoCodesByGuidAsync("invalid");
            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesByGuidAsync_Expected()
            {
                var expectedResults =
                    _countryIsoDtoCollection.First(c => c.Id == countriesGuid);
                var actualResult =
                    await _countriesService.GetCountryIsoCodesByGuidAsync(countriesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
            }

            [TestMethod]
            public async Task CountriesService_GetCountryIsoCodesByGuidAsync_Properties()
            {
                var result =
                    await _countriesService.GetCountryIsoCodesByGuidAsync(countriesGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Status);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }

        }
    }
}