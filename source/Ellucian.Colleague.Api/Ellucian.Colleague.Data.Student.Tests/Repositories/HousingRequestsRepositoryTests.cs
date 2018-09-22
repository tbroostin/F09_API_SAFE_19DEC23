// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Threading;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class HousingRequestsRepositoryTests : BaseRepositorySetup
    {
        [TestClass]
        public class GetHousingRequestsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;

            //Data contract objects returned as responses by the mocked datareaders
            private RoomRequestsIntg housingResponseData;
            private Collection<RoomRequestsIntg> housingResponseCollection;
            private RoomPreferences roomPreferenceResponseData;
            private Collection<RoomPreferences> roomPreferenceResponseCollection;

            //TestRepositories
            private TestHousingRequestsRepository expectedRepository;
            private HousingRequestRepository actualRepository;

            //Test data
            private HousingRequest expectedHousing;
            private HousingRequest actualHousing;
            private List<HousingRequest> expectedHousings;

            //used throughout
            private string housingId;
            private string[] housingIds;
            private string housingGuid;
            int offset = 0;
            int limit = 200;


            [TestInitialize]
            public async void Initialize()
            {
                expectedRepository = new TestHousingRequestsRepository();

                //setup the expected housings
                var pageOfHousings = await expectedRepository.GetHousingRequestsAsync(offset, limit, false);
                expectedHousings = pageOfHousings.Item1.ToList();

                housingId = expectedHousings.First().RecordKey;
                housingGuid = expectedHousings.First().Guid;
                expectedHousing = await expectedRepository.GetHousingRequestByGuidAsync(housingGuid);

                //set the response data objects
                housingIds = expectedHousings.Select(ap => ap.RecordKey).ToArray();
                housingResponseCollection = BuildResponseData(expectedHousings);
                housingResponseData = housingResponseCollection.FirstOrDefault(ap => ap.RecordGuid == housingGuid);

                roomPreferenceResponseCollection = BuildRoomPreferenceResponseData(expectedHousings);
                roomPreferenceResponseData = roomPreferenceResponseCollection.FirstOrDefault();

                //build the repository
                actualRepository = BuildRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                loggerMock = null;
                transFactoryMock = null;

                housingResponseData = null;
                expectedRepository = null;
                actualRepository = null;
                expectedHousings = null;
                expectedHousing = null;
                actualHousing = null;
                housingId = null;
                housingIds = null;
            }

            private HousingRequestRepository BuildRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Logger Mock
                loggerMock = new Mock<ILogger>();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettingsMock = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Single Application
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var appl = housingResponseCollection.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, appl == null ? null : new GuidLookupResult() { Entity = "ROOM.REQUESTS.INTG", PrimaryKey = appl.Recordkey });
                    }
                    return Task.FromResult(result);
                });
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<RoomRequestsIntg>(string.Empty, true)).ReturnsAsync(housingResponseCollection);
                //dataReaderMock.Setup<Task<Collection<RoomPreferences>>>(dr => dr.BulkReadRecordAsync<RoomPreferences>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<RoomPreferences>());
                
                // Multiple Housings
                //dataReaderMock.Setup(dr => dr.SelectAsync("APPLICATIONS", It.IsAny<string>())).ReturnsAsync(housingIds);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", It.IsAny<string[]>(), true)).ReturnsAsync(roomPreferenceResponseCollection);
                return new HousingRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            private Collection<RoomRequestsIntg> BuildResponseData(List<HousingRequest> applData)
            {
                var collection = new Collection<RoomRequestsIntg>();
                foreach (var appl in applData)
                {
                    var housingRecord = new RoomRequestsIntg()
                    {
                        Recordkey = appl.RecordKey,
                        RecordGuid = appl.Guid,
                        RmriStartDate = DateTime.Now,
                        RmriStatus = new List<string>() { appl.Status },
                        RmriTerm = appl.Term,
                        RmriPersonId = appl.PersonId,
                        RmriEndDate = DateTime.Now,
                        RmriLotteryNo = appl.LotteryNo,
                        RmriRoomPreferences = appl.RoomPreferences != null && appl.RoomPreferences.Any() ? appl.RoomPreferences.FirstOrDefault().Room : null,

                        RmriStatusDate = new List<DateTime?>(),
                        RoomReqIntgStatusesEntityAssociation = new List<RoomRequestsIntgRoomReqIntgStatuses>(),
                        RmriRoomAssignment = "",
                    };
                    //foreach (var status in appl.AdmissionApplicationStatuses)
                    //{
                    //    housingRecord.ApplStatus.Add(status.ApplicationStatus);
                    //    housingRecord.ApplStatusDate.Add(status.ApplicationStatusDate);
                    //    housingRecord.ApplStatusTime.Add(status.ApplicationStatusTime);
                    //    housingRecord.ApplDecisionBy.Add(string.Empty);
                    //}
                    housingRecord.buildAssociations();

                    collection.Add(housingRecord);
                }
                return collection;
            }

            private Collection<RoomPreferences> BuildRoomPreferenceResponseData(List<HousingRequest> applData)
            {
                var roomPreferences = new Collection<RoomPreferences>();
                var collection1 = new List<RoomPreferencesRmprPreferences>();
                var collection2 = new List<RoomPreferencesRoommatePreferences>();
                var collection3 = new List<RoomPreferencesRoommateChars>();
                if (applData != null && applData.Any())
                {
                    foreach (var appl in applData.FirstOrDefault().RoomPreferences)
                    {
                        var roomPreferenceRecord = new RoomPreferencesRmprPreferences()
                        {
                            RmprBldgFloorPreferencesAssocMember = Int32.Parse(appl.Floor),
                            RmprBldgFloorReqdFlagAssocMember = appl.FloorReqd,
                            RmprBldgPreferencesAssocMember = appl.Building,
                            RmprBldgReqdFlagAssocMember = appl.BuildingReqdFlag,
                            RmprBldgWingPreferencesAssocMember = appl.Wing,
                            RmprRoomPreferencesAssocMember = appl.Room,
                            RmprRoomReqdFlagAssocMember = appl.RoomReqdFlag,
                            RmprWingReqdFlagAssocMember = appl.WingReqdFlag,
                        };
                        var roommatePrefernceRecord = new RoomPreferencesRoommatePreferences()
                        {
                            RmprRoommatePreferencesAssocMember = appl.Wing,
                            RmprRoommateReqdFlagAssocMember = appl.WingReqdFlag,
                        };

                        collection1.Add(roomPreferenceRecord);
                        collection2.Add(roommatePrefernceRecord);

                        roomPreferences.Add(new RoomPreferences() { Recordkey = "room", RmprPreferencesEntityAssociation = collection1, RoommateCharsEntityAssociation = collection3, RoommatePreferencesEntityAssociation = collection2 });
                    }
                    
                    return roomPreferences;
                }

                return null;
                
            }

            //[TestMethod]
            //public async Task GetHousingRequestByIdAsync()
            //{
            //    actualHousing = await actualRepository.GetHousingRequestByGuidAsync(housingGuid);
            //    Assert.AreEqual(expectedHousing.PersonId, actualHousing.PersonId);
            //    Assert.AreEqual(expectedHousing.RecordKey, actualHousing.RecordKey);
            //    //Assert.IsTrue(expectedHousing.StartDate.Equals(actualHousing.StartDate));
            //    Assert.AreEqual(expectedHousing.Status, actualHousing.Status);
            //    Assert.AreEqual(expectedHousing.PersonId, actualHousing.PersonId);
            //    Assert.AreEqual(expectedHousing.Term, actualHousing.Term);
            //    //Assert.IsTrue(expectedHousing.EndDate.Equals(actualHousing.EndDate));
            //    Assert.AreEqual(expectedHousing.LotteryNo, actualHousing.LotteryNo);
            //}

            //[TestMethod]
            //public async Task GetHousingRequestsAsync()
            //{
            //    var actuals = await actualRepository.GetHousingRequestsAsync(offset, limit, false);

            //    Assert.AreEqual(expectedHousings.Count(), actuals.Item1.Count());

            //    foreach (var actual in actuals.Item1)
            //    {
            //        var expected = expectedHousings.FirstOrDefault(i => i.RecordKey.Equals(actual.RecordKey, StringComparison.OrdinalIgnoreCase));
            //        Assert.IsNotNull(expected);
            //        Assert.AreEqual(expected.PersonId, actual.PersonId);
            //        Assert.AreEqual(expected.RecordKey, actual.RecordKey);
            //        //Assert.AreEqual(expected.StartDate, actual.StartDate);
            //        Assert.AreEqual(expected.Status, actual.Status);
            //        //Assert.AreEqual(expected.PersonId, actual.PersonId);
            //        Assert.AreEqual(expected.Term, actual.Term);
            //        //Assert.AreEqual(expected.EndDate, actual.EndDate);
            //        Assert.AreEqual(expected.LotteryNo, actual.LotteryNo);
            //    }
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task HousingIdRequiredTest()
            //{
            //    await actualRepository.GetHousingRequestByGuidAsync(null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            //public async Task NullHousingRecord_ExceptionTest()
            //{
            //    //set the response data object to null and the dataReaderMock
            //    housingResponseData = null;
            //    dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);

            //    actualHousing = await actualRepository.GetHousingRequestByGuidAsync(housingGuid);
            //}

            //[TestMethod]
            //public async Task NullApplicationRecord_LogsErrorTest()
            //{
            //    var exceptionCaught = false;
            //    try
            //    {
            //        //set the response data object to null and the dataReaderMock
            //        housingResponseData = null;
            //        dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);

            //        actualHousing = await actualRepository.GetHousingRequestByGuidAsync(housingGuid);
            //    }
            //    catch
            //    {
            //        exceptionCaught = true;
            //    }

            //    Assert.IsTrue(exceptionCaught);
            //    // loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            //}
        }

        //[TestClass]
        //public class PutHousingRequestsTests
        //{
        //    Mock<IColleagueTransactionFactory> transFactoryMock;
        //    Mock<ICacheProvider> cacheProviderMock;
        //    Mock<IColleagueDataReader> dataReaderMock;
        //    Mock<IColleagueTransactionInvoker> mockManager;
        //    Mock<ILogger> loggerMock;
        //    ApiSettings apiSettingsMock;
        //    CreateUpdateRoomReqRequest updateRequest;

        //    //Data contract objects returned as responses by the mocked datareaders
        //    private RoomRequestsIntg housingResponseData;
        //    private Collection<RoomRequestsIntg> housingResponseCollection;
        //    private RoomPreferences roomPreferenceResponseData;
        //    private Collection<RoomPreferences> roomPreferenceResponseCollection;

        //    //TestRepositories
        //    private TestHousingRequestsRepository expectedRepository;
        //    private HousingRequestRepository actualRepository;

        //    //Test data
        //    private HousingRequest expectedHousing;
        //    private HousingRequest actualHousing;
        //    private List<HousingRequest> expectedHousings;

        //    //used throughout
        //    private string housingId;
        //    private string[] housingIds;
        //    private string housingGuid;
        //    int offset = 0;
        //    int limit = 200;


        //    [TestInitialize]
        //    public async void Initialize()
        //    {
        //        expectedRepository = new TestHousingRequestsRepository();

        //        //setup the expected housings
        //        var pageOfHousings = await expectedRepository.GetHousingRequestsAsync(offset, limit, false);
        //        expectedHousings = pageOfHousings.Item1.ToList();

        //        housingId = expectedHousings.First().RecordKey;
        //        housingGuid = expectedHousings.First().Guid;
        //        expectedHousing = await expectedRepository.GetHousingRequestByGuidAsync(housingGuid);

        //        //set the response data objects
        //        housingIds = expectedHousings.Select(ap => ap.RecordKey).ToArray();
        //        housingResponseCollection = BuildResponseData(expectedHousings);
        //        housingResponseData = housingResponseCollection.FirstOrDefault(ap => ap.RecordGuid == housingGuid);

        //        roomPreferenceResponseCollection = BuildRoomPreferenceResponseData(expectedHousings);
        //        roomPreferenceResponseData = roomPreferenceResponseCollection.FirstOrDefault();

        //        //build the repository
        //        actualRepository = BuildRepository();
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        cacheProviderMock = null;
        //        dataReaderMock = null;
        //        loggerMock = null;
        //        transFactoryMock = null;

        //        housingResponseData = null;
        //        expectedRepository = null;
        //        actualRepository = null;
        //        expectedHousings = null;
        //        expectedHousing = null;
        //        actualHousing = null;
        //        housingId = null;
        //        housingIds = null;
        //    }

        //    private HousingRequestRepository BuildRepository()
        //    {
        //        // transaction factory mock
        //        transFactoryMock = new Mock<IColleagueTransactionFactory>();
        //        // Cache Provider Mock
        //        cacheProviderMock = new Mock<ICacheProvider>();
        //        // Logger Mock
        //        loggerMock = new Mock<ILogger>();

        //        mockManager = new Mock<IColleagueTransactionInvoker>();

        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        //        // Set up data accessor for mocking 
        //        dataReaderMock = new Mock<IColleagueDataReader>();
        //        apiSettingsMock = new ApiSettings("TEST");

        //        // Set up dataAccessorMock as the object for the DataAccessor
        //        transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
        //        // Set up successful response to a transaction request, capturing the completed request for verification
        //        transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                
        //        // Single Application
        //        dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
        //        {
        //            var result = new Dictionary<string, GuidLookupResult>();
        //            foreach (var gl in gla)
        //            {
        //                var appl = housingResponseCollection.FirstOrDefault(x => x.RecordGuid == gl.Guid);
        //                result.Add(gl.Guid, appl == null ? null : new GuidLookupResult() { Entity = "ROOM.REQUESTS.INTG", PrimaryKey = appl.Recordkey });
        //            }
        //            return Task.FromResult(result);
        //        });
        //        dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);
        //        dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<RoomRequestsIntg>(string.Empty, true)).ReturnsAsync(housingResponseCollection);
        //        //dataReaderMock.Setup<Task<Collection<RoomPreferences>>>(dr => dr.BulkReadRecordAsync<RoomPreferences>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<RoomPreferences>());

        //        // Multiple Housings
        //        //dataReaderMock.Setup(dr => dr.SelectAsync("APPLICATIONS", It.IsAny<string>())).ReturnsAsync(housingIds);
        //        dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", It.IsAny<string[]>(), true)).ReturnsAsync(roomPreferenceResponseCollection);
        //        return new HousingRequestRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        //    }

        //    private Collection<RoomRequestsIntg> BuildResponseData(List<HousingRequest> applData)
        //    {
        //        var collection = new Collection<RoomRequestsIntg>();
        //        foreach (var appl in applData)
        //        {
        //            var housingRecord = new RoomRequestsIntg()
        //            {
        //                Recordkey = appl.RecordKey,
        //                RecordGuid = appl.Guid,
        //                RmriStartDate = DateTime.Now,
        //                RmriStatus = new List<string>() { appl.Status },
        //                RmriTerm = appl.Term,
        //                RmriPersonId = appl.PersonId,
        //                RmriEndDate = DateTime.Now,
        //                RmriLotteryNo = appl.LotteryNo,
        //                RmriRoomPreferences = appl.RoomPreferences != null && appl.RoomPreferences.Any() ? appl.RoomPreferences.FirstOrDefault().Room : null,

        //                RmriStatusDate = new List<DateTime?>(),
        //                RoomReqIntgStatusesEntityAssociation = new List<RoomRequestsIntgRoomReqIntgStatuses>(),
        //                RmriRoomAssignment = "",
        //            };
        //            //foreach (var status in appl.AdmissionApplicationStatuses)
        //            //{
        //            //    housingRecord.ApplStatus.Add(status.ApplicationStatus);
        //            //    housingRecord.ApplStatusDate.Add(status.ApplicationStatusDate);
        //            //    housingRecord.ApplStatusTime.Add(status.ApplicationStatusTime);
        //            //    housingRecord.ApplDecisionBy.Add(string.Empty);
        //            //}
        //            housingRecord.buildAssociations();

        //            collection.Add(housingRecord);
        //        }
        //        return collection;
        //    }

        //    private Collection<RoomPreferences> BuildRoomPreferenceResponseData(List<HousingRequest> applData)
        //    {
        //        var roomPreferences = new Collection<RoomPreferences>();
        //        var collection1 = new List<RoomPreferencesRmprPreferences>();
        //        var collection2 = new List<RoomPreferencesRoommatePreferences>();
        //        var collection3 = new List<RoomPreferencesRoommateChars>();
        //        if (applData != null && applData.Any())
        //        {
        //            foreach (var appl in applData.FirstOrDefault().RoomPreferences)
        //            {
        //                var roomPreferenceRecord = new RoomPreferencesRmprPreferences()
        //                {
        //                    RmprBldgFloorPreferencesAssocMember = Int32.Parse(appl.Floor),
        //                    RmprBldgFloorReqdFlagAssocMember = appl.FloorReqd,
        //                    RmprBldgPreferencesAssocMember = appl.Building,
        //                    RmprBldgReqdFlagAssocMember = appl.BuildingReqdFlag,
        //                    RmprBldgWingPreferencesAssocMember = appl.Wing,
        //                    RmprRoomPreferencesAssocMember = appl.Room,
        //                    RmprRoomReqdFlagAssocMember = appl.RoomReqdFlag,
        //                    RmprWingReqdFlagAssocMember = appl.WingReqdFlag,
        //                };
        //                var roommatePrefernceRecord = new RoomPreferencesRoommatePreferences()
        //                {
        //                    RmprRoommatePreferencesAssocMember = appl.Wing,
        //                    RmprRoommateReqdFlagAssocMember = appl.WingReqdFlag,
        //                };

        //                collection1.Add(roomPreferenceRecord);
        //                collection2.Add(roommatePrefernceRecord);

        //                roomPreferences.Add(new RoomPreferences() { Recordkey = "room", RmprPreferencesEntityAssociation = collection1, RoommateCharsEntityAssociation = collection3, RoommatePreferencesEntityAssociation = collection2 });
        //            }

        //            return roomPreferences;
        //        }

        //        return null;

        //    }

        //    [TestMethod]
        //    public async Task UpdateHousingRequestAsync()
        //    {
        //        CreateUpdateRoomReqResponse updateResponse = new CreateUpdateRoomReqResponse() { Guid = housingGuid };
        //        mockManager.Setup(mgr => mgr.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(It.IsAny<CreateUpdateRoomReqRequest>())).Returns(Task.FromResult(updateResponse)).Callback<CreateUpdateRoomReqRequest>(req => updateRequest = req);

        //        actualHousing = await actualRepository.UpdateHousingRequestAsync(expectedHousing);
        //        Assert.AreEqual(expectedHousing.PersonId, actualHousing.PersonId);
        //        Assert.AreEqual(expectedHousing.RecordKey, actualHousing.RecordKey);
        //        //Assert.IsTrue(expectedHousing.StartDate.Equals(actualHousing.StartDate));
        //        Assert.AreEqual(expectedHousing.Status, actualHousing.Status);
        //        Assert.AreEqual(expectedHousing.PersonId, actualHousing.PersonId);
        //        Assert.AreEqual(expectedHousing.Term, actualHousing.Term);
        //        //Assert.IsTrue(expectedHousing.EndDate.Equals(actualHousing.EndDate));
        //        Assert.AreEqual(expectedHousing.LotteryNo, actualHousing.LotteryNo);
        //    }

        //    [TestMethod]
        //    public async Task GetHousingRequestsAsync()
        //    {
        //        var actuals = await actualRepository.GetHousingRequestsAsync(offset, limit, false);

        //        Assert.AreEqual(expectedHousings.Count(), actuals.Item1.Count());

        //        foreach (var actual in actuals.Item1)
        //        {
        //            var expected = expectedHousings.FirstOrDefault(i => i.RecordKey.Equals(actual.RecordKey, StringComparison.OrdinalIgnoreCase));
        //            Assert.IsNotNull(expected);
        //            Assert.AreEqual(expected.PersonId, actual.PersonId);
        //            Assert.AreEqual(expected.RecordKey, actual.RecordKey);
        //            //Assert.AreEqual(expected.StartDate, actual.StartDate);
        //            Assert.AreEqual(expected.Status, actual.Status);
        //            //Assert.AreEqual(expected.PersonId, actual.PersonId);
        //            Assert.AreEqual(expected.Term, actual.Term);
        //            //Assert.AreEqual(expected.EndDate, actual.EndDate);
        //            Assert.AreEqual(expected.LotteryNo, actual.LotteryNo);
        //        }
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public async Task HousingIdRequiredTest()
        //    {
        //        await actualRepository.GetHousingRequestByGuidAsync(null);
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task NullHousingRecord_ExceptionTest()
        //    {
        //        //set the response data object to null and the dataReaderMock
        //        housingResponseData = null;
        //        dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);

        //        actualHousing = await actualRepository.GetHousingRequestByGuidAsync(housingGuid);
        //    }

        //    [TestMethod]
        //    public async Task NullApplicationRecord_LogsErrorTest()
        //    {
        //        var exceptionCaught = false;
        //        try
        //        {
        //            //set the response data object to null and the dataReaderMock
        //            housingResponseData = null;
        //            dataReaderMock.Setup(dr => dr.ReadRecordAsync<RoomRequestsIntg>("ROOM.REQUESTS.INTG", housingId, true)).ReturnsAsync(housingResponseData);

        //            actualHousing = await actualRepository.GetHousingRequestByGuidAsync(housingGuid);
        //        }
        //        catch
        //        {
        //            exceptionCaught = true;
        //        }

        //        Assert.IsTrue(exceptionCaught);
        //        // loggerMock.Verify(l => l.Error(It.IsAny<string>()));
        //    }
        //}

    }
}
