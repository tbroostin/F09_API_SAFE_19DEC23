// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using FrequencyType = Ellucian.Colleague.Dtos.FrequencyType;
using Room = Ellucian.Colleague.Domain.Base.Entities.Room;
using RoomType = Ellucian.Colleague.Dtos.RoomType;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RoomsControllerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private  ILogger _logger = new Mock<ILogger>().Object;

        private RoomsController _roomsController;
        private IEnumerable<Room> _allRoomEntities;
        private  List<Dtos.Base.Room> _allRoomDtos = new List<Dtos.Base.Room>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;
            var testAdapter = new AutoMapperAdapter<Room, Dtos.Base.Room>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);
            _adapterRegistryMock.Setup(x => x.GetAdapter<Room, Dtos.Base.Room>()).Returns(testAdapter);

            _allRoomEntities = new TestRoomRepository().Get().ToList(); ;
            Mapper.CreateMap<Room, Dtos.Base.Room>();
            foreach (var target in _allRoomEntities.Select(Mapper.Map<Room, Dtos.Base.Room>))
            {
                _allRoomDtos.Add(target);
            }
            
            _roomsController = new RoomsController(_adapterRegistry, _roomRepository, _facilitiesService, _logger)
            {
                Request = new HttpRequestMessage()
            };
             _roomsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
     
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomsController = null;
            _roomRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roomRepositoryMock = null;
            _allRoomEntities = null;
            _facilitiesServiceMock = null;
            _logger = null;
            _allRoomDtos = null;
          
        }

        [TestMethod]
        public async Task RoomsController_ReturnsAllRooms_Cache()
        {
            _roomsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(false)).ReturnsAsync(_allRoomEntities); 
            
            var rooms = (await _roomsController.GetRoomsAsync()).ToList();
            Assert.AreEqual(_allRoomDtos.Count, rooms.Count);
            for (var i = 0; i < rooms.Count; i++)
            {
                var expected = _allRoomDtos[i];
                var actual = rooms[i];
                Assert.AreEqual(expected.BuildingCode, actual.BuildingCode, "BuildingCode, Index=" + i);
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i);
            }
        }

        [TestMethod]
        public async Task RoomsController_ReturnsAllRooms_NoCache()
        {
            _roomsController.Request.Headers.CacheControl =
               new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true }; 
            
            _roomRepositoryMock.Setup(x => x.GetRoomsAsync(true)).ReturnsAsync(_allRoomEntities);

            var rooms = (await _roomsController.GetRoomsAsync()).ToList();
            Assert.AreEqual(_allRoomDtos.Count, rooms.Count);
            for (var i = 0; i < rooms.Count; i++)
            {
                var expected = _allRoomDtos[i];
                var actual = rooms[i];
                Assert.AreEqual(expected.BuildingCode, actual.BuildingCode, "BuildingCode, Index=" + i);
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i);
            }
        }

       

    }

    [TestClass]
    public class RoomsControllerHedmTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private ILogger _logger = new Mock<ILogger>().Object;

        private RoomsController _roomsController;
        private IEnumerable<Room> _allRoomEntities;
        private List<Dtos.Room> _allRoomDtos = new List<Dtos.Room>();
        private List<Room2> _allRoom2Dtos = new List<Room2>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _allRoomEntities = new TestRoomRepository().Get().ToList();
            Mapper.CreateMap<Room, Dtos.Room>();
            foreach (var room in _allRoomEntities)
            {
                var target = Mapper.Map<Room, Dtos.Room>(room);
                _allRoomDtos.Add(target);
            }

            Mapper.CreateMap<Room, Room2>();
            foreach (var room in _allRoomEntities)
            {
                var target = Mapper.Map<Room, Room2>(room);
                target.Id = room.Guid;
                _allRoom2Dtos.Add(target);
            }

            _roomsController = new RoomsController(_adapterRegistry, _roomRepository, _facilitiesService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _roomsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomsController = null;
            _roomRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roomRepositoryMock = null;
            _allRoomEntities = null;
            _facilitiesServiceMock = null;
            _logger = null;
            _allRoomDtos = null;
            _allRoom2Dtos = null;
        }
      
        [TestMethod]
        public async Task RoomsController_QueryRoomsMinimumByPostAsync()
        {
            var rooms = new List<Dtos.RoomsMinimumResponse>();
            var expected = _allRoom2Dtos.FirstOrDefault();
            rooms.Add(new Dtos.RoomsMinimumResponse() { Id = expected.Id } );

            var request = new RoomsAvailabilityRequest2();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };

            _facilitiesServiceMock.Setup(x => x.GetRoomsMinimumAsync(request)).ReturnsAsync(rooms);
            var response = (await _roomsController.QueryRoomsMinimumByPostAsync(request)).FirstOrDefault();
            Assert.AreEqual(expected.Id, response.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void RoomsController_PostRoom_Exception()
        {
            _roomsController.PostRoom(_allRoomDtos.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void RoomsController_PutRoom_Exception()
        {
            var room = _allRoomDtos.FirstOrDefault();
             _roomsController.PutRoom(room.Guid, room);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void RoomsController_DeleteRoom_Exception()
        {
             _roomsController.DeleteRoom(_allRoomDtos.FirstOrDefault().Guid);
        }

    }

    [TestClass]
    public class RoomsPagingControllerHedmTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private ILogger _logger = new Mock<ILogger>().Object;

        private RoomsController _roomsController;
        private IEnumerable<Room> _allRoomEntities;
        private List<Room3> _allRoom3Dtos = new List<Room3>();
        private Tuple<IEnumerable<Dtos.Room3>, int> roomsTuple;

        private Paging page = new Paging(200, 0);

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _allRoomEntities = new TestRoomRepository().Get().ToList();
            
            Mapper.CreateMap<Room, Room3>();
            foreach (var room in _allRoomEntities)
            {
                var target = Mapper.Map<Room, Room3>(room);
                target.Id = room.Guid;
                _allRoom3Dtos.Add(target);
            }

            roomsTuple = new Tuple<IEnumerable<Room3>, int>(_allRoom3Dtos, _allRoom3Dtos.Count);

            _roomsController = new RoomsController(_adapterRegistry, _roomRepository, _facilitiesService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _roomsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomsController = null;
            _roomRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roomRepositoryMock = null;
            _allRoomEntities = null;
            _facilitiesServiceMock = null;
            _logger = null;
            _allRoom3Dtos = null;
        }

        [TestMethod]
        public async Task RoomsController_GetRooms_ValidateFields_Nocache()
        {
            _roomsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), false))
                .ReturnsAsync(roomsTuple);

            var sourceContexts = await _roomsController.GetHedmRooms3Async(null);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        public async Task RoomsController_GetRooms_ValidateFields_cache()
        {
            _roomsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), true))
                .ReturnsAsync(roomsTuple);

            var sourceContexts = await _roomsController.GetHedmRooms3Async(page);
            Assert.IsNotNull(sourceContexts);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetRooms_Exception()
        {
            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), true)).ThrowsAsync(new Exception());
            await _roomsController.GetHedmRooms3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetRooms_ArgumentException()
        {
            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), true)).ThrowsAsync(new ArgumentException());
            //var tempCriteria = "{'ABC':'ABC'}";
            await _roomsController.GetHedmRooms3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetRooms_KeyNotFoundException()
        {
            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), false)).ThrowsAsync(new KeyNotFoundException());
            await _roomsController.GetHedmRooms3Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetRooms_PermissionsException()
        {
            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
            await _roomsController.GetHedmRooms3Async(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetRooms_ArgumentNullException()
        {
            _facilitiesServiceMock.Setup(x => x.GetRooms3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            await _roomsController.GetHedmRooms3Async(It.IsAny<Paging>());
        }
    }

    [TestClass]
    public class RoomsControllerBuildingWingTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private ILogger _logger = new Mock<ILogger>().Object;

        private RoomsController _roomsController;
        private IEnumerable<RoomWing> _allRoomWingEntities;
        private List<BuildingWing> _allBuildingWingDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _allBuildingWingDtos = new List<BuildingWing>();
            _allRoomWingEntities = new TestRoomWingsRepository().Get().ToList();

            foreach (var source in _allRoomWingEntities)
            {
                var buildingWing = new BuildingWing()
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,

                };
                _allBuildingWingDtos.Add(buildingWing);
            }

            _roomsController = new RoomsController(_adapterRegistry, _roomRepository, _facilitiesService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _roomsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomsController = null;
            _roomRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roomRepositoryMock = null;
            _allRoomWingEntities = null;
            _facilitiesServiceMock = null;
            _logger = null;
            _allBuildingWingDtos = null;
            _allRoomWingEntities = null;
        }

        [TestMethod]
        public async Task RoomsController_GetBuildingWingsAsync_ValidateFields_Cache()
        {
            _roomsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            _facilitiesServiceMock.Setup(x => x.GetBuildingWingsAsync(false)).ReturnsAsync(_allBuildingWingDtos);

            var buildingWings = (await _roomsController.GetBuildingWingsAsync()).ToList();
            Assert.AreEqual(_allBuildingWingDtos.Count, buildingWings.Count);
            for (var i = 0; i < buildingWings.Count; i++)
            {
                var expected = _allBuildingWingDtos[i];
                var actual = buildingWings[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RoomsController_GetBuildingWingsAsync_ValidateFields_BypassCache()
        {
            _roomsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _facilitiesServiceMock.Setup(x => x.GetBuildingWingsAsync(true)).ReturnsAsync(_allBuildingWingDtos);

            var buildingWings = (await _roomsController.GetBuildingWingsAsync()).ToList();
            Assert.AreEqual(_allBuildingWingDtos.Count, buildingWings.Count);
            for (var i = 0; i < buildingWings.Count; i++)
            {
                var expected = _allBuildingWingDtos[i];
                var actual = buildingWings[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RoomsController_GetBuildingWingsByGuid_ValidateFields()
        {
            var expected = _allBuildingWingDtos.FirstOrDefault();
            _facilitiesServiceMock.Setup(x => x.GetBuildingWingsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            Assert.IsNotNull(expected);
            var actual = await _roomsController.GetBuildingWingsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetBuildingWings_Exception()
        {
            _facilitiesServiceMock.Setup(x => x.GetBuildingWingsAsync(It.IsAny<bool>())).Throws<Exception>();
            await _roomsController.GetBuildingWingsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_GetBuildingWingsByGuidAsync_Exception()
        {
            _facilitiesServiceMock.Setup(x => x.GetBuildingWingsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _roomsController.GetBuildingWingsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_PostBuildingWings_Exception()
        {
            var expected = _allBuildingWingDtos.FirstOrDefault();
            await _roomsController.PostBuildingWingsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_PutBuildingWings_Exception()
        {
            var expected = _allBuildingWingDtos.FirstOrDefault();
            await _roomsController.PutBuildingWingsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RoomsController_DeleteBuildingWings_Exception()
        {
            var expected = _allBuildingWingDtos.FirstOrDefault();
            Assert.IsNotNull(expected);
            await _roomsController.DeleteBuildingWingsAsync(expected.Id);
        }
    }

    [TestClass]
    public class RoomsAvailability10ControllerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoomRepository> _roomRepositoryMock;
        private IRoomRepository _roomRepository;
        private Mock<IFacilitiesService> _facilitiesServiceMock;
        private IFacilitiesService _facilitiesService;
        private ILogger _logger = new Mock<ILogger>().Object;

        private RoomsController _roomsController;
        private IEnumerable<Room> _allRoomEntities;
        private List<Dtos.Room> _allRoomDtos = new List<Dtos.Room>();
        private List<Room3> _allRoom3Dtos = new List<Room3>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomRepository = _roomRepositoryMock.Object;

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _facilitiesServiceMock = new Mock<IFacilitiesService>();
            _facilitiesService = _facilitiesServiceMock.Object;

            _allRoomEntities = new TestRoomRepository().Get().ToList();
            Mapper.CreateMap<Room, Dtos.Room>();
            foreach (var room in _allRoomEntities)
            {
                var target = Mapper.Map<Room, Dtos.Room>(room);
                _allRoomDtos.Add(target);
            }

            Mapper.CreateMap<Room, Room3>();
            foreach (var room in _allRoomEntities)
            {
                var target = Mapper.Map<Room, Room3>(room);
                target.Id = room.Guid;
                _allRoom3Dtos.Add(target);
            }

            _roomsController = new RoomsController(_adapterRegistry, _roomRepository, _facilitiesService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _roomsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomsController = null;
            _roomRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roomRepositoryMock = null;
            _allRoomEntities = null;
            _facilitiesServiceMock = null;
            _logger = null;
            _allRoomDtos = null;
            _allRoom3Dtos = null;
        }

        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async()
        {
            _roomsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };


            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            var response = (await _roomsController.QueryAvailableRoomsByPost4Async(request)).FirstOrDefault();
            Assert.AreEqual(expected.Id, response.Id);
        }

       
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_RepeatRuleWeekly()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2 { StartOn = new DateTime(2015, 2, 16) };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleWeekly { Type = FrequencyType2.Weekly, Ends = repeatRuleEnds, Interval = 1 };

            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };
            
            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }

        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_RepeatRuleMonthly()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2 { StartOn = new DateTime(2015, 2, 21) };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleMonthly { Type = FrequencyType2.Monthly,
                Ends = repeatRuleEnds, Interval = 1, RepeatBy = new RepeatRuleRepeatBy() { DayOfMonth = 2 } };

            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };


            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }

        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_RepeastRuleYearly()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleYearly { Type = FrequencyType2.Yearly, Ends = repeatRuleEnds, Interval = 1 };

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };

            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_MaximumOccupancyMissing()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };
            var repeatRuleEnds = new RepeatRuleEnds { Date = new DateTime(2015, 2, 21) };
            var repeatRule = new RepeatRuleDaily { Type = FrequencyType2.Daily, Ends = repeatRuleEnds, Interval = 1 };

            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { RepeatRule = repeatRule, TimePeriod = timePeriod };


            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }

      
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_RepeatRuleMissing()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };


            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { TimePeriod = timePeriod };


            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ReturnsAsync(rooms);
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_KeyNotFoundException()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };


            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { TimePeriod = timePeriod };

            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_IntegrationApiException()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };


            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { TimePeriod = timePeriod };

            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public async Task RoomsController_QueryAvailableRoomsByPost4Async_ArgumentException()
        {
            var rooms = new List<Room3>();
            var expected = _allRoom3Dtos.FirstOrDefault();
            rooms.Add(expected);

            var request = new RoomsAvailabilityRequest3();
            var timePeriod = new RepeatTimePeriod2
            {
                StartOn = new DateTime(2015, 2, 16),
                EndOn = new DateTime(2015, 2, 21)
            };


            var roomType = new RoomType
            {
                RoomTypesGuid = new GuidObject2("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4"),
                Type = RoomTypeTypes.Classroom
            };
            var roomTypes = new List<RoomType> { roomType };
            request.RoomType = roomTypes;

            var occupancies = new List<Occupancy2>();
            var occupancy = new Occupancy2 { MaximumOccupancy = 25, RoomLayoutType = RoomLayoutType2.Default };
            occupancies.Add(occupancy);
            request.Occupancies = occupancies;

            request.Recurrence = new Recurrence3 { TimePeriod = timePeriod };

            _facilitiesServiceMock.Setup(x => x.CheckRoomAvailability4Async(request, It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
            await _roomsController.QueryAvailableRoomsByPost4Async(request);
        }      
    }
}