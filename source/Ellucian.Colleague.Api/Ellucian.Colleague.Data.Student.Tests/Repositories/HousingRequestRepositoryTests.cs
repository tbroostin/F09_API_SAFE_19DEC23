using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class HousingRequestRepositoryTests_V10
    {
        [TestClass]
        public class HousingRequestRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
        {
            #region DECLARATIONS

            Collection<HousingRequest> housingRequest;

            Collection<RoomPreferences> roomPreferences;

            Collection<RoomAssignment> roomAssignment;

            private HousingRequestRepository housingRequestRepository;

            //private Mock<BaseColleagueRepository> baseColleagueRepository;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            private Dictionary<string, GuidLookupResult> lookUpResult;

            private Dictionary<string, RecordKeyLookupResult> rKeyLookUpResult;

            private IEnumerable<string> personRecordKeys;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                //baseColleagueRepository = new Mock<BaseColleagueRepository>();

                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                housingRequestRepository = new HousingRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                housingRequestRepository = null;

                //baseColleagueRepository = null;
            }

            private void InitializeTestData()
            {

                housingRequest = new Collection<HousingRequest>()
                {
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e101",DateTime.Now,"R") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1" },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e102",DateTime.Now,"A") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1" },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e103",DateTime.Now,"C") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1" },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e104",DateTime.Now,"T") { PersonId="1", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_1", Site= "loc_Id_1", Room="room_1", Wing="wing_code_1" }  }, Term = "code_1", RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="4" }, { new RoommatePreference() { RoommateId = "5" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_1" } }, FloorCharacteristic="floor_char_1" },
                     new HousingRequest("0ca1a878-3555-4a3f-a17b-20d054d5e201",DateTime.Now,"Status") { PersonId ="2", RoomPreferences = new List<RoomPreference>() { new RoomPreference() { Building="build_2", Site = "loc_Id_2", Room="room_2", Wing = "wing_code_2" } }, Term = "code_2" , RoomCharacerstics = new List<RoomCharacteristicPreference>() { new RoomCharacteristicPreference() { RoomCharacteristic= "roomChar_code_1" }, new RoomCharacteristicPreference() { RoomCharacteristic = "roomChar_code_2"} }, RoommatePreferences = new List<RoommatePreference>() { new RoommatePreference() { RoommateId="5" }, { new RoommatePreference() { RoommateId = "4" } } }, RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>() { new RoommateCharacteristicPreference() { RoommateCharacteristic= "roommate_char_2" } },FloorCharacteristic="floor_char_2" }
                };

                string[] requestedIds1 = { "1", "2" };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllHousingRequestsRecordKeys",
                    Entity = "ROOM.ASSIGNMENT",
                    Sublist = requestedIds1.ToList(),
                    TotalCount = 2,
                    KeyCacheInfo = new List<KeyCacheInfo>()
               {
                   new KeyCacheInfo()
                   {
                       KeyCacheMax = 5905,
                       KeyCacheMin = 1,
                       KeyCachePart = "000",
                       KeyCacheSize = 5905
                   },
                   new KeyCacheInfo()
                   {
                       KeyCacheMax = 7625,
                       KeyCacheMin = 5906,
                       KeyCachePart = "001",
                       KeyCacheSize = 1720
                   }
               }
                };
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                roomPreferences = new Collection<RoomPreferences>() {
                    new RoomPreferences { Recordkey="1", RmprPersonId="1", RmprPreferencesEntityAssociation = new List<RoomPreferencesRmprPreferences>() { new RoomPreferencesRmprPreferences() {  RmprBldgFloorReqdFlagAssocMember="flag", RmprBldgPreferencesAssocMember="ASC", RmprBldgReqdFlagAssocMember="Reqd", RmprBldgWingPreferencesAssocMember="Wing" } }, RoomCharsEntityAssociation = new List<RoomPreferencesRoomChars>() { new RoomPreferencesRoomChars() { RmprRoomCharReqdFlagAssocMember="REq", RmprRoomCharsAssocMember="Mem" } }, RoommateCharsEntityAssociation = new List<RoomPreferencesRoommateChars>() { new RoomPreferencesRoommateChars() { RmprMateCharsReqdFlagAssocMember="Req", RmprRoommateCharsAssocMember="MEM" } }, RoommatePreferencesEntityAssociation = new List<RoomPreferencesRoommatePreferences>() { new RoomPreferencesRoommatePreferences() { RmprRoommatePreferencesAssocMember="Mem" } } },
                    new RoomPreferences { Recordkey="2", RmprPersonId="2", RmprPreferencesEntityAssociation = new List<RoomPreferencesRmprPreferences>() { new RoomPreferencesRmprPreferences() {  RmprBldgFloorReqdFlagAssocMember="flag", RmprBldgPreferencesAssocMember="ASC", RmprBldgReqdFlagAssocMember="Reqd", RmprBldgWingPreferencesAssocMember="Wing" } } }
                };

                roomAssignment = new Collection<RoomAssignment>() {
                    new RoomAssignment { Recordkey="1", RecordGuid="3a46eef8-5fe7-4120-b1cf-f23266b9e874", RmasPreference="1", RmasIntgKeyIdx="1", RmasStartDate= DateTime.Now, RmasStatus= new List<string> {"R","A" }, RmasStatusesEntityAssociation= new List<RoomAssignmentRmasStatuses>() { new RoomAssignmentRmasStatuses() { RmasStatusAssocMember="R" }, new RoomAssignmentRmasStatuses() { RmasStatusAssocMember="A" } }  },
                    new RoomAssignment { Recordkey="2", RecordGuid="3a46eef8-5fe7-4120-b1cf-f23266b9e875", RmasPreference="2", RmasIntgKeyIdx="2", RmasStartDate= DateTime.Now, RmasStatus= new List<string> {"R","A" }, RmasStatusesEntityAssociation= new List<RoomAssignmentRmasStatuses>() { new RoomAssignmentRmasStatuses() { RmasStatusAssocMember="R" }, new RoomAssignmentRmasStatuses() { RmasStatusAssocMember="A" } } }
                };

                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "1", SecondaryKey = "1" } }, { "2", new GuidLookupResult { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "2", SecondaryKey = "2" } } };
                rKeyLookUpResult = new Dictionary<string, RecordKeyLookupResult>() { { "ROOM.ASSIGNMENT+1+1", new RecordKeyLookupResult { Guid = "0ca1a878-3555-4a3f-a17b-20d054d5e101" } }, { "ROOM.ASSIGNMENT+2+2", new RecordKeyLookupResult { Guid = "0ca1a878-3555-4a3f-a17b-20d054d5e102" } } };
                personRecordKeys = new List<string>() { "1", "2", "3" };




            }

            private void InitializeTestMock()
            {

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                dataReaderMock.Setup(d => d.SelectAsync("ROOM.ASSIGNMENT", It.IsAny<string>())).ReturnsAsync(new List<string>() { "1'ý'1", "2'ý'2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(rKeyLookUpResult);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1'ý'1", "2'ý'2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<RoomPreferences>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(roomPreferences);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(roomAssignment);

                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.RoomAssignment>()
                {
                    BulkRecordsRead = new Collection<RoomAssignment>() {roomAssignment[0], roomAssignment[1]},
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);
                
                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(roomAssignment.FirstOrDefault());
                //baseColleagueRepository.Setup(b => b.GetGuidFromRecordInfoAsync("ROOM.ASSIGNMENT", It.IsAny<string>(), "RMAS.INTG.KEY.IDX", It.IsAny<string>())).ReturnsAsync(guid);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task HousingRequestRepository_GetHousingRequestsAsync_Exception()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await housingRequestRepository.GetHousingRequestsAsync(0, 10, false);
            }

            [TestMethod]
            public async Task HousingRequestRepository_GetHousingRequestsAsync_WithRoomAssignmentData_Null()
            {
                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.RoomAssignment>()
                {
                    BulkRecordsRead = null,
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);
                var result = await housingRequestRepository.GetHousingRequestsAsync(0, 10, false);
                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            public async Task HousingRequestRepository_GetHousingRequestsAsync_WithRoomAssignmentData_Empty()
            {
                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.RoomAssignment>()
                {
                    BulkRecordsRead = new Collection<RoomAssignment>(),
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);
                var result = await housingRequestRepository.GetHousingRequestsAsync(0, 10, false);
                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task HousingRequestRepository_GetHousingRequestsAsync_KeyNotFoundException()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ThrowsAsync(new RepositoryException());
                
                await housingRequestRepository.GetHousingRequestsAsync(0, 10, false);

            }

            [TestMethod]
            public async Task HousingRequestRepository_GetHousingRequestsAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(rKeyLookUpResult);

                var result = await housingRequestRepository.GetHousingRequestsAsync(0, 10, false);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_EmptyId()
            {
                await housingRequestRepository.GetHousingRequestByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_KeyNotFound_When_DictionaryEmpty()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { });
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_RepositoryException_When_EntityNull()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_RepositoryException_When_ValueNull()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", null } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_GuidLookUpException()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new Exception());
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_GuidLookUpKeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new KeyNotFoundException());
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_ThrowsKeyNotFound()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new KeyNotFoundException());
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync_KeyNotFoundWhenHousingRequestIs_Null()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestRepository_GetHousingRequestKeyAsync_HousingRequestKey_Null()
            {
                await housingRequestRepository.GetHousingRequestKeyAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingRequestRepository_GetHousingRequestKeyAsync_Returns_Null()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "", SecondaryKey = "1" } } };

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);

                await housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            }

            [TestMethod]
            public async Task HousingRequestRepository_GetHousingRequestByIdAsync()
            {
                await housingRequestRepository.GetHousingRequestByGuidAsync("0ca1a878-3555-4a3f-a17b-20d054d5e101");

            }

            [TestMethod]
            public async Task HousingRequestRepository_GetPersonGuidsAsync_PersonRecordAsNull()
            {
                var result = await housingRequestRepository.GetPersonGuidsAsync(new List<string>() { });
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task HousingRequestRepository_GetPersonGuidsAsync()
            {
                var result = await housingRequestRepository.GetPersonGuidsAsync(personRecordKeys);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 2);
            }

        }

        [TestClass]
        public class HousingRequestRepositoryTests_POST_AND_PUT : BaseRepositorySetup
        {
            #region DECLARATIONS

            private HousingRequestRepository housingRequestRepository;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            private CreateUpdateRoomReqResponse response;
            private HousingRequest housingRequest;

            private Dictionary<string, GuidLookupResult> lookUpResult;
            private Dictionary<string, RecordKeyLookupResult> recordKeyLookupResult;

            private RoomAssignment roomAssignment;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                housingRequestRepository = new HousingRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                housingRequestRepository = null;
            }

            private void InitializeTestData()
            {
                housingRequest = new HousingRequest("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", DateTime.Today, "Y")
                {
                   Term = "1",
                   EndDate = DateTime.Today.AddDays(10),
                   LotteryNo = 123456,
                   PersonId = "1",
                   FloorCharacteristic = "1",
                   FloorCharacteristicReqd = "Y",
                   RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>()
                   {
                       new RoommateCharacteristicPreference()
                       {
                           RoommateCharacteristic = "1",
                           RoommateCharacteristicRequired = "Y"
                       }
                   },
                   RoommatePreferences = new List<RoommatePreference>()
                   {
                       new RoommatePreference()
                       {
                           RoommateId = "1",
                           RoommateRequired = "Y"
                       }
                   },
                   RoomPreferences = new List<RoomPreference>()
                   {
                       new RoomPreference()
                       {
                           Building = "1",
                           BuildingReqdFlag = "Y",
                           Room = "1",
                           RoomReqdFlag = "Y",
                           Floor = "1",
                           FloorReqd = "Y",
                           Wing = "1",
                           WingReqdFlag = "Y"
                       }
                   },
                   RoomCharacerstics = new List<RoomCharacteristicPreference>()
                   {
                       new RoomCharacteristicPreference()
                       {
                           RoomCharacteristic = "1",
                           RoomCharacteristicRequired = "Y"
                       }
                   }
                };

                response = new CreateUpdateRoomReqResponse()
                {
                    Guid = guid
                };

                lookUpResult = new Dictionary<string, GuidLookupResult>()
                {
                    {
                        "1",
                        new GuidLookupResult
                        {
                            Entity = "ROOM.ASSIGNMENT",
                            PrimaryKey = "1",
                            SecondaryKey = "1"
                        }
                    }
                };

                roomAssignment = new RoomAssignment()
                {
                    Recordkey = "1",
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    RmasStartDate = DateTime.Today,
                    RmasIntgKeyIdx = "1",
                    RmasPreference = "1",
                    RmasStatusesEntityAssociation = new List<RoomAssignmentRmasStatuses>()
                    {
                        new RoomAssignmentRmasStatuses("1", DateTime.Today) { RmasStatusAssocMember = "1" }
                    }
                };

                recordKeyLookupResult = new Dictionary<string, RecordKeyLookupResult>()
                {
                    { "ROOM.ASSIGNMENT+1+1", new RecordKeyLookupResult() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", ModelName = "" } }
                };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>())).ReturnsAsync(response);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>("ROOM.ASSIGNMENT", It.IsAny<string>(), true)).ReturnsAsync(roomAssignment);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResult);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<RoomPreferences>() { });
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync_Entity_As_Null()
            {
                await housingRequestRepository.UpdateHousingRequestAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync_Invalid_Response()
            {
                response = new CreateUpdateRoomReqResponse()
                {
                    CreateUpdateRoomRequestErrors = new List<CreateUpdateRoomRequestErrors>()
                    {
                        new CreateUpdateRoomRequestErrors(){ErrorCodes = "ERROR_CODE", ErrorMessages = "ERROR_MESSAGE"}
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>())).ReturnsAsync(response);
                await housingRequestRepository.UpdateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync_ArgumentNullException()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>()))
                    .ThrowsAsync(new ArgumentNullException());
                await housingRequestRepository.UpdateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync_InvalidOperationException()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>()))
                    .ThrowsAsync(new InvalidOperationException());
                await housingRequestRepository.UpdateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync_Exception()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>()))
                    .ThrowsAsync(new Exception());
                await housingRequestRepository.UpdateHousingRequestAsync(housingRequest);
            }

            [TestMethod]
            public async Task HousingRequestRepository_UpdateHousingRequestAsync()
            {
                var result = await housingRequestRepository.UpdateHousingRequestAsync(housingRequest);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }
        }
    }
}
