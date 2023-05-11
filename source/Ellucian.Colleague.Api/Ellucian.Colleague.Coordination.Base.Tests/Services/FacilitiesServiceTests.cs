// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.TestUtil;
using FrequencyType = Ellucian.Colleague.Dtos.FrequencyType;
using Room = Ellucian.Colleague.Domain.Base.Entities.Room;
using RoomCharacteristic = Ellucian.Colleague.Domain.Base.Entities.RoomCharacteristic;
using RoomType = Ellucian.Colleague.Dtos.RoomType;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class FacilitiesServiceTests
    {
        private FacilitiesService _facilitiesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        private IEventRepository _eventRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;

        private Mock<IPersonRepository> _personRepoMock;
        private IPersonRepository _personRepo;
        
        private ICurrentUserFactory _currentUserFactory;
       
        private IEnumerable<Domain.Base.Entities.Building> _allBuildingTypes;
        private IEnumerable<Domain.Base.Entities.Location> _allLocations;
        private IEnumerable<Domain.Base.Entities.Country> _countries;
        private IEnumerable<Domain.Base.Entities.State> _states;

        private const string personId = "S001";
        private const string FacilitiesGuid = "28a52594-1f3e-44e1-9f8c-26cb7aa6a7aa";

        [TestInitialize]
        public void Initialize()
        {
            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;

            _personRepoMock = new Mock<IPersonRepository>();
            _personRepo = _personRepoMock.Object;

            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            
            _logger = new Mock<ILogger>().Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

   
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _facilitiesService = new FacilitiesService(_refRepo, _configurationRepository, _roomRepository, _eventRepo,
                _personRepo , _adapterRegistry, _currentUserFactory, _roleRepo, _logger);

            var allRoomTypesEntities = new TestRoomTypesRepository().GetRoomTypes().ToList();
            _refRepoMock.Setup(repo => repo.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesEntities);
            _refRepoMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allRoomTypesEntities);

            _allBuildingTypes = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(_allBuildingTypes);

            _allLocations = new TestLocationRepository().Get();
            _refRepoMock.Setup(repo => repo.GetLocations(It.IsAny<bool>())).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);

            _countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("USA", "America", "US", "USA"),
                    new Domain.Base.Entities.Country("CAN", "Canada", "CA", "CAN"),
                    new Domain.Base.Entities.Country("AUS", "Austria", "AU", "AUS"),
                    new Domain.Base.Entities.Country("BRA", "Brazil", "BR", "BRA"),
                    new Domain.Base.Entities.Country("MEX", "Mexico", "MX", "MEX"),
                    new Domain.Base.Entities.Country("NLD", "Netherland", "ND", "NLD"),
                    new Domain.Base.Entities.Country("GBR", "London", "GB", "GBR"),
                    new Domain.Base.Entities.Country("ITL", "ITALY", "IL", "ILT"),
                    new Domain.Base.Entities.Country("JPN", "Japan", "JP", "JPN")
                };

            _states = new List<Domain.Base.Entities.State>()
                {
                    new Domain.Base.Entities.State("VA", "description", "GBR"),
                    new Domain.Base.Entities.State("AV", "description", "IND"),
                    new Domain.Base.Entities.State("SA", "sa", "USA"),
                };

            _refRepoMock.Setup(r => r.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(_countries);
            _refRepoMock.Setup(r => r.GetStateCodesAsync(It.IsAny<bool>())).ReturnsAsync(_states);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _facilitiesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _roomRepositoryMock = null;
            _roomRepository = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingsByGuid2Async_CompareBuildingsAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.Building thisBuilding = _allBuildingTypes.FirstOrDefault(m => m.Guid == FacilitiesGuid);
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(true)).ReturnsAsync(_allBuildingTypes.Where(m => m.Guid == FacilitiesGuid));
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var building = await _facilitiesService.GetBuilding2Async(FacilitiesGuid);
            Assert.AreEqual(thisBuilding.Guid, building.Id);
            Assert.AreEqual(thisBuilding.Code, building.Code);
            Assert.AreEqual(thisBuilding.Description, building.Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingsByGuid3Async_CompareBuildingsAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.Building thisBuilding = _allBuildingTypes.FirstOrDefault(m => m.Guid == FacilitiesGuid);
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(true)).ReturnsAsync(_allBuildingTypes.Where(m => m.Guid == FacilitiesGuid));
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var building = await _facilitiesService.GetBuilding3Async(FacilitiesGuid);
            Assert.AreEqual(thisBuilding.Guid, building.Id);
            Assert.AreEqual(thisBuilding.Code, building.Code);
            Assert.AreEqual(thisBuilding.Description, building.Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings2Async_CountBuildingsAsync()
        {
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(false)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings2Async(It.IsAny<bool>());
            Assert.AreEqual(5, buildings.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings3Async_CountBuildingsAsync()
        {
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(false)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings3Async(It.IsAny<bool>(), "");
            Assert.AreEqual(5, buildings.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings3Async_CountFilteredBuildingsAsync()
        {
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(false)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings3Async(It.IsAny<bool>(), "xyz");
            Assert.AreEqual(0, buildings.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings2Async_CompareBuildingsAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(false)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings2Async(It.IsAny<bool>());
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings3Async_CompareBuildingsAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(false)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings3Async(It.IsAny<bool>(), "");
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings2Async_CompareBuildingsAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(true)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings2Async(true);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildings3Async_CompareBuildingsAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(true)).ReturnsAsync(_allBuildingTypes);
            _refRepoMock.Setup(repo => repo.Locations).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations);
            var buildings = await _facilitiesService.GetBuildings3Async(true, "");
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(_allBuildingTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task FacilitiesService_GetBuildingByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(true)).ReturnsAsync(_allBuildingTypes);
            await _facilitiesService.GetBuilding2Async("siuowurhf");
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task FacilitiesService_GetBuildingByGuid3Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetBuildings2Async(true)).ReturnsAsync(_allBuildingTypes);
            await _facilitiesService.GetBuilding3Async("siuowurhf");
        }

        [TestMethod]
        public async Task FacilitiesService_GetSiteByGuid2Async_CompareSiteAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.Location thisLocation = _allLocations.FirstOrDefault(m => m.Guid == "b0eba383-5acf-4050-949d-8bb7a17c5012");
            _refRepoMock.Setup(repo => repo.GetLocations(true)).Returns(_allLocations.Where(m => m.Guid == "b0eba383-5acf-4050-949d-8bb7a17c5012"));
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_allLocations.Where(m => m.Guid == "b0eba383-5acf-4050-949d-8bb7a17c5012"));
            var site = await _facilitiesService.GetSite2Async("b0eba383-5acf-4050-949d-8bb7a17c5012");
            Assert.AreEqual(thisLocation.Guid, site.Id);
            Assert.AreEqual(thisLocation.Code, site.Code);
            Assert.AreEqual(thisLocation.Description, site.Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetSites2Async_CountSitesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetLocations(false)).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(false)).ReturnsAsync(_allLocations);
            var sites = await _facilitiesService.GetSites2Async(It.IsAny<bool>());
            Assert.AreEqual(5, sites.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetSites2Async_CompareSitesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetLocations(false)).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(false)).ReturnsAsync(_allLocations);
            var sites = await _facilitiesService.GetSites2Async(false);
            Assert.AreEqual(_allLocations.ElementAt(0).Guid, sites.ElementAt(0).Id);
            Assert.AreEqual(_allLocations.ElementAt(0).Code, sites.ElementAt(0).Code);
            Assert.AreEqual(_allLocations.ElementAt(0).Description, sites.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetSites2Async_CompareSitesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetLocations(true)).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(true)).ReturnsAsync(_allLocations);
            var sites = await _facilitiesService.GetSites2Async(true);
            Assert.AreEqual(_allLocations.ElementAt(0).Guid, sites.ElementAt(0).Id);
            Assert.AreEqual(_allLocations.ElementAt(0).Code, sites.ElementAt(0).Code);
            Assert.AreEqual(_allLocations.ElementAt(0).Description, sites.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetSites2Async_with_BuildingCode()
        {
            var newLocations = new List<Ellucian.Colleague.Domain.Base.Entities.Location>();
            List<string> buildings = new List<string>();
            buildings.Add("AND");
            buildings.Add("EIN");
            buildings.Add("SPORT");
            var location1 = new Ellucian.Colleague.Domain.Base.Entities.Location(Guid.NewGuid().ToString(), "MAIN", "Main Campus", Decimal.Parse("77.123456"), Decimal.Parse("-114.987654"), Decimal.Parse("77.123450"), Decimal.Parse("-114.987652"), "Y", buildings);
            newLocations.Add(location1);
            _refRepoMock.Setup(repo => repo.GetLocations(true)).Returns(newLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(true)).ReturnsAsync(newLocations);
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(_allBuildingTypes);
            var sites = await _facilitiesService.GetSites2Async(true);
            Assert.AreEqual(newLocations.ElementAt(0).Guid, sites.ElementAt(0).Id);
            Assert.AreEqual(newLocations.ElementAt(0).Code, sites.ElementAt(0).Code);
            Assert.AreEqual(newLocations.ElementAt(0).Description, sites.ElementAt(0).Title);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task FacilitiesService_GetSiteByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetLocations(true)).Returns(_allLocations);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(true)).ReturnsAsync(_allLocations);
            await _facilitiesService.GetSite2Async("siuowurhf");
        }

        // Fake an ICurrentUserFactory
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = personId,
                        /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string> { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }

    [TestClass]
    public class FacilitiesServiceRoomTests
    {
        // The service to be tested
        private FacilitiesService _facilitiesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        private Mock<IPersonRepository> _personRepoMock;
        private IPersonRepository _personRepo;

        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string PersonId = "S001";
        private const string RoomGuid = "2ae6e009-40ca-4ac0-bb41-c123f7c344e3";
        private const string SiteId = "b0eba383-5acf-4050-949d-8bb7a17c5012"; //MAIN
        private const string BuildingId = "4950a5ef-d542-48e9-bf07-a06b4de2f663"; //EIN

        private CampusCalendar _campusCalendar;
        private List<Room> _allRoomEntities;

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;

            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _personRepoMock = new Mock<IPersonRepository>();
            _personRepo = _personRepoMock.Object;

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _loggerMock = new Mock<ILogger>();
            _logger = _loggerMock.Object;
           
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _facilitiesService = new FacilitiesService(_refRepo, _configurationRepository, _roomRepository, _eventRepo, _personRepo,
                _adapterRegistry, _currentUserFactory, _roleRepo, _logger);

            _campusCalendar = BuildCalendars().FirstOrDefault();

            _allRoomEntities = new List<Room>
            {
                new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*0101", "COE")
                {
                    Capacity = 50,
                    RoomType = "110",
                    Name = "Room 1"
                },
                new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                {
                    Capacity = 100,
                    RoomType = "111",
                    Name = "Room 2"
                },
                new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                {
                    Capacity = 20,
                    RoomType = "111",
                    Name = "Room 13"
                },
                new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                {
                    Capacity = 50,
                    RoomType = "111",
                    Name = "Room 112"
                },
                new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                {
                    Capacity = 30,
                    RoomType = "111",
                    Name = "Room BSF",
                    
                    
                }
            };

            var allRoomTypesEntities = new TestRoomTypesRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesEntities);
            _refRepoMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allRoomTypesEntities);
            _loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _facilitiesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _roomRepositoryMock = null;
            _roomRepository = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task FacilitiesService_GetRoomsAsync_CountRoomsAsync()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities); 
            
            var rooms = await _facilitiesService.GetRoomsAsync(false);
            Assert.AreEqual(5, rooms.Count());
        }
        [TestMethod]
        public async Task FacilitiesService_GetRoomsAsync_CompareRoomsAsync_Cache()
        {
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(false)).ReturnsAsync(_allRoomEntities);

            var rooms = await _facilitiesService.GetRoomsAsync(false);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Guid, rooms.ElementAt(0).Guid);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Name, rooms.ElementAt(0).Title);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Number, rooms.ElementAt(0).Number);
        }

        [TestMethod]
        public async Task FacilitiesService_GetRoomsAsync_CompareRoomsAsync_NoCache()
        {
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(true)).ReturnsAsync(_allRoomEntities);

            var rooms = await _facilitiesService.GetRoomsAsync(true);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Guid, rooms.ElementAt(0).Guid);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Name, rooms.ElementAt(0).Title);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Number, rooms.ElementAt(0).Number);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Exception()
        {
            var request = new RoomsAvailabilityRequest2();
            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FacilitiesService_GetRoomsMinimum_Exception()
        {
            var request = new RoomsAvailabilityRequest2();
            await _facilitiesService.GetRoomsMinimumAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_NoArgument()
        {
            await _facilitiesService.CheckRoomAvailability3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_GetRoomsMinimum_NoArgument()
        {
            await _facilitiesService.GetRoomsMinimumAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidRoomCapacity()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 250, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod}
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

           await _facilitiesService.CheckRoomAvailability3Async(request);    
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability3_ValidRooms()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities); 
                     
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod}
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = await _facilitiesService.CheckRoomAvailability3Async(request);

            Assert.AreEqual(4, rooms.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetRoomsMinimum_ValidRooms()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = await _facilitiesService.GetRoomsMinimumAsync(request);

            Assert.AreEqual(4, rooms.Count());
        }

        [TestMethod]
        [ExpectedException(typeof (ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidRooms()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(() => null);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            var repeatRule = new RepeatRuleDaily {Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod}
            };

            var defaultsConfig = new DefaultsConfiguration {CampusCalendarId = "2016"};
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability3_ValidBuilding()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(allBuildings);
         
             var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod},
                Building = new Building2() {Id = BuildingId}
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = (await _facilitiesService.CheckRoomAvailability3Async(request)).ToList();
            Assert.AreEqual(1, rooms.Count());
            Assert.AreEqual(BuildingId, rooms.FirstOrDefault().BuildingGuid.Id);
          
        }

        [TestMethod]    
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidBuilding()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            var repeatRule = new RepeatRuleDaily {Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod},
                Building = new Building2() {Id = "x"}
            };

            var defaultsConfig = new DefaultsConfiguration {CampusCalendarId = "2016"};
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidRoomTypes()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 21)};
            var repeatRule = new RepeatRuleDaily {Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("x")
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod}
            };

            var defaultsConfig = new DefaultsConfiguration {CampusCalendarId = "2016"};
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidSite()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds {Date = new DateTime(2015, 2, 16)};
            var repeatRule = new RepeatRuleDaily {Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1};
            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod},
                Site = new Site2() {Id = "x"}
            };

            var defaultsConfig = new DefaultsConfiguration {CampusCalendarId = "2016"};
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability3_Site()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);

            var allSites = new TestLocationRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetLocations(It.IsAny<bool>())).Returns(allSites);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allSites);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 16) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 {RepeatRule = repeatRule, TimePeriod = timePeriod},
                Site = new Site2() {Id = SiteId}
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = await _facilitiesService.CheckRoomAvailability3Async(request);

            Assert.AreEqual(rooms.ElementAt(0).SiteGuid.Id, SiteId);
        }

        [TestMethod]
        public async Task FacilitiesService_GetRoomsMinimum_Site()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);

            var allSites = new TestLocationRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allSites);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 16) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod },
                Site = new Site2() { Id = SiteId }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = await _facilitiesService.GetRoomsMinimumAsync(request);

            Assert.AreEqual(rooms.Count(), 1);
        }    

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_RepeatRuleNull()
        {
                 var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            
            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = null, TimePeriod = timePeriod },
                Building = new Building2() { Id = "x" }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Invalid()
        {
             var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
             var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };
           
            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidOccupancy()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = null,
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_InvalidOccupancy_Zero()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 0, RoomLayoutType = RoomLayoutType2.Boardmeeting}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Daily_Interval_Zero()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 0 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Exam}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Daily_Interval_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 366 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Performance}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Daily_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0};
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Daily_Repetitions_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 366 };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Daily_RepeatRuleEnds_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = null };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Weekly_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0};
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 1, DayOfWeek = new List<HedmDayOfWeek?>(){HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday}};

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Weekly_Repetitions_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 366 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 1, DayOfWeek = new List<HedmDayOfWeek?>(){HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday}};

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Weekly_Interval_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 1 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 0, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Weekly_Interval_55()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 1 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 55, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Weekly_RepeatRuleEnds_null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds();
            var repeatRule = new RepeatRuleWeekly
            {
                Type = FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1, 
                DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday },
             };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_RepeatBy_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = null};

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_RepeatBy_Occurence_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy {DayOfWeek = new RepeatRuleDayOfWeek {Occurrence = 0}};
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_RepeatBy_Occurence_5()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 5 } };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_RepeatBy_Occurence_null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Day = HedmDayOfWeek.Sunday} };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_Interval_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 1, Day = HedmDayOfWeek.Monday} }; 
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 0, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_Interval_13()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 1, Day = HedmDayOfWeek.Monday } };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 13, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_DayOfMonth_35()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 35 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0};
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 30 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability3_Monthly_Repetitions_13()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 13 };
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 30 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest2
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability3Async(request);
        }


        private static IEnumerable<CampusCalendar> BuildCalendars()
        {
            ICollection<CampusCalendar> calendars = new List<CampusCalendar>();
            var campusCalendarsData = GetCampusCalendarsData();
            var campusCalendarsCount = campusCalendarsData.Length / 5;
            for (var i = 0; i < campusCalendarsCount; i++)
            {
                // Parse out the data
                var id = campusCalendarsData[i, 0].Trim();
                var description = campusCalendarsData[i, 1].Trim();
                var startOfDay = DateTime.Parse(campusCalendarsData[i, 2].Trim());
                var endOfDay = DateTime.Parse(campusCalendarsData[i, 3].Trim());
                var bookPastNoDays = campusCalendarsData[i, 4].Trim();
                calendars.Add(new CampusCalendar(id, description, startOfDay.TimeOfDay, endOfDay.TimeOfDay)
                {
                    BookPastNumberOfDays = int.Parse(bookPastNoDays)
                });
            }
            return calendars;
        }

        /// <summary>
        ///     Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        private static string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable =
            {
                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                {"2015", "2015 Calendar Year", "1/1/2015 12:00:00 AM", "12/31/2015 11:59:00 PM", "9999"},
                {"2016", "2016 Campus Calendar", "1/1/2016 12:00:00 AM", "12/31/2016 11:59:00 PM", "9999"},
                {"2017", "2017 Campus Calendar", "1/1/2017 12:00:00 AM", "12/31/2017 11:59:00 PM", "9999"},
                {"MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM", "1/1/1900 11:59:00 PM", "9999"}
            };
            return campusCalendarsTable;
        }

        // Fake an ICurrentUserFactory
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = PersonId,
                        /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string> { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }

    [TestClass]
    public class FacilitiesServiceRoomV6Tests
    {
        // The service to be tested
        private FacilitiesService _facilitiesService;
        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private Mock<IPersonRepository> _personRepoMock;
        private IPersonRepository _personRepo;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        private const string RoomGuid = "2ae6e009-40ca-4ac0-bb41-c123f7c344e3";
        private List<Room> _allRoomEntities;
        private Tuple<IEnumerable<Room>, int> result;


        private List<RoomWing> _allRoomWings;
        private List<RoomCharacteristic> _allCharacteristics;

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;
            _personRepoMock = new Mock<IPersonRepository>();
            _personRepo = _personRepoMock.Object;
            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _logger = new Mock<ILogger>().Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _facilitiesService = new FacilitiesService(_refRepo, _configurationRepository, _roomRepository, _eventRepo, _personRepo,
                _adapterRegistry, _currentUserFactory, _roleRepo, _logger);

            _allRoomEntities = new List<Room>
            {
                new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*0101", "COE")
                {
                    Capacity = 50,
                    RoomType = "110",
                    Name = "Room 1", 
                    Characteristics = new List<string>(){"SM"},
                    Wing = "N"
                },
                new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                {
                    Capacity = 100,
                    RoomType = "111",
                    Name = "Room 2", 
                    Characteristics = new List<string>(){"SM"},
                    Wing = "N"
                },
                new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                {
                    Capacity = 20,
                    RoomType = "111",
                    Name = "Room 13", 
                    Characteristics = new List<string>(){"SM"},
                    Wing = "N"
                },
                new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                {
                    Capacity = 50,
                    RoomType = "111",
                    Name = "Room 112", 
                    Characteristics = new List<string>(){"SM"},
                    Wing = "N"
                },
                new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                {
                    Capacity = 30,
                    RoomType = "111",
                    Name = "Room BSF", 
                    Characteristics = new List<string>(){"SM"},
                    Wing = "N"
                    
                }
            };

            result = new Tuple<IEnumerable<Room>, int>(_allRoomEntities, _allRoomEntities.Count);

            var allRoomTypesEntities = new TestRoomTypesRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesEntities);
            _refRepoMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allRoomTypesEntities);

            _allRoomWings = new TestRoomWingsRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomWings);

            _allCharacteristics = new List<RoomCharacteristic>()
            {
                new RoomCharacteristic("B371B2D7-01AC-4CAC-89F7-C652021C4D64", "SM","Smoking")
            };
            _refRepoMock.Setup(repo => repo.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(_allCharacteristics);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _facilitiesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _roomRepositoryMock = null;
            _roomRepository = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task FacilitiesService_GetRoomById3Async_CompareRoomsAsync()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var thisRoom = _allRoomEntities.FirstOrDefault(m => m.Guid == RoomGuid);
            Assert.IsNotNull(thisRoom);
            
            var expectedWing = _allRoomWings.FirstOrDefault(r => r.Code.Equals(thisRoom.Wing));
            Assert.IsNotNull(expectedWing);

            var expectedCharacteristic = _allCharacteristics.FirstOrDefault(x => x.Code.Equals(thisRoom.Characteristics.FirstOrDefault()));
           Assert.IsNotNull(expectedCharacteristic);

            var room = await _facilitiesService.GetRoomById3Async(RoomGuid);
            Assert.AreEqual(room.Id, thisRoom.Guid);
            Assert.AreEqual(room.Number, thisRoom.Number);
            Assert.AreEqual(room.Title, thisRoom.Name);
            Assert.AreEqual(expectedWing.Guid, room.Wing.Id);
            Assert.AreEqual(expectedCharacteristic.Guid, room.RoomCharacteristics.FirstOrDefault().Id);

        }

        [TestMethod]
        public async Task FacilitiesService_GetRooms3Async_CountRoomsAsync()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(result);

            var rooms = await _facilitiesService.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), false);
            Assert.AreEqual(5, rooms.Item1.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetRooms3Async_CompareRoomsAsync_Cache()
        {
            _roomRepositoryMock.Setup(x => x.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(result);

            var rooms = await _facilitiesService.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), false);
            Assert.IsNotNull(rooms);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Guid, rooms.Item1.ElementAt(0).Id);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Name, rooms.Item1.ElementAt(0).Title);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Number, rooms.Item1.ElementAt(0).Number);
        }

        [TestMethod]
        public async Task FacilitiesService_GetRooms3Async_CompareRoomsAsync_NoCache()
        {
            _roomRepositoryMock.Setup(x => x.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(result);

            var rooms = await _facilitiesService.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), true);
            Assert.IsNotNull(rooms);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Guid, rooms.Item1.ElementAt(0).Id);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Name, rooms.Item1.ElementAt(0).Title);
            Assert.AreEqual(_allRoomEntities.ElementAt(0).Number, rooms.Item1.ElementAt(0).Number);
        }

        [ExpectedException(typeof(NullReferenceException))]
        [TestMethod]
        public async Task FacilitiesService_GetRoomById3Async_InvalidAsync()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);
            await _facilitiesService.GetRoomById3Async("");
        }

        [TestMethod]
        public async Task FacilitiesService_GetRooms3Async_NoCache_Guid()
        {
            var roomGuids = _allRoomEntities.Select(x => x.Guid).ToList();

            _roomRepositoryMock.Setup(x => x.GetRoomsWithPagingAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(result);
            var guids = (await _facilitiesService.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), false)).Item1.Select(x => x.Id).ToList();
            Assert.AreEqual(roomGuids.Count, guids.Count);
            CollectionAssert.AllItemsAreInstancesOfType(guids, typeof (string));
            CollectionAssert.AreEqual(roomGuids, guids);
        }
    }

    [TestClass]
    public class FacilitiesServiceBuildingWingsTests
    {
        #region private variables

        private FacilitiesService _facilitiesService;
        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private Mock<IPersonRepository> _personRepoMock;
        private IPersonRepository _personRepo;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        //  RoomWing: {"84b9c4eb-22fa-4467-94b0-1966015e9953", "N", "North"},      
        private const string RoomWingGuid = "84b9c4eb-22fa-4467-94b0-1966015e9953";

        private List<RoomWing> _allRoomWingEntities;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;
            _personRepoMock = new Mock<IPersonRepository>();
            _personRepo = _personRepoMock.Object;
            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _logger = new Mock<ILogger>().Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();

            _allRoomWingEntities = new TestRoomWingsRepository().Get().ToList();

            _facilitiesService = new FacilitiesService(_refRepo, _configurationRepository, _roomRepository, _eventRepo, _personRepo,
                _adapterRegistry, _currentUserFactory, _roleRepo, _logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _facilitiesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _roomRepositoryMock = null;
            _roomRepository = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingWingsByGuid_ValidateFields()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomWingEntities);

            var expected = _allRoomWingEntities.FirstOrDefault(m => m.Guid.Equals(RoomWingGuid, StringComparison.OrdinalIgnoreCase));
            var actual = await _facilitiesService.GetBuildingWingsByGuidAsync(RoomWingGuid);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Description, actual.Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingWings_CountResult()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomWingEntities);

            var actual = await _facilitiesService.GetBuildingWingsAsync(true);
            Assert.AreEqual(_allRoomWingEntities.Count(), actual.Count());
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingWings_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(true)).ReturnsAsync(_allRoomWingEntities);

            var expected = _allRoomWingEntities.FirstOrDefault(m => m.Guid.Equals(RoomWingGuid, StringComparison.OrdinalIgnoreCase));
            var actual = (await _facilitiesService.GetBuildingWingsAsync(true)).FirstOrDefault(x => x.Id.Equals(RoomWingGuid, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Description, actual.Title);
        }

        [TestMethod]
        public async Task FacilitiesService_GetBuildingWings_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(false)).ReturnsAsync(_allRoomWingEntities);

            var expected = _allRoomWingEntities.FirstOrDefault(m => m.Guid.Equals(RoomWingGuid, StringComparison.OrdinalIgnoreCase));
            var actual = (await _facilitiesService.GetBuildingWingsAsync(false)).FirstOrDefault(x => x.Id.Equals(RoomWingGuid, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Description, actual.Title);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task FacilitiesService_GetBuildingWingsByGuid_InvalidGuid()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomWingEntities);
            await _facilitiesService.GetBuildingWingsByGuidAsync("9999999");
        }

        [TestMethod]
        [ExpectedException(typeof (NullReferenceException))]
        public async Task FacilitiesService_GetBuildingWingsByGuid_EmptyGuid()
        {
            _refRepoMock.Setup(repo => repo.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomWingEntities);
            await _facilitiesService.GetBuildingWingsByGuidAsync("");
        }
    }

    [TestClass]
    public class FacilitiesServiceQueryAvailableRoomsByPost4Tests
    {
        // The service to be tested
        private FacilitiesService _facilitiesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<ILogger> _loggerMock;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        private Mock<IPersonRepository> _personRepoMock;
        private IPersonRepository _personRepo;

        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string PersonId = "S001";
        private const string RoomGuid = "2ae6e009-40ca-4ac0-bb41-c123f7c344e3";
        private const string SiteId = "b0eba383-5acf-4050-949d-8bb7a17c5012"; //MAIN
        private const string BuildingId = "4950a5ef-d542-48e9-bf07-a06b4de2f663"; //EIN

        private CampusCalendar _campusCalendar;
        private List<Room> _allRoomEntities;

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;

            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _personRepoMock = new Mock<IPersonRepository>();
            _personRepo = _personRepoMock.Object;

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _loggerMock = new Mock<ILogger>();
            _logger = _loggerMock.Object;
            
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _facilitiesService = new FacilitiesService(_refRepo, _configurationRepository, _roomRepository, _eventRepo, _personRepo,
                _adapterRegistry, _currentUserFactory, _roleRepo, _logger);

            _campusCalendar = BuildCalendars().FirstOrDefault();

            _allRoomEntities = new List<Room>
            {
                new Room("2ae6e009-40ca-4ac0-bb41-c123f7c344e3", "COE*0101", "COE")
                {
                    Capacity = 50,
                    RoomType = "110",
                    Name = "Room 1"
                },
                new Room("8c92e963-5f05-45a2-8484-d9ad21e6ab47", "COE*0110", "CEE")
                {
                    Capacity = 100,
                    RoomType = "111",
                    Name = "Room 2"
                },
                new Room("8fdbaec7-4198-4348-b95a-a48a357e67f5", "COE*0120", "CDF")
                {
                    Capacity = 20,
                    RoomType = "111",
                    Name = "Room 13"
                },
                new Room("327a6856-0230-4a6d-82ed-5c99dc1b1862", "COE*0121", "CSD")
                {
                    Capacity = 50,
                    RoomType = "111",
                    Name = "Room 112"
                },
                new Room("cc9aa34c-db5e-46dc-9e5b-ba3f4b2557a8", "EIN*0121", "BSF")
                {
                    Capacity = 30,
                    RoomType = "111",
                    Name = "Room BSF",


                }
            };

            var allRoomTypesEntities = new TestRoomTypesRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetRoomTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRoomTypesEntities);
            _refRepoMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allRoomTypesEntities);
            _loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _facilitiesService = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _eventRepo = null;
            _eventRepoMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _roomRepositoryMock = null;
            _roomRepository = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Exception()
        {
            var request = new RoomsAvailabilityRequest3();
            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_NoArgument()
        {
            await _facilitiesService.CheckRoomAvailability4Async(null);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability4_RoomCapacity()
        {

            _loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 250, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability4_ValidRooms()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = await _facilitiesService.CheckRoomAvailability4Async(request);

            Assert.AreEqual(4, rooms.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_InvalidRooms()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(() => null);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability4_ValidBuilding()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(allBuildings);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod },
                Building = new Building2() { Id = BuildingId }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            var rooms = (await _facilitiesService.CheckRoomAvailability4Async(request)).ToList();
            Assert.AreEqual(1, rooms.Count());
            Assert.AreEqual(BuildingId, rooms.FirstOrDefault().BuildingGuid.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_InvalidBuilding()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod },
                Building = new Building2() { Id = "x" }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_InvalidRoomTypes()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("x")
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_InvalidSite()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 16) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod },
                Site = new Site2() { Id = "x" }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts2(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>())).Returns(new List<string>());

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        public async Task FacilitiesService_CheckRoomAvailability4_Site()
        {
            _roomRepositoryMock.Setup(x => x.RoomsAsync()).ReturnsAsync(_allRoomEntities);
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(_allRoomEntities);

            var allBuildings = new TestBuildingRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.BuildingsAsync()).ReturnsAsync(allBuildings);
            _refRepoMock.Setup(repo => repo.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(allBuildings);

            var allSites = new TestLocationRepository().Get().ToList();
            _refRepoMock.Setup(repo => repo.GetLocations(It.IsAny<bool>())).Returns(allSites);
            _refRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allSites);

            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 16)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 16) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };
            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod },
                Site = new Site2() { Id = SiteId }
            };

            var defaultsConfig = new DefaultsConfiguration { CampusCalendarId = "2016" };
            _configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultsConfig);

            _eventRepoMock.Setup(e => e.GetCalendar("2016")).Returns(_campusCalendar);

            _eventRepoMock.Setup(
                e =>
                    e.GetRoomIdsWithConflicts3Async(timePeriod.StartOn.Value, timePeriod.EndOn.Value, It.IsAny<IEnumerable<DateTime>>(),
                        It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            var rooms = await _facilitiesService.CheckRoomAvailability4Async(request);

            Assert.AreEqual(rooms.ElementAt(0).SiteGuid.Id, SiteId);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_RepeatRuleNull()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = null, TimePeriod = timePeriod },
                Building = new Building2() { Id = "x" }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Invalid()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

      

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_InvalidOccupancy_Zero()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 0, RoomLayoutType = RoomLayoutType2.Boardmeeting}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Daily_Interval_Zero()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 0 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Exam}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Daily_Interval_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 366 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Performance}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Daily_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0 };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Daily_Repetitions_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 366 };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

      


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Weekly_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 1, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Weekly_Repetitions_366()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 366 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 1, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Weekly_Interval_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 1 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 0, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Weekly_Interval_55()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 1 };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 55, DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday } };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Weekly_RepeatRuleEnds_null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds();
            var repeatRule = new RepeatRuleWeekly
            {
                Type = FrequencyType2.Weekly,
                Ends = repeatRuleEnds,
                Interval = 1,
                DayOfWeek = new List<HedmDayOfWeek?>() { HedmDayOfWeek.Monday, HedmDayOfWeek.Wednesday },
            };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_RepeatBy_Null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = null };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_RepeatBy_Occurence_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 0 } };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_RepeatBy_Occurence_5()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 5 } };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_RepeatBy_Occurence_null()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Day = HedmDayOfWeek.Sunday } };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_Interval_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 1, Day = HedmDayOfWeek.Monday } };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 0, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_Interval_13()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfWeek = new RepeatRuleDayOfWeek { Occurrence = 1, Day = HedmDayOfWeek.Monday } };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 13, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_DayOfMonth_35()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = DateTime.Today.AddDays(14) };
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 35 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_Repetitions_0()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 0 };
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 30 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task FacilitiesService_CheckRoomAvailability4_Monthly_Repetitions_13()
        {
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = DateTime.Today,
                EndOn = DateTime.Today.AddDays(14)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Repetitions = 13 };
            var repeatBy = new RepeatRuleRepeatBy { DayOfMonth = 30 };
            var repeatRule = new RepeatRuleMonthly() { Type = FrequencyType2.Monthly, Ends = repeatRuleEnds, Interval = 1, RepeatBy = repeatBy };

            var request = new RoomsAvailabilityRequest3
            {
                RoomType = new List<RoomType>
                {
                    new RoomType
                    {
                        RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                        Type = RoomTypeTypes.Classroom
                    }
                },
                Occupancies = new List<Occupancy2>()
                {
                    new Occupancy2 {MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default}
                },
                Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod }
            };

            await _facilitiesService.CheckRoomAvailability4Async(request);
        }


        private static IEnumerable<CampusCalendar> BuildCalendars()
        {
            ICollection<CampusCalendar> calendars = new List<CampusCalendar>();
            var campusCalendarsData = GetCampusCalendarsData();
            var campusCalendarsCount = campusCalendarsData.Length / 5;
            for (var i = 0; i < campusCalendarsCount; i++)
            {
                // Parse out the data
                var id = campusCalendarsData[i, 0].Trim();
                var description = campusCalendarsData[i, 1].Trim();
                var startOfDay = DateTime.Parse(campusCalendarsData[i, 2].Trim());
                var endOfDay = DateTime.Parse(campusCalendarsData[i, 3].Trim());
                var bookPastNoDays = campusCalendarsData[i, 4].Trim();
                calendars.Add(new CampusCalendar(id, description, startOfDay.TimeOfDay, endOfDay.TimeOfDay)
                {
                    BookPastNumberOfDays = int.Parse(bookPastNoDays)
                });
            }
            return calendars;
        }

        /// <summary>
        ///     Gets campus calendar raw data
        /// </summary>
        /// <returns>String array of raw campus calendar data</returns>
        private static string[,] GetCampusCalendarsData()
        {
            string[,] campusCalendarsTable =
            {
                // ID      Description               Start of Day Time        End of Day Time         //Book Past No Days
                {"2015", "2015 Calendar Year", "1/1/2015 12:00:00 AM", "12/31/2015 11:59:00 PM", "9999"},
                {"2016", "2016 Campus Calendar", "1/1/2016 12:00:00 AM", "12/31/2016 11:59:00 PM", "9999"},
                {"2017", "2017 Campus Calendar", "1/1/2017 12:00:00 AM", "12/31/2017 11:59:00 PM", "9999"},
                {"MAIN", "Main Calendar for PID2", "1/1/1900 12:00:00 AM", "1/1/1900 11:59:00 PM", "9999"}
            };
            return campusCalendarsTable;
        }

        // Fake an ICurrentUserFactory
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = PersonId,
                        /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string> { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}