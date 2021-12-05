//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CurrenciesServiceTests
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
        public class CurrenciesServiceTests_GET
        {
            private const string currenciesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string currenciesCode = "USA";
            private List<Currencies> _currenciesDtoCollection;
            private List<Domain.Base.Entities.CurrencyConv> _currenciesEntityCollection;
            private CurrenciesService _currenciesService;

            private Mock<ICurrencyRepository> _currencyRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;

            [TestInitialize]
            public async void Initialize()
            {
                _currencyRepositoryMock = new Mock<ICurrencyRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();

                _currenciesEntityCollection = new List<Domain.Base.Entities.CurrencyConv>()
                {
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "EUR", "Euro",  "EUR"),
                    new Domain.Base.Entities.CurrencyConv(currenciesGuid, "USA", "US Dollars", "USD"),
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "CAD", "Canadian", "CAD")
                };
                _currenciesDtoCollection = new List<Dtos.Currencies>();
                foreach (var source in _currenciesEntityCollection)
                {
                    var currencies = new Ellucian.Colleague.Dtos.Currencies
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        ISOCode = source.IsoCode
                    };
                    _currenciesDtoCollection.Add(currencies);
                }

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_currenciesEntityCollection);

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(_currenciesEntityCollection.FirstOrDefault(c => c.Guid.Equals(currenciesGuid)));

                _currenciesService = new CurrenciesService(_adapterRegistryMock.Object,
                     _currentUserFactoryMock.Object,
                    _roleRepositoryMock.Object,
                    _currencyRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _currenciesService = null;
                _currenciesDtoCollection = null;
                _currencyRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesAsync()
            {
                var results = await _currenciesService.GetCurrenciesAsync(true);
                Assert.IsTrue(results is IEnumerable<Currencies>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesAsync_Count()
            {
                var results = await _currenciesService.GetCurrenciesAsync(true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesAsync_Properties()
            {
                var result =
                    (await _currenciesService.GetCurrenciesAsync(true)).FirstOrDefault(x => x.Code == currenciesCode);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);

            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesAsync_Expected()
            {
                var expectedResults = _currenciesDtoCollection.FirstOrDefault(c => c.Id == currenciesGuid);
                var actualResult =
                    (await _currenciesService.GetCurrenciesAsync(true)).FirstOrDefault(x => x.Id == currenciesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
                Assert.AreEqual(expectedResults.Code, actualResult.Code);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CurrenciesService_GetCurrenciesAsync_RepositoryException_RepoError()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionAsync(It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                await _currenciesService.GetCurrenciesAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_GetCurrenciesAsync_GuidsNotGenerated()
            {
                _currenciesEntityCollection = new List<Domain.Base.Entities.CurrencyConv>()
                {
                    new Domain.Base.Entities.CurrencyConv("", "EUR", "Euro",  "EUR"),
                    new Domain.Base.Entities.CurrencyConv("", "USA", "US Dollars", "USD"),
                    new Domain.Base.Entities.CurrencyConv("", "CAD", "Canadian", "CAD")
                };

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_Empty()
            {
                await _currenciesService.GetCurrenciesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_Null()
            {
                await _currenciesService.GetCurrenciesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_InvalidId()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _currenciesService.GetCurrenciesByGuidAsync("99");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_RepositoryException_RepoError()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(It.IsAny<string>()))
                    .Throws<RepositoryException>();

                await _currenciesService.GetCurrenciesByGuidAsync("99");
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_Expected()
            {
                var expectedResults =
                    _currenciesDtoCollection.First(c => c.Id == currenciesGuid);
                var actualResult =
                    await _currenciesService.GetCurrenciesByGuidAsync(currenciesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
                Assert.AreEqual(expectedResults.Code, actualResult.Code);

            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrenciesByGuidAsync_Properties()
            {
                var result =
                    await _currenciesService.GetCurrenciesByGuidAsync(currenciesGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }
        }

        [TestClass]
        public class CurrenciesServiceTests_PUT : CurrentUserSetup
        {
            private const string currenciesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string currenciesCode = "USA";
            private List<Currencies> _currenciesDtoCollection;
            private List<Domain.Base.Entities.CurrencyConv> _currenciesEntityCollection;
            private CurrenciesService _currenciesService;

            private Mock<ICurrencyRepository> _currencyRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IRoleRepository _roleRepository;
            private Mock<IConfigurationRepository> _configurationRepoMock;

            [TestInitialize]
            public async void Initialize()
            {
                _currencyRepositoryMock = new Mock<ICurrencyRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock permissions               
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdateCurrency));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });


                _currenciesEntityCollection = new List<Domain.Base.Entities.CurrencyConv>()
                {
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "EUR", "Euro",  "EUR"),
                    new Domain.Base.Entities.CurrencyConv(currenciesGuid, "USA", "US Dollars", "USD"),
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "CAD", "Canadian", "CAD")
                };
                _currenciesDtoCollection = new List<Dtos.Currencies>();
                foreach (var source in _currenciesEntityCollection)
                {
                    var currencies = new Ellucian.Colleague.Dtos.Currencies
                    {
                        Id = source.Guid,
                        Code = source.Code,
                        Title = source.Description,
                        ISOCode = source.IsoCode
                    };
                    _currenciesDtoCollection.Add(currencies);
                }

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_currenciesEntityCollection);

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(_currenciesEntityCollection.FirstOrDefault(c => c.Guid.Equals(currenciesGuid)));

                _currenciesService = new CurrenciesService(_adapterRegistryMock.Object,
                     _currentUserFactory,
                    _roleRepositoryMock.Object,
                    _currencyRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _currenciesService = null;
                _currenciesDtoCollection = null;
                _currencyRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CurrenciesService_PutCurrenciesAsync_Properties()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(currencyEntity);

                var result =
                    await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);

                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Code);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }

            [TestMethod]
            public async Task CurrenciesService_PutCurrenciesAsync_Response()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(currencyEntity);

                var result =
                    await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);

                Assert.AreEqual(currencyDto.Id, result.Id, "id");
                Assert.AreEqual(currencyDto.Code, result.Code, "code");
                Assert.AreEqual(currencyDto.ISOCode, result.ISOCode, "isoCode");
                Assert.AreEqual(currencyDto.Title, result.Title, "title");

            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_ChangedCode()
            {
                var existing = _currenciesEntityCollection.FirstOrDefault(c => c.Guid.Equals(currenciesGuid));

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                currencyDto.Code = "changed";

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_ChangedTitle()
            {
                var existing = _currenciesEntityCollection.FirstOrDefault(c => c.Guid.Equals(currenciesGuid));

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                currencyDto.Title = "changed";

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_InvalidISOCode()
            {
                var existing = _currenciesEntityCollection.FirstOrDefault(c => c.Guid.Equals(currenciesGuid));

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(existing);

                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                currencyDto.ISOCode = "1234";

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_GuidNotFound()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                    .ReturnsAsync(() => null);

                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_PutCurrenciesAsync_ArgumentNullException_EmptyGuid()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                await _currenciesService.PutCurrenciesAsync("", currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_PutCurrenciesAsync_ArgumentNullException_EmptyBody()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                await _currenciesService.PutCurrenciesAsync(currencyDto.Id, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_RepoError()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>()))
                    .ThrowsAsync(new RepositoryException());

                var result =
                    await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrenciesService_PutCurrenciesAsync_PermissionsException()
            {
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                facultyRole.RemovePermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdateCurrency));
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(currencyEntity);

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_EmptyCode()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(currencyEntity);

                currencyDto.Code = "";

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_IntegrationApiException_EmptyTitle()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(currencyEntity);

                currencyDto.Title = "";

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_PutCurrenciesAsync_ExistingRecordNotFound()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                _currencyRepositoryMock.Setup(repo => repo.GetCurrencyConversionByGuidAsync(currenciesGuid))
                   .ThrowsAsync(new KeyNotFoundException());


                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>()))
                    .ReturnsAsync(currencyEntity);

                var result =
                    await _currenciesService.PutCurrenciesAsync("invalid", currencyDto);

            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CurrenciesService_PutCurrenciesAsync_UpdateCurrencyConversionAsync_null()
            {
                var currencyDto = _currenciesDtoCollection.FirstOrDefault(x => x.Id == currenciesGuid);

                var currencyEntity = _currenciesEntityCollection.FirstOrDefault(x => x.Guid == currenciesGuid);
                _currencyRepositoryMock.Setup(repo => repo.UpdateCurrencyConversionAsync(It.IsAny<Domain.Base.Entities.CurrencyConv>())).ReturnsAsync(() => null);

                await _currenciesService.PutCurrenciesAsync(currenciesGuid, currencyDto);
            }
        }

        [TestClass]
        public class CurrenciesServiceTests_CurrencyIsoCodes
        {
            private const string currenciesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string currenciesCode = "USA";
            private List<CurrencyIsoCodes> _currencyIsoDtoCollection;
            private List<Domain.Base.Entities.IntgIsoCurrencyCodes> _intgIsoEntityCollection;
            private CurrenciesService _currenciesService;

            private Mock<ICurrencyRepository> _currencyRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;

            [TestInitialize]
            public async void Initialize()
            {
                _currencyRepositoryMock = new Mock<ICurrencyRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();

                _intgIsoEntityCollection = new List<Domain.Base.Entities.IntgIsoCurrencyCodes>()
                {
                    new Domain.Base.Entities.IntgIsoCurrencyCodes(Guid.NewGuid().ToString(), "EUR", "Euro",  "A"),
                    new Domain.Base.Entities.IntgIsoCurrencyCodes(currenciesGuid, "USA", "US Dollars", "A"),
                    new Domain.Base.Entities.IntgIsoCurrencyCodes(Guid.NewGuid().ToString(), "CAD", "Canadian", "A")
                };
                _currencyIsoDtoCollection = new List<Dtos.CurrencyIsoCodes>();
                foreach (var source in _intgIsoEntityCollection)
                {
                    var currencies = new Ellucian.Colleague.Dtos.CurrencyIsoCodes
                    {
                        Id = source.Guid,

                        Title = source.Description,
                        Status = Dtos.EnumProperties.Status.Active
                    };
                    _currencyIsoDtoCollection.Add(currencies);
                }

                _currencyRepositoryMock.Setup(repo => repo.GetIntgIsoCurrencyCodesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_intgIsoEntityCollection);


                _currenciesService = new CurrenciesService(_adapterRegistryMock.Object,
                     _currentUserFactoryMock.Object,
                    _roleRepositoryMock.Object,
                    _currencyRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _currenciesService = null;
                _currencyIsoDtoCollection = null;
                _currencyRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync()
            {
                var results = await _currenciesService.GetCurrencyIsoCodesAsync(true);
                Assert.IsTrue(results is IEnumerable<CurrencyIsoCodes>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_Count()
            {
                var results = await _currenciesService.GetCurrencyIsoCodesAsync(true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_Properties()
            {
                var result =
                    (await _currenciesService.GetCurrencyIsoCodesAsync(true)).FirstOrDefault(x => x.ISOCode == currenciesCode);
                Assert.IsNotNull(result.Id);

                Assert.IsNotNull(result.ISOCode);

            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_Expected()
            {
                var expectedResults = _currencyIsoDtoCollection.FirstOrDefault(c => c.Id == currenciesGuid);
                var actualResult =
                    (await _currenciesService.GetCurrencyIsoCodesAsync(true)).FirstOrDefault(x => x.Id == currenciesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_RepositoryException()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetIntgIsoCurrencyCodesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new RepositoryException());

                var expectedResults = _currencyIsoDtoCollection.FirstOrDefault(c => c.Id == currenciesGuid);
                var actualResult =
                    (await _currenciesService.GetCurrencyIsoCodesAsync(true)).FirstOrDefault(x => x.Id == currenciesGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_KeyNotFoundException()
            {
                _currencyRepositoryMock.Setup(repo => repo.GetIntgIsoCurrencyCodesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new KeyNotFoundException());

                var expectedResults = _currencyIsoDtoCollection.FirstOrDefault(c => c.Id == currenciesGuid);
                var actualResult =
                    (await _currenciesService.GetCurrencyIsoCodesAsync(true)).FirstOrDefault(x => x.Id == currenciesGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesByGuidAsync_Empty()
            {
                await _currenciesService.GetCurrencyIsoCodesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesByGuidAsync_Null()
            {
                await _currenciesService.GetCurrencyIsoCodesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesByGuidAsync_Invalid()
            {
                await _currenciesService.GetCurrencyIsoCodesByGuidAsync("invalid");
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesByGuidAsync_Expected()
            {
                var expectedResults =
                    _currencyIsoDtoCollection.First(c => c.Id == currenciesGuid);
                var actualResult =
                    await _currenciesService.GetCurrencyIsoCodesByGuidAsync(currenciesGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
            }

            [TestMethod]
            public async Task CurrenciesService_GetCurrencyIsoCodesByGuidAsync_Properties()
            {
                var result =
                    await _currenciesService.GetCurrencyIsoCodesByGuidAsync(currenciesGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Status);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrenciesService_GetCurrencyIsoCodesAsync_GuidsNotGenerated()
            {
                _intgIsoEntityCollection = new List<Domain.Base.Entities.IntgIsoCurrencyCodes>()
                {
                    new Domain.Base.Entities.IntgIsoCurrencyCodes("", "EUR", "Euro",  "A"),
                    new Domain.Base.Entities.IntgIsoCurrencyCodes("", "USA", "US Dollars", "A"),
                    new Domain.Base.Entities.IntgIsoCurrencyCodes("", "CAD", "Canadian", "A")
                };

            }
        }
    }
}