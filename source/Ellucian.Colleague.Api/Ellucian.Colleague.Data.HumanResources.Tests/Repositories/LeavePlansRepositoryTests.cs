using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class LeavePlansRepositoryTests_V11
    {
        [TestClass]
        public class LeavePlansRepositoryTests_GET_AND_GETALL : BaseRepositorySetup
        {
            #region DECLARATIONS

            private LeavePlansRepository leavePlansRepository;
            private Dictionary<string, GuidLookupResult> dicResult;
            private List<string> leavePlanKeys;
            private Leavplan leavePlan;
            private Collection<Leavplan> leavePlans;
            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                leavePlansRepository = new LeavePlansRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "LEAVPLAN", PrimaryKey = "1" } } };
                leavePlanKeys = new List<string>() { "1" };

                leavePlan = new Leavplan()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    LpnStartDate = DateTime.Today,
                    LpnDesc = "description",
                    LpnType = "1",
                    LpnAccrualMethod = "1",
                    LpnYrStartDate = (DateTime.Today - new DateTime(1967, 12, 31)).TotalDays.ToString(),
                    LpnEndDate = DateTime.Today.AddDays(100),
                    LpnRolloverLeaveType = "1",
                    LpnAccrualFrequency = "1",
                    LpnDaysAllowed = 10,
                    LpnAllowNegative = "Y",
                    LpnEarntypes = new List<string> { "VAC" },
                    LpnLeaveReporting = null
                };

                leavePlans = new Collection<Leavplan>() { leavePlan };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("LEAVPLAN", It.IsAny<string>())).ReturnsAsync(leavePlanKeys.ToArray());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(leavePlans);

                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(leavePlans);                
                dataReaderMock.Setup(r => r.ReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(leavePlan);

                var ids = leavePlans.Select(x => x.Recordkey).ToList();
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "LeavePlanValues",
                    Entity = "LEAVPLAN",
                    Sublist = ids.ToList(),
                    TotalCount = ids.Count,
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
            }

            #endregion

            [TestMethod]
            public async Task GetLeavePlansAsync_GetCacheApiKeys_Returns_Null()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                  .ReturnsAsync(() => null);
                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetLeavePlansAsync_GetCacheApiKeys_Returns_Empty()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                  .ReturnsAsync(new GetCacheApiKeysResponse());

                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetLeavePlansAsync_GetCacheApiKeys_Exception()
            {
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ThrowsAsync(new RepositoryException());

                await leavePlansRepository.GetLeavePlansAsync(0, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetLeavePlansAsync_DataReader_BulkReadRecord_Exception()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ThrowsAsync(new RepositoryException());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string[]>(), It.IsAny<bool>()))
                   .ThrowsAsync(new RepositoryException());
                await leavePlansRepository.GetLeavePlansAsync(0, 1);
            }


            [TestMethod]
            public async Task LeavePlanRepository_GetLeavePlansAsync()
            {
                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(guid, result.Item1.FirstOrDefault().Guid);
            }


            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task LeavePlanRepository_MissingRequiredFields__GetLeavePlansAsync()
            {
                leavePlan = new Leavplan()
                {                 
                };

                leavePlans = new Collection<Leavplan>() { leavePlan };
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(leavePlans);

                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLeavePlansByIdAsync_GetRecordInfoFromGuidAsync_Entity_Null()
            {
                dicResult[guid].Entity = null;
                await leavePlansRepository.GetLeavePlansByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLeavePlansByIdAsync_GetRecordInfoFromGuidAsync_Invalid_Entity()
            {
                dicResult[guid].Entity = "LEAVEPLAN";
                await leavePlansRepository.GetLeavePlansByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLeavePlansByIdAsync_GetRecordInfoFromGuidAsync_Invalid_PrimaryKey()
            {
                dicResult[guid].PrimaryKey = null;
                await leavePlansRepository.GetLeavePlansByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLeavePlansByIdAsync_ReadRecordAsync_Record_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await leavePlansRepository.GetLeavePlansByIdAsync(guid);
            }

            [TestMethod]
            public async Task LeaveRepository_GetLeavePlansByIdAsync()
            {
                var result = await leavePlansRepository.GetLeavePlansByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetLeavePlansAsync_With_Cache_BulkReadRecordAsync_Throws_Exception()
            {
                dataReaderMock.Setup(r => r.IsAnonymous).Returns(true);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                await leavePlansRepository.GetLeavePlansAsync(true);
            }

            [TestMethod]
            public async Task LeavePlansRepository_GetLeavePlansAsync_With_Cache()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), true)).ReturnsAsync(leavePlans);
                var result = await leavePlansRepository.GetLeavePlansAsync(true);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.FirstOrDefault().Guid);
            }
        }
    }
}