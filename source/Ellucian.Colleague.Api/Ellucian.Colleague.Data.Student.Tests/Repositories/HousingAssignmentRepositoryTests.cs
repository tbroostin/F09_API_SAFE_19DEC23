//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Ellucian.Colleague.Data.Student.Transactions;
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class HousingAssignmentRepositoryTests_V10
    {
        [TestClass]
        public class HousingAssignmentRepositoryTests_GETALL_GETBYID
        {
            #region DECLARATIONS

            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueTransactionFactory> transactionFactoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IColleagueDataReader> dataReaderMock;

            private HousingAssignmentRepository housingAssignmentRepository;

            private string[] housingAssignmentIds;
            private string[] additionalAmountIds;
            private string[] roomPrefIds;
            private Collection<DataContracts.RoomAssignment> roomAssignments;
            private Collection<DataContracts.ArAddnlAmts> additionalAmounts;
            private List<RoomAssignmentRmasStatuses> roomStatuses;
            private Dictionary<string, RecordKeyLookupResult> personIds;
            private Dictionary<string, string> haDict;
            private Dictionary<string, string> hrDict;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                cacheProviderMock = new Mock<ICacheProvider>();
                transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();

                transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                InitializeTestData();

                housingAssignmentRepository = new HousingAssignmentRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                transactionFactoryMock = null;
                loggerMock = null;
                dataReaderMock = null;

                housingAssignmentRepository = null;
            }

            private void InitializeTestData()
            {
                housingAssignmentIds = new string[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874", "2a49eed8-5fe7-4120-b1cf-f23266b9e875" };
                additionalAmountIds = new string[] { "1bf5e697-8168-4203-b48b-c667556cfb8a", "2bf5e697-8168-4203-b48b-c667556cfb8a" };
                roomPrefIds = new string[] { "1cf5e697-8168-4203-b48b-c667556cfb8a", "2cf5e697-8168-4203-b48b-c667556cfb8a" };

                roomStatuses = new List<RoomAssignmentRmasStatuses>()
                {
                    new RoomAssignmentRmasStatuses() { RmasStatusAssocMember = "1", RmasStatusDateAssocMember = System.DateTime.Today},
                    new RoomAssignmentRmasStatuses() { RmasStatusAssocMember = "2"}
                };

                roomAssignments = new Collection<DataContracts.RoomAssignment>()
                {
                    new DataContracts.RoomAssignment()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1", RmasPersonId = "1", RmasRoom = "1",
                        RmasStartDate = System.DateTime.Today, RmasEndDate = System.DateTime.Today.AddDays(100), RmasPreference = "1",
                        RmasStatusesEntityAssociation = roomStatuses
                    },
                    new DataContracts.RoomAssignment()
                    {
                        RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e875", Recordkey = "2", RmasPersonId = "2", RmasRoom = "2",
                        RmasStartDate = System.DateTime.Today, RmasEndDate = System.DateTime.Today.AddDays(100),
                    }
                };

                additionalAmounts = new Collection<DataContracts.ArAddnlAmts>()
                {
                    new DataContracts.ArAddnlAmts() { Recordkey = "1", AraaRoomAssignment = "1" },
                    new DataContracts.ArAddnlAmts() { Recordkey = "2", AraaRoomAssignment = "2" }
                };

                haDict = new Dictionary<string, string>();
                haDict.Add("1", "1a49eed8-5fe7-4120-b1cf-f23266b9e874");
                hrDict = new Dictionary<string, string>();
                hrDict.Add("1", "2a49eed8-5fe7-4120-b1cf-f23266b9e875");

                dataReaderMock.SetupSequence(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" }))
                    .Returns(Task.FromResult(new[] { "2a49eed8-5fe7-4120-b1cf-f23266b9e875" }));

                personIds = new Dictionary<string, RecordKeyLookupResult>()
                {
                    { "PERSON+1", new RecordKeyLookupResult() { Guid= "26ed68cf-ecfa-4c1a-8953-45b4df5e695a", ModelName = "PERSON" } },
                    { "PERSON+2", new RecordKeyLookupResult() { Guid= "6c138879-35f2-4ab8-8bbf-ef4dbc316d3d", ModelName = "PERSON" } }
                };                
                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personIds);
            }

            #endregion

            [TestMethod]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentsAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(housingAssignmentIds);
                dataReaderMock.SetupSequence(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                              .Returns(Task.FromResult(additionalAmountIds));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.RoomAssignment>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(roomAssignments);
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.RoomAssignment>() { BulkRecordsRead = roomAssignments };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(results);

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.ArAddnlAmts>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(additionalAmounts);

                var result = await housingAssignmentRepository.GetHousingAssignmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(roomAssignments.Count, result.Item2);
                Assert.AreEqual(roomAssignments.FirstOrDefault().RecordGuid, result.Item1.FirstOrDefault().Guid);
            }

            [TestMethod]
            public async Task HousingAssignmentRepositoryTests_GetPersonGuidsAsync_Empty_Arguments()
            {
                var results = await housingAssignmentRepository.GetPersonGuidsAsync(new List<string>());
                Assert.IsNull(results);
            }

            [TestMethod]
            public async Task HousingAssignmentRepositoryTests_GetPersonGuidsAsync_Null_Arguments()
            {
                var results = await housingAssignmentRepository.GetPersonGuidsAsync(null);
                Assert.IsNull(results);
            }

            [TestMethod]
            public async Task HousingAssignmentRepositoryTests_GetPersonGuidsAsync()
            {
                var results = await housingAssignmentRepository.GetPersonGuidsAsync(new List<string>() { "1", "2" });
                Assert.IsNotNull(results);

                foreach (var result in results)
                {
                    var key = string.Concat("PERSON", "+", result.Key);
                    var expected = personIds[key].Guid;
                    Assert.AreEqual(expected, result.Value);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentByGuidAsync_ArgumentNullException()
            {
                await housingAssignmentRepository.GetHousingAssignmentByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentByGuidAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }));

                await housingAssignmentRepository.GetHousingAssignmentByGuidAsync(roomAssignments.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_HousingAssignment_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(() => null);

                await housingAssignmentRepository.GetHousingAssignmentByGuidAsync(roomAssignments.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentByGuidAsync_KeyNotFoundException_When_RmasRoom_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                var record = roomAssignments.FirstOrDefault();
                record.RmasRoom = null;

                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(record);

                await housingAssignmentRepository.GetHousingAssignmentByGuidAsync(roomAssignments.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            public async Task HousingAssignmentRepositoryTests_GetHousingAssignmentByGuidAsyncl()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "1" } } }));

                dataReaderMock.Setup(d => d.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(roomAssignments.FirstOrDefault());

                dataReaderMock.SetupSequence(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                              .Returns(Task.FromResult(additionalAmountIds))
                              .Returns(Task.FromResult(roomPrefIds));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.ArAddnlAmts>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(() => null);

                var result = await housingAssignmentRepository.GetHousingAssignmentByGuidAsync(roomAssignments.FirstOrDefault().RecordGuid);

                Assert.IsNotNull(result);

                Assert.AreEqual(result.Guid, roomAssignments.FirstOrDefault().RecordGuid);
            }
        }

        [TestClass]
        public class HousingAssignmentRepositoryTests_POST_AND_PUT : BaseRepositorySetup
        {

            #region DECLARATIONS

            private HousingAssignmentRepository repository;
            private CreateUpdateHousingAssignResponse response;
            private Dictionary<string, GuidLookupResult> dicResult;
            private Dictionary<string, string> haDict;
            private Dictionary<string, string> hrDict;
            private HousingAssignment housingAssignment;
            private RoomAssignment roomAssignment;
            private IEnumerable<ArAdditionalAmount> additionalAmounts;
            private IEnumerable<RoomAssignmentRmasStatuses> statuses;
            private Collection<ArAddnlAmts> arAdditionalAmounts;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                repository = new HousingAssignmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                statuses = new List<RoomAssignmentRmasStatuses>()
                {
                    new RoomAssignmentRmasStatuses()
                    {
                        RmasStatusAssocMember = "1",
                        RmasStatusDateAssocMember = DateTime.Today
                    }
                };

                arAdditionalAmounts = new Collection<ArAddnlAmts>()
                {
                    new ArAddnlAmts()
                    {
                        AraaRoomAssignment = "1",
                    }
                };

                roomAssignment = new RoomAssignment()
                {
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    Recordkey = "1",
                    RmasRoom = "1",
                    RmasPersonId = "1",
                    RmasStartDate = DateTime.Today,
                    RmasEndDate = DateTime.Today,
                    RmasStatusesEntityAssociation = new List<RoomAssignmentRmasStatuses>()
                    {
                        new RoomAssignmentRmasStatuses()
                        {
                            RmasStatusAssocMember = "1",
                            RmasStatusDateAssocMember = DateTime.Today
                        }
                    },
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "1" } }
                };

                haDict = new Dictionary<string, string>();
                haDict.Add("1", "1a49eed8-5fe7-4120-b1cf-f23266b9e874");
                hrDict = new Dictionary<string, string>();
                hrDict.Add("1", "2a49eed8-5fe7-4120-b1cf-f23266b9e875");

                additionalAmounts = new List<ArAdditionalAmount>()
                {
                    new ArAdditionalAmount()
                    {
                        AraaArCode = "1",
                        AraaChargeAmt = 100,
                        AraaCrAmt = 0,
                        AraaRoomAssignmentId = "1",
                        Recordkey = "1",
                    }
                };

                response = new CreateUpdateHousingAssignResponse() { AGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };

                housingAssignment = new HousingAssignment("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1", "1", "1", DateTime.Today, DateTime.Today.AddDays(10))
                {
                    ArAdditionalAmounts = additionalAmounts,
                    StatusDate = DateTime.Today,
                };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateHousingAssignRequest, CreateUpdateHousingAssignResponse>(It.IsAny<CreateUpdateHousingAssignRequest>())).ReturnsAsync(response);
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" });
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "2a49eed8-5fe7-4120-b1cf-f23266b9e875" });
                dataReaderMock.Setup(r => r.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(roomAssignment);
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "1" });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<DataContracts.ArAddnlAmts>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(arAdditionalAmounts);

            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateHousingAssignmentAsync_ArgumentNullException_HousingAssignment_Null()
            {
                await repository.UpdateHousingAssignmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task UpdateHousingAssignmentAsync_InvalidOperationException()
            {
                response.CreateUpdareHousingAssignmentErrors = new List<CreateUpdareHousingAssignmentErrors>()
                {
                    new CreateUpdareHousingAssignmentErrors() { AlErrorCode = "1", AlErrorMsg = "Error" }
                };

                await repository.UpdateHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateHousingAssignmentAsync_Get_ArgumentNullException()
            {
                response.AGuid = null;

                await repository.UpdateHousingAssignmentAsync(housingAssignment);
            }
                        
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_Get_KeyNotFoundException_HousingAssignmentId_Null()
            {
                dicResult.FirstOrDefault().Value.PrimaryKey = null;

                await repository.UpdateHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_Get_KeyNotFoundException_HousingAssignmentDc_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<RoomAssignment>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);

                await repository.UpdateHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateHousingAssignmentAsync_Get_KeyNotFoundException_HousingAssignmentDc_RmasRoom_Null()
            {
                roomAssignment.RmasRoom = null;

                await repository.UpdateHousingAssignmentAsync(housingAssignment);
            }

            [TestMethod]
            public async Task HousingAssignmentRepository_UpdateHousingAssignmentAsync()
            {
                var result = await repository.UpdateHousingAssignmentAsync(housingAssignment);

                Assert.IsNotNull(result);
            }
        }
    }
}
