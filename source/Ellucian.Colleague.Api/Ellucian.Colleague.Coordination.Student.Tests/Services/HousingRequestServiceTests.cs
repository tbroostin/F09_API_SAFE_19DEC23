using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class HousingRequestServiceTests_V10
    {
        [TestClass]
        public class HousingRequestServiceTests_GETALL_GETBYID : StudentUserFactory
        {
            #region DECLCARATIONS

            protected Domain.Entities.Role viewHousingRequest = new Domain.Entities.Role(1, "VIEW.HOUSING.REQS");
            private Mock<IHousingRequestRepository> housingRequestRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<ILogger> loggerMock;

            private HousingRequestUser currentUserFactory;

            private HousingRequestsService housingRequestService;

            private IEnumerable<HousingRequest> housingRequest;
            private IEnumerable<Term> terms;
            private IEnumerable<AcademicPeriod> academicPeriods;
            private IEnumerable<Building> buildings;
            private IEnumerable<Room> rooms;
            private IEnumerable<Location> locations;
            private IEnumerable<RoomWing> wings;
            private IEnumerable<RoomCharacteristic> roomCharacteristics;
            private IEnumerable<RoommateCharacteristics> roommateCharacteristics;

            private IEnumerable<FloorCharacteristics> floorCharacteristics;

            private Tuple<IEnumerable<HousingRequest>, int> housingRequestTuple;
            private Dictionary<string, string> personGuids;
            private Dictionary<string, string> roommateGuids;
            private string guid = "0ca1a878-3555-4a3f-a17b-20d054d5e001";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                housingRequestRepositoryMock = new Mock<IHousingRequestRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new HousingRequestUser();

                InitializeTestData();

                InitializeTestMock();

                housingRequestService = new HousingRequestsService(housingRequestRepositoryMock.Object, personRepositoryMock.Object, termRepositoryMock.Object,
                    roomRepositoryMock.Object, referenceDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

            }

            private void InitializeTestData()
            {
                housingRequest = new List<HousingRequest>() {
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e101",DateTime.Now,"R") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { BuildingReqdFlag="Y", Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1", Floor="floor_1", FloorReqd="Y" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1", LotteryNo=111 },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e102",DateTime.Now,"A") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { BuildingReqdFlag = "N", Building ="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1", LotteryNo=222 },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e103",DateTime.Now,"C") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1", LotteryNo=222 },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e104",DateTime.Now,"T") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1", LotteryNo=333 },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e201",DateTime.Now,"Status") { PersonId ="2", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_2", Site = "loc_Id_2", Room="room_2", Wing = "wing_code_2" } }, Term = "code_2" , RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="5" }, { new RoommatePreference() { RoommateId = "4" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_2" } },FloorCharacteristic="floor_char_2" }
                };

                housingRequestTuple = new Tuple<IEnumerable<HousingRequest>, int>(housingRequest, 2);
                personGuids = new Dictionary<string, string>() { { "1", "0ca1a878-3555-4a3f-a17b-20d054d5e461" }, { "2", "0ca1a878-3555-4a3f-a17b-20d054d5e462" }, { "3", "0ca1a878-3555-4a3f-a17b-20d054d5e463" }, { "4", "0ca1a878-3555-4a3f-a17b-20d054d5e464" }, { "5", "0ca1a878-3555-4a3f-a17b-20d054d5e465" } };
                roommateGuids = new Dictionary<string, string>() { { "11", "6ca1a878-3555-4a3f-a17b-20d054d5e461" }, { "22", "6ca1a878-3555-4a3f-a17b-20d054d5e462" }, { "33", "6ca1a878-3555-4a3f-a17b-20d054d5e463" } };
                terms = new List<Term>() {
                    new Term("code_1","desc1",DateTime.Now,DateTime.Now,2007,1,false,true,"Rep Term1",false),
                    new Term("code_2","desc2",DateTime.Now,DateTime.Now,2008,2,false,true,"Rep Term2",false)
                };

                academicPeriods = new List<AcademicPeriod>() {
                    new AcademicPeriod("1ca1a878-3555-4a3f-a17b-20d054d5e101","code_1","desc1",DateTime.Now,DateTime.Now,2007,1,"Rep Term1","","", new List<RegistrationDate>() { new RegistrationDate("",DateTime.Now,DateTime.Now,DateTime.Now,null,null,null,null,null,null,null)}),
                    new AcademicPeriod("1ca1a878-3555-4a3f-a17b-20d054d5e102","code_2","desc2",DateTime.Now,DateTime.Now,2008,2,"Rep Term2","","", new List<RegistrationDate>() { new RegistrationDate("",DateTime.Now,DateTime.Now,DateTime.Now,null,null,null,null,null,null,null)})
                };

                buildings = new List<Building>() {
                    new Building("2ca1a878-3555-4a3f-a17b-20d054d5e102","build_1","building 1","loc_Id_1","build_type_1","desc1",new List<string>(),"","","","",null,null,"","",""),
                    new Building("2ca1a878-3555-4a3f-a17b-20d054d5e103","build_2","building 2","loc_Id_2","build_type_2","desc2",new List<string>(),"","","","",null,null,"","","")
                };

                rooms = new List<Room>() {
                    new Room("3ca1a878-3555-4a3f-a17b-20d054d5e101","build_1*room_1","desc1"),
                    new Room("3ca1a878-3555-4a3f-a17b-20d054d5e102","build_2*room_2","desc1")
                };

                locations = new List<Location>() { new Location("3ca1a878-3555-4a3f-a17b-20d054d5e101", "loc_Id_1", "location 1"), new Location("3ca1a878-3555-4a3f-a17b-20d054d5e102", "loc_Id_2", "location 2") };
                wings = new List<RoomWing>() { new RoomWing("4ca1a878-3555-4a3f-a17b-20d054d5e101", "wing_code_1", "desc1"), new RoomWing("4ca1a878-3555-4a3f-a17b-20d054d5e102", "wing_code_2", "desc2") };
                roomCharacteristics = new List<RoomCharacteristic>() { new RoomCharacteristic("5ca1a878-3555-4a3f-a17b-20d054d5e101", "roomChar_code_1", "desc1"), new RoomCharacteristic("5ca1a878-3555-4a3f-a17b-20d054d5e102", "roomChar_code_2", "desc2") };
                roommateCharacteristics = new List<RoommateCharacteristics>() { new RoommateCharacteristics("7ca1a878-3555-4a3f-a17b-20d054d5e101", "roommate_char_1", "desc1"), new RoommateCharacteristics("7ca1a878-3555-4a3f-a17b-20d054d5e101", "roommate_char_2", "desc1") };
                floorCharacteristics = new List<FloorCharacteristics>() { new FloorCharacteristics("8ca1a878-3555-4a3f-a17b-20d054d5e101", "floor_char_1", "desc1"), new FloorCharacteristics("8ca1a878-3555-4a3f-a17b-20d054d5e101", "floor_char_2", "desc1") };
            }

            private void InitializeTestMock()
            {
                viewHousingRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewHousingRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewHousingRequest });

                housingRequestRepositoryMock.Setup(h => h.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);

                termRepositoryMock.Setup(t => t.GetAsync()).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(terms)).Returns(academicPeriods);

                roomRepositoryMock.Setup(r => r.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(rooms);

                referenceDataRepositoryMock.Setup(r => r.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(buildings);
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(r => r.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(wings);
                referenceDataRepositoryMock.Setup(r => r.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristics);

                studentReferenceDataRepositoryMock.Setup(s => s.GetRoommateCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roommateCharacteristics);
                studentReferenceDataRepositoryMock.Setup(s => s.GetFloorCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(floorCharacteristics);

                housingRequestRepositoryMock.Setup(i => i.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(housingRequestTuple);

                housingRequestRepositoryMock.Setup(h => h.GetHousingRequestByGuidAsync(It.IsAny<string>())).ReturnsAsync(housingRequest.FirstOrDefault());
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingRequestRepositoryMock = null;
                personRepositoryMock = null;
                termRepositoryMock = null;
                roomRepositoryMock = null;
                referenceDataRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                configurationRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                housingRequestService = null;
            }

            #endregion

            #region GETALL

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task HousingRequestService_GetHousingRequestAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            public async Task HousingRequestService_GetHousingRequestAsync_When_There_Are_No_HousingRequest_Records()
            {
                housingRequestRepositoryMock.Setup(i => i.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(null);
                var result = await housingRequestService.GetHousingRequestsAsync(0, 100, false);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingRequestService_GetHousingRequestAsync_When_Person_Id_As_Null()
            {
                housingRequest = new List<HousingRequest>() {
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e201",DateTime.Now,"Status") { PersonId=null }
                };

                housingRequestTuple = new Tuple<IEnumerable<HousingRequest>, int>(housingRequest, 2);

                housingRequestRepositoryMock.Setup(i => i.GetHousingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(housingRequestTuple);
                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Person_NotFound()
            {
                personGuids = new Dictionary<string, string>() { { "7", "0ca1a878-3555-4a3f-a17b-20d054d5e463" } };
                housingRequestRepositoryMock.Setup(h => h.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuids);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_AcademicPeriod_NotFound()
            {
                academicPeriods = new List<AcademicPeriod>() {
                    new AcademicPeriod("1ca1a878-3555-4a3f-a17b-20d054d5e104","code_3","desc3",DateTime.Now,DateTime.Now,2007,1,"Reporting Term","","", new List<RegistrationDate>() { new RegistrationDate("",DateTime.Now,DateTime.Now,DateTime.Now,null,null,null,null,null,null,null)})
                };
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(terms)).Returns(academicPeriods);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Buildings_NotFound()
            {
                buildings = new List<Building>() {
                    new Building("2ca1a878-3555-4a3f-a17b-20d054d5e103","build_3","building 3"),
                    new Building("2ca1a878-3555-4a3f-a17b-20d054d5e104","build_4","building 4")
                };

                referenceDataRepositoryMock.Setup(h => h.GetBuildingsAsync(It.IsAny<bool>())).ReturnsAsync(buildings);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Site_NotFound()
            {
                locations = new List<Location>() { new Location("3ca1a878-3555-4a3f-a17b-20d054d5e103", "loc_3", "location 3"), new Location("3ca1a878-3555-4a3f-a17b-20d054d5e104", "loc_4", "location 4") };
                referenceDataRepositoryMock.Setup(h => h.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Room_NotFound()
            {
                rooms = new List<Room>() {
                    new Room("3ca1a878-3555-4a3f-a17b-20d054d5e103","building_1*room_3","desc3"),
                    new Room("3ca1a878-3555-4a3f-a17b-20d054d5e104","building_2*room_4","desc4")
                };

                roomRepositoryMock.Setup(h => h.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(rooms);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Wing_NotFound()
            {
                wings = new List<RoomWing>() { new RoomWing("4ca1a878-3555-4a3f-a17b-20d054d5e103", "wing_code_3", "desc1"), new RoomWing("4ca1a878-3555-4a3f-a17b-20d054d5e104", "wing_code_4", "desc2") };

                referenceDataRepositoryMock.Setup(r => r.GetRoomWingsAsync(It.IsAny<bool>())).ReturnsAsync(wings);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_RoomCharacteristics_NotFound()
            {
                roomCharacteristics = new List<RoomCharacteristic>() { new RoomCharacteristic("5ca1a878-3555-4a3f-a17b-20d054d5e103", "roomChar_code_3", "desc1"), new RoomCharacteristic("5ca1a878-3555-4a3f-a17b-20d054d5e104", "roomChar_code_4", "desc2") };

                referenceDataRepositoryMock.Setup(r => r.GetRoomCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roomCharacteristics);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_Roommate_NotFound()
            {

                roommateGuids = new Dictionary<string, string>() { { "1", "7ba1a878-3555-4a3f-a17b-20d054d5e461" }, { "6", "7ba1a878-3555-4a3f-a17b-20d054d5e461" } };
                housingRequestRepositoryMock.Setup(h => h.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(roommateGuids);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_RoommateCharacter_NotFound()
            {

                roommateCharacteristics = new List<RoommateCharacteristics>() { new RoommateCharacteristics("7ca1a878-3555-4a3f-a17b-20d054d5e106", "roommate_char_6", "desc1"), new RoommateCharacteristics("7ca1a878-3555-4a3f-a17b-20d054d5e108", "roommate_char_9", "desc1") };
                studentReferenceDataRepositoryMock.Setup(s => s.GetRoommateCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(roommateCharacteristics);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestService_GetHousingRequestAsync_FloorCharacteristics_NotFound()
            {
                floorCharacteristics = new List<FloorCharacteristics>() { new FloorCharacteristics("8ca1a878-3555-4a3f-a17b-20d054d5e105", "floor_char_5", "desc1"), new FloorCharacteristics("8ca1a878-3555-4a3f-a17b-20d054d5e106", "floor_char_3", "desc1") };
                studentReferenceDataRepositoryMock.Setup(s => s.GetFloorCharacteristicsAsync(It.IsAny<bool>())).ReturnsAsync(floorCharacteristics);

                await housingRequestService.GetHousingRequestsAsync(0, 100, false);
            }

            [TestMethod]
            public async Task HousingRequestService_GetHousingRequestsAsync()
            {
                var result = await housingRequestService.GetHousingRequestsAsync(0, 100, false);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task HousingRequestService_GetHousingRequestByGuidAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await housingRequestService.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            public async Task HousingRequestService_GetHousingRequestByGuidAsync()
            {
                var result = await housingRequestService.GetHousingRequestByGuidAsync("0ca1a878-3555-4a3f-a17b-20d054d5e101");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, housingRequest.FirstOrDefault().Guid);
            }

            #endregion
        }

        [TestClass]
        public class HousingRequestServiceTests_POST_PUT : StudentUserFactory
        {
            #region DECLCARATIONS

            protected Domain.Entities.Role createOrUpdateHousingRequest = new Domain.Entities.Role(1, "UPDATE.HOUSING.REQS");

            private Mock<IHousingRequestRepository> housingRequestRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<ILogger> loggerMock;

            private HousingRequestUser currentUserFactory;

            private HousingRequestsService housingRequestService;

            private Dtos.HousingRequest housingRequest;
            private HousingRequest domainHousingRequest;

            private IEnumerable<Term> terms;
            private IEnumerable<AcademicPeriod> academicPeriods;
            private IEnumerable<Building> buildings;
            private IEnumerable<Location> locations;
            private IEnumerable<Room> rooms;
            private IEnumerable<RoomWing> roomWings;
            private IEnumerable<RoomCharacteristic> roomCharacteristics;
            private IEnumerable<FloorCharacteristics> floorCharacteristics;
            private IEnumerable<RoommateCharacteristics> roommateCharacteristics;
            private Dictionary<string, string> personIds;

            private string guid = "0ca1a878-3555-4a3f-a17b-20d054d5e001";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                housingRequestRepositoryMock = new Mock<IHousingRequestRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new HousingRequestUser();

                InitializeTestData();

                InitializeTestMock();

                housingRequestService = new HousingRequestsService(housingRequestRepositoryMock.Object, personRepositoryMock.Object, termRepositoryMock.Object,
                    roomRepositoryMock.Object, referenceDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

            }

            private void InitializeTestData()
            {
                domainHousingRequest = new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e001", "1", DateTime.Today, "R")
                {
                    PersonId = "1",
                    LotteryNo = 1234567,
                    Term = "1",
                    FloorCharacteristic = "1",
                    RoomPreferences = new List<RoomPreference>()
                    {
                        new RoomPreference()
                        {
                            Building = "1",
                            BuildingReqdFlag = "Y",
                            Room = "1",
                            RoomReqdFlag = "Y",
                            Wing = "1",
                            WingReqdFlag = "Y",
                            Floor = "1",
                            FloorReqd = "Y"
                        }
                    },
                    RoomCharacerstics = new List<RoomCharacteristicPreference>()
                    {
                        new RoomCharacteristicPreference()
                        {
                            RoomCharacteristic = "1",
                            RoomCharacteristicRequired = "Y"
                        }
                    },
                    RoommatePreferences = new List<RoommatePreference>()
                    {
                        new RoommatePreference() { RoommateId = "1" },
                    },
                    RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>()
                    {
                        new RoommateCharacteristicPreference() {RoommateCharacteristic = "1", RoommateCharacteristicRequired = "Y"}
                    }
                };

                personIds = new Dictionary<string, string>()
                {
                    {"1", "0ca1a878-3555-4a3f-a17b-20d054d5e001"}
                };

                var preference = new HousingRequestPreferenceProperty()
                {
                    Building = new HousingPreferenceRequiredProperty()
                    {
                        Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"),
                        Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                    },
                    Site = new HousingPreferenceRequiredProperty()
                    {
                        Preferred = new Dtos.GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e001"),
                        Required = Dtos.EnumProperties.RequiredPreference.Optional
                    }
                };

                housingRequest = new Dtos.HousingRequest()
                {
                    Id = "0ca1a878-3555-4a3f-a17b-20d054d5e001",
                    Status = Dtos.EnumProperties.HousingRequestsStatus.Submitted,
                    StartOn = DateTime.Today,
                    EndOn = DateTime.Today.AddDays(100),
                    AcademicPeriods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2("1ca1a878-3555-4a3f-a17b-20d054d5e001") },
                    Person = new Dtos.GuidObject2("2ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Preferences = new List<HousingRequestPreferenceProperty>() { preference },
                    RoomCharacteristics = new List<HousingPreferenceRequiredProperty>()
                    {
                        new HousingPreferenceRequiredProperty()
                        {
                            Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                        }
                    },
                    FloorCharacteristics = new HousingPreferenceRequiredProperty()
                    {
                        Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"),
                        Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                    },
                    RoommatePreferences = new List<HousingRequestRoommatePreferenceProperty>()
                    {
                        new HousingRequestRoommatePreferenceProperty()
                        {
                            Roommate = new HousingPreferenceRequiredProperty()
                            {
                                Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"),
                                Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                            },
                            RoommateCharacteristic = new HousingPreferenceRequiredProperty()
                            {
                                Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"),
                                Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                            }
                        }
                    }
                };

                terms = new List<Term>()
                {
                    new Term("3ca1a878-3555-4a3f-a17b-20d054d5e001","1","desc",DateTime.Today, DateTime.Today.AddDays(10),2017, 1, true, true, "1", true){ }
                };

                academicPeriods = new List<AcademicPeriod>()
                {
                    new AcademicPeriod("1ca1a878-3555-4a3f-a17b-20d054d5e001","1","desc",DateTime.Today, DateTime.Today.AddDays(10),2017, 1,"1","1","1",null){ }
                };

                buildings = new List<Building>()
                {
                    new Building("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc", "1", "1", "desc", new List<string>(){ }, "city", "state", "postalcode", "country", null, null,
                    null, null, null){ }
                };

                locations = new List<Location>()
                {
                    new Location("5ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){}
                };

                rooms = new List<Room>()
                {
                    new Room("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1*1", "desc"){}
                };

                roomWings = new List<RoomWing>()
                {
                    new RoomWing("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){}
                };

                roomCharacteristics = new List<RoomCharacteristic>()
                {
                    new RoomCharacteristic("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){ }
                };

                floorCharacteristics = new List<FloorCharacteristics>()
                {
                    new FloorCharacteristics("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){ }
                };

                roommateCharacteristics = new List<RoommateCharacteristics>()
                {
                    new RoommateCharacteristics("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){ }
                };
            }

            private void InitializeTestMock()
            {
                createOrUpdateHousingRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateHousingRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createOrUpdateHousingRequest });

                housingRequestRepositoryMock.Setup(h => h.GetHousingRequestKeyAsync(It.IsAny<string>())).ReturnsAsync("1");
                housingRequestRepositoryMock.Setup(h => h.UpdateHousingRequestAsync(It.IsAny<HousingRequest>())).ReturnsAsync(domainHousingRequest);
                housingRequestRepositoryMock.Setup(h => h.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personIds);

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

                termRepositoryMock.Setup(t => t.GetAsync()).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(academicPeriods);

                referenceDataRepositoryMock.Setup(r => r.GetBuildingsAsync(true)).ReturnsAsync(buildings);
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(true)).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(r => r.GetRoomWingsAsync(true)).ReturnsAsync(roomWings);
                referenceDataRepositoryMock.Setup(r => r.GetRoomCharacteristicsAsync(true)).ReturnsAsync(roomCharacteristics);

                studentReferenceDataRepositoryMock.Setup(s => s.GetFloorCharacteristicsAsync(true)).ReturnsAsync(floorCharacteristics);
                studentReferenceDataRepositoryMock.Setup(s => s.GetRoommateCharacteristicsAsync(true)).ReturnsAsync(roommateCharacteristics);

                roomRepositoryMock.Setup(r => r.GetRoomsAsync(true)).ReturnsAsync(rooms);
            }

            [TestCleanup]
            public void Cleanup()
            {
                housingRequestRepositoryMock = null;
                personRepositoryMock = null;
                termRepositoryMock = null;
                roomRepositoryMock = null;
                referenceDataRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                configurationRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                housingRequestService = null;
            }

            #endregion

            #region POST & PUT

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestService_CreateHousingRequestAsync_Dto_Null()
            {
                await housingRequestService.CreateHousingRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task HousingRequestService_CreateHousingRequestAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingRequestService_CreateHousingRequestAsync_Status_As_Approved()
            {
                housingRequest.Status = Dtos.EnumProperties.HousingRequestsStatus.Approved;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task HousingRequestService_CreateHousingRequestAsync_Invalid_Status()
            {
                housingRequest.Status = Dtos.EnumProperties.HousingRequestsStatus.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_EndOn_And_AcademicPeriods_Are_Required()
            {
                housingRequest.EndOn = null;
                housingRequest.AcademicPeriods = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_AcademicPeriods_Id_As_Null()
            {
                housingRequest.AcademicPeriods.FirstOrDefault().Id = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_MoreThan_One_AcademicPeriods()
            {
                housingRequest.AcademicPeriods.Add(new Dtos.GuidObject2(guid));
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_AcademicPeriod_NotFound()
            {
                housingRequest.AcademicPeriods.FirstOrDefault().Id = "5ca1a878-3555-4a3f-a17b-20d054d5e001";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Building_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Building = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Building_Preferred_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Building.Preferred = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Building_PreferredId_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Building.Preferred = new Dtos.GuidObject2();
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Building_Invalid_Required_Value()
            {
                housingRequest.Preferences.FirstOrDefault().Building.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Site_Preferred_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Site.Preferred = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Site_PreferredId_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Site.Preferred = new Dtos.GuidObject2();
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Site_Invalid_Required_Value()
            {
                housingRequest.Preferences.FirstOrDefault().Site.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Having_Values_For_Room_And_Floor()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                };

                housingRequest.Preferences.FirstOrDefault().Floor = new HousingFloorPreferenceRequiredProperty()
                {
                    Preferred = "3ca1a878-3555-4a3f-a17b-20d054d5e001"
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Having_Values_For_Room_And_Wing()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                };

                housingRequest.Preferences.FirstOrDefault().Wing = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001")
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Room_Preferred_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty() { Preferred = null };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Room_PreferredId_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty() { Preferred = new Dtos.GuidObject2() };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Room_Invalid_Required_Value()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Required = Dtos.EnumProperties.RequiredPreference.NotSet
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_More_Than_One_Same_Room_And_Building()
            {
                housingRequest.Preferences = new List<HousingRequestPreferenceProperty>()
                {
                    new HousingRequestPreferenceProperty()
                    {
                        Building = new HousingPreferenceRequiredProperty()
                        { Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory },
                        Room = new HousingPreferenceRequiredProperty()
                        { Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory }
                    },
                    new HousingRequestPreferenceProperty()
                    {
                        Building = new HousingPreferenceRequiredProperty()
                        { Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory },
                        Room = new HousingPreferenceRequiredProperty()
                        { Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory }
                    }
                };

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Floor_Preferred_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Floor = new HousingFloorPreferenceRequiredProperty() { Preferred = null };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Floor_Invalid_Required_Value()
            {
                housingRequest.Preferences.FirstOrDefault().Floor = new HousingFloorPreferenceRequiredProperty()
                {
                    Preferred = "3ca1a878-3555-4a3f-a17b-20d054d5e001",
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Wing_Preferred_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Wing = new HousingPreferenceRequiredProperty() { Preferred = null };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Wing_PreferredId_As_Null()
            {
                housingRequest.Preferences.FirstOrDefault().Wing = new HousingPreferenceRequiredProperty() { Preferred = new Dtos.GuidObject2() };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Wing_Invalid_Required_Value()
            {
                housingRequest.Preferences.FirstOrDefault().Wing = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Required = Dtos.EnumProperties.RequiredPreference.NotSet
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_Preferences_With_Invalid_Building_Id()
            {
                housingRequest.Preferences = new List<HousingRequestPreferenceProperty>()
                {
                    new HousingRequestPreferenceProperty()
                    {
                        Building = new HousingPreferenceRequiredProperty()
                        { Preferred = new Dtos.GuidObject2("3ca1a878-3555-4a3f-a17b-20d054d5e001"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory }
                    }
                };

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_Preferences_With_Invalid_Location_Id()
            {
                buildings = new List<Building>()
                {
                    new Building("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc", "2", "1", "desc", new List<string>(){ }, "city", "state", "postalcode", "country", null, null,
                    null, null, null){ }
                };

                referenceDataRepositoryMock.Setup(r => r.GetBuildingsAsync(true)).ReturnsAsync(buildings);

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CreateHousingRequestAsync_Preferences_Site_NotFound_For_Given_Guid()
            {
                housingRequest.Preferences.FirstOrDefault().Site.Preferred = new Dtos.GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e002");

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_Preferences_Room_With_InvalidId()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("5ca1a878-3555-4a3f-a17b-20d054d5e002"),
                    Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                };

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_Preferences_Room_NotMatching_With_Building()
            {
                housingRequest.Preferences.FirstOrDefault().Room = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e001"),
                    Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                };
                rooms = new List<Room>()
                {
                    new Room("4ca1a878-3555-4a3f-a17b-20d054d5e001", "2*1", "desc"){ }
                };

                roomRepositoryMock.Setup(r => r.GetRoomsAsync(true)).ReturnsAsync(rooms);

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_Preferences_BuildingWing_Notfound()
            {
                housingRequest.Preferences.FirstOrDefault().Wing = new HousingPreferenceRequiredProperty()
                {
                    Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"),
                    Required = Dtos.EnumProperties.RequiredPreference.Mandatory
                };

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoomCharacteristics_Prefered_As_Null()
            {
                housingRequest.RoomCharacteristics.FirstOrDefault().Preferred = null;

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoomCharacteristics_PreferedId_As_Null()
            {
                housingRequest.RoomCharacteristics.FirstOrDefault().Preferred = new Dtos.GuidObject2();

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoomCharacteristics_Required_As_NotSet()
            {
                housingRequest.RoomCharacteristics.FirstOrDefault().Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoomCharacteristics_Preferred_As_Same_Type()
            {
                housingRequest.RoomCharacteristics = new List<HousingPreferenceRequiredProperty>()
                {
                    new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory},
                    new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory}
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_RoomCharacteristics_NotFound_For_Given_Guid()
            {
                roomCharacteristics = new List<RoomCharacteristic>()
                {
                    new RoomCharacteristic("5ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc"){ }
                };
                referenceDataRepositoryMock.Setup(r => r.GetRoomCharacteristicsAsync(true)).ReturnsAsync(roomCharacteristics);

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_FloorCharacteristics_Prefered_As_Null()
            {
                housingRequest.FloorCharacteristics.Preferred = null;

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_FloorCharacteristics_PreferedId_As_Null()
            {
                housingRequest.FloorCharacteristics.Preferred = new Dtos.GuidObject2();

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_FloorCharacteristics_Required_As_NotSet()
            {
                housingRequest.FloorCharacteristics.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsyncFloorCharacteristics_NotFound_For_Given_Guid()
            {
                floorCharacteristics = new List<FloorCharacteristics>()
                {
                    new FloorCharacteristics("4ca1a878-3555-4a3f-a17b-20d054d5e002", "1", "desc"){ }
                };
                studentReferenceDataRepositoryMock.Setup(s => s.GetFloorCharacteristicsAsync(true)).ReturnsAsync(floorCharacteristics);

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommatePreferences_Prefered_As_Null()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().Roommate.Preferred = null;

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommatePreferences_PreferedId_As_Null()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().Roommate.Preferred = new Dtos.GuidObject2();

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommatePreferences_Required_As_NotSet()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().Roommate.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommatePreferences_With_Duplicates()
            {
                housingRequest.RoommatePreferences = new List<HousingRequestRoommatePreferenceProperty>()
                {
                    new HousingRequestRoommatePreferenceProperty()
                    {
                        Roommate = new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory}
                    },
                    new HousingRequestRoommatePreferenceProperty()
                    {
                        Roommate = new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory}
                    },
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommateCharacteristic_Prefered_As_Null()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().RoommateCharacteristic.Preferred = null;

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommateCharacteristic_PreferedId_As_Null()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().RoommateCharacteristic.Preferred = new Dtos.GuidObject2();

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommateCharacteristic_Required_As_NotSet()
            {
                housingRequest.RoommatePreferences.FirstOrDefault().RoommateCharacteristic.Required = Dtos.EnumProperties.RequiredPreference.NotSet;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_RoommateCharacteristic_With_Duplicates()
            {
                housingRequest.RoommatePreferences = new List<HousingRequestRoommatePreferenceProperty>()
                {
                    new HousingRequestRoommatePreferenceProperty()
                    {
                        RoommateCharacteristic = new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory}
                    },
                    new HousingRequestRoommatePreferenceProperty()
                    {
                        RoommateCharacteristic = new HousingPreferenceRequiredProperty(){Preferred = new Dtos.GuidObject2("4ca1a878-3555-4a3f-a17b-20d054d5e002"), Required = Dtos.EnumProperties.RequiredPreference.Mandatory}
                    },
                };
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_RoommatePreferences_Roommate_NotFound()
            {
                personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_RoommateCharacteristics_NotFound_For_Given_Guid()
            {
                roommateCharacteristics = new List<RoommateCharacteristics>()
                {
                    new RoommateCharacteristics("4ca1a878-3555-4a3f-a17b-20d054d5e002", "1", "desc"){ }
                };
                studentReferenceDataRepositoryMock.Setup(s => s.GetRoommateCharacteristicsAsync(true)).ReturnsAsync(roommateCharacteristics);

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_PersonId_Null()
            {
                domainHousingRequest.PersonId = null;
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_Person_Notfound_For_Given_Key()
            {
                domainHousingRequest.PersonId = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_AcademicPeriod_Notfound_For_Given_Key()
            {
                domainHousingRequest.Term = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_Building_Notfound_For_Given_Key()
            {
                domainHousingRequest.RoomPreferences.FirstOrDefault().Building = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_Location_Notfound_For_Given_Key()
            {
                var buildingsWithInvalidLocationId = new List<Building>()
                {
                    new Building("4ca1a878-3555-4a3f-a17b-20d054d5e001", "1", "desc", "3", "1", "desc", new List<string>(){ }, "city", "state", "postalcode", "country", null, null,
                    null, null, null){ }
                };

                referenceDataRepositoryMock.SetupSequence(r => r.GetBuildingsAsync(true)).Returns(Task.FromResult<IEnumerable<Building>>(buildings))
                    .Returns(Task.FromResult<IEnumerable<Building>>(buildingsWithInvalidLocationId));

                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }
            

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_Room_Notfound_For_Given_Key()
            {
                domainHousingRequest.RoomPreferences.FirstOrDefault().Room = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_ConvertEntityToDto_Wing_Notfound_For_Given_Key()
            {
                domainHousingRequest.RoomPreferences.FirstOrDefault().Wing = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_EntityToDto_RoomCharacterstics_Notfound_For_Given_Key()
            {
                domainHousingRequest.RoomCharacerstics.FirstOrDefault().RoomCharacteristic = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_EntityToDto_FloorCharacterstics_Notfound_For_Given_Key()
            {
                domainHousingRequest.FloorCharacteristic = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_EntityToDto_RoommatePreference_Notfound_For_Given_Key()
            {
                domainHousingRequest.RoommatePreferences.FirstOrDefault().RoommateId = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateHousingRequestAsync_EntityToDto_RoommateCharactersticPreference_Notfound()
            {
                domainHousingRequest.RoommateCharacteristicPreferences.FirstOrDefault().RoommateCharacteristic = "3";
                await housingRequestService.CreateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            public async Task HousingRequestService_CreateHousingRequestAsync()
            {
                var result = await housingRequestService.CreateHousingRequestAsync(housingRequest);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, housingRequest.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestService_UpdateHousingRequestAsync_Dto_As_Null()
            {
                var result = await housingRequestService.UpdateHousingRequestAsync(guid, null);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, housingRequest.Id);
            }

            [TestMethod]
            public async Task HousingRequestService_UpdateHousingRequestAsync()
            {
                var result = await housingRequestService.UpdateHousingRequestAsync(guid, housingRequest);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, housingRequest.Id);
            }

            #endregion
        }
    }
}
