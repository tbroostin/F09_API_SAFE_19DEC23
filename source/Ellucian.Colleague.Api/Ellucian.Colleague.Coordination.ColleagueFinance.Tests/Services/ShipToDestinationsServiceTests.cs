//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ShipToDestinationsServiceTests : CurrentUserSetup
    {
        private const string shipToDestinationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string shipToDestinationsCode = "AT";
        private const string shipToCodesCode = "MC";
        private ICollection<ShipToDestination> _shipToDestinationsCollection;
        private IEnumerable<ShipToCode> _shipToCodesCollection;
        private ShipToDestinationsService _shipToDestinationsService;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ILogger> _loggerMock;
        private Mock<IColleagueFinanceReferenceDataRepository> _cfReferenceRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAddressRepository> _addressRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;

        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public async void Initialize()
        {
            _cfReferenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();
            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });


            _shipToDestinationsCollection = new List<ShipToDestination>()
                {
                    new ShipToDestination("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ShipToDestination("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ShipToDestination("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            _shipToCodesCollection = new List<ShipToCode>()
                {
                    new ShipToCode("CD","Datatel - Central Dist. Office"),
                    new ShipToCode("DT","Datatel - Downtown"),
                    new ShipToCode("EC","Datatel - Extension Center"),
                    new ShipToCode("MC","Datatel - Main Campus")
                };


            _cfReferenceRepositoryMock.Setup(repo => repo.GetShipToDestinationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_shipToDestinationsCollection);

            _cfReferenceRepositoryMock.Setup(repo => repo.GetShipToCodesAsync())
                .ReturnsAsync(_shipToCodesCollection);

            // Mock the reference repository for country
            countries = new List<Domain.Base.Entities.Country>()
                 {
                    new Domain.Base.Entities.Country("US","United States","US"){ IsoAlpha3Code = "USA" },
                    new Domain.Base.Entities.Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN" },
                    new Domain.Base.Entities.Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX" },
                    new Domain.Base.Entities.Country("FR","France","FR"){ IsoAlpha3Code = "FRA" },
                    new Domain.Base.Entities.Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA" },
                    new Domain.Base.Entities.Country("AU","Australia","AU"){ IsoAlpha3Code = "AUS" },
                };
            _referenceRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

            // Mock the reference repository for states
            states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("VA","Virginia"),
                    new Domain.Base.Entities.State("MD","Maryland"),
                    new Domain.Base.Entities.State("NY","New York"),
                    new Domain.Base.Entities.State("MA","Massachusetts"),
                    new Domain.Base.Entities.State("DC","District of Columbia"),
                    new Domain.Base.Entities.State("TN","tennessee")
                };
            _referenceRepositoryMock.Setup(repo => repo.GetStateCodesAsync()).Returns(Task.FromResult(states));
            _referenceRepositoryMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

            _addressRepositoryMock.Setup(repo => repo.GetHostCountryAsync())
                .ReturnsAsync("USA");

            _shipToDestinationsService = new ShipToDestinationsService(_cfReferenceRepositoryMock.Object, _referenceRepositoryMock.Object, _addressRepositoryMock.Object, _configurationRepositoryMock.Object, adapterRegistry, currentUserFactory, roleRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _shipToDestinationsService = null;
            _shipToDestinationsCollection = null;
            _cfReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsAsync()
        {
            var results = await _shipToDestinationsService.GetShipToDestinationsAsync(true);
            Assert.IsTrue(results is IEnumerable<ShipToDestinations>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsAsync_Count()
        {
            var results = await _shipToDestinationsService.GetShipToDestinationsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsAsync_Properties()
        {
            var result =
                (await _shipToDestinationsService.GetShipToDestinationsAsync(true)).FirstOrDefault(x => x.Code == shipToDestinationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsAsync_Expected()
        {
            var expectedResults = _shipToDestinationsCollection.FirstOrDefault(c => c.Guid == shipToDestinationsGuid);
            var actualResult =
                (await _shipToDestinationsService.GetShipToDestinationsAsync(true)).FirstOrDefault(x => x.Id == shipToDestinationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ShipToDestinationsService_GetShipToDestinationsByGuidAsync_Empty()
        {
            await _shipToDestinationsService.GetShipToDestinationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ShipToDestinationsService_GetShipToDestinationsByGuidAsync_Null()
        {
            await _shipToDestinationsService.GetShipToDestinationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ShipToDestinationsService_GetShipToDestinationsByGuidAsync_InvalidId()
        {
            _cfReferenceRepositoryMock.Setup(repo => repo.GetShipToDestinationsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _shipToDestinationsService.GetShipToDestinationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsByGuidAsync_Expected()
        {
            var expectedResults =
                _shipToDestinationsCollection.First(c => c.Guid == shipToDestinationsGuid);
            var actualResult =
                await _shipToDestinationsService.GetShipToDestinationsByGuidAsync(shipToDestinationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToDestinationsByGuidAsync_Properties()
        {
            var result =
                await _shipToDestinationsService.GetShipToDestinationsByGuidAsync(shipToDestinationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToCodesAsync()
        {
            var results = await _shipToDestinationsService.GetShipToCodesAsync();
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToCodesAsync_Count()
        {
            var results = await _shipToDestinationsService.GetShipToCodesAsync();
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task ShipToDestinationsService_GetShipToCodesAsync_Properties()
        {
            var result =
                (await _shipToDestinationsService.GetShipToCodesAsync()).FirstOrDefault(x => x.Code == shipToCodesCode);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Description);

        }
    }
}