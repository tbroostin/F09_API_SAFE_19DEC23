using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
                    LpnEarntypes = new List<string> { "VAC" }
                };

                leavePlans = new Collection<Leavplan>() { leavePlan };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("LEAVPLAN", It.IsAny<string>())).ReturnsAsync(leavePlanKeys.ToArray());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>(It.IsAny<string[]>(), false)).ReturnsAsync(leavePlans);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), true)).ReturnsAsync(leavePlan);
            }

            #endregion

            [TestMethod]
            public async Task GetLeavePlansAsync_DataReader_Returns_Null()
            {
                dataReaderMock.Setup(r => r.SelectAsync("LEAVPLAN", It.IsAny<string>())).ReturnsAsync(null);
                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task GetLeavePlansAsync_DataReader_Returns_Empty()
            {
                dataReaderMock.Setup(r => r.SelectAsync("LEAVPLAN", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await leavePlansRepository.GetLeavePlansAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetLeavePlansAsync_DataReader_SelectAsync_Exception()
            {
                dataReaderMock.Setup(r => r.SelectAsync("LEAVPLAN", It.IsAny<string>())).ThrowsAsync(new Exception());
                await leavePlansRepository.GetLeavePlansAsync(0, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetLeavePlansAsync_DataReader_BulkReadRecord_Exception()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Leavplan>(It.IsAny<string[]>(), false)).ThrowsAsync(new Exception());
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
                dataReaderMock.Setup(r => r.ReadRecordAsync<Leavplan>("LEAVPLAN", It.IsAny<string>(), true)).ReturnsAsync(null);
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
            public async Task GetLeavePlansAsync_With_Cache_BulkReadRecordAsyncc_Throws_Exception()
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
