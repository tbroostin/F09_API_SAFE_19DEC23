using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
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
    public class EmployeeLeaveTransactionsRepositoryTests_V11
    {
        [TestClass]
        public class EmployeeLeaveTransactionsRepositoryTests_GET_AND_GETALL : BaseRepositorySetup
        {
            #region DECLARATIONS

            private EmployeeLeaveTransactionsRepository empLeaveTransRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private Tuple<IEnumerable<PerleaveDetails>, int> tupleEmployeeLeaveTransactions;

            private List<string> PerleaveDetailsKeys;

            private Perlvdtl perLeaveDetailsData;

            private Collection<Perlvdtl> empLeaveTrans;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                empLeaveTransRepository = new EmployeeLeaveTransactionsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERLVDTL", PrimaryKey = "1" } } };
                PerleaveDetailsKeys = new List<string>() { "1" };

                perLeaveDetailsData = new Perlvdtl()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    PldDate = DateTime.Today,
                    PldHours = 20,
                    PldPerleaveId = "1",
                    //PldCurrentBalance = 50,
                    PldForwardingBalance = 50
                };

                empLeaveTrans = new Collection<Perlvdtl>() { perLeaveDetailsData };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("PERLVDTL", It.IsAny<string>())).ReturnsAsync(PerleaveDetailsKeys.ToArray());
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perlvdtl>(It.IsAny<string[]>(), false)).ReturnsAsync(empLeaveTrans);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Perlvdtl>("PERLVDTL", It.IsAny<string>(), true)).ReturnsAsync(perLeaveDetailsData);
                //var empLeaveplanRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perlvdtl>("PERLVDTL", subList);

                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perlvdtl>("PERLVDTL", It.IsAny<string[]>(), true))
                    .ReturnsAsync(empLeaveTrans);

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllAccountingStringComponentValues",
                    Entity = "",
                    Sublist = new List<string>() { "1" },
                    TotalCount = 1,
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
            [ExpectedException(typeof(Exception))]
            public async Task GetEmployeeLeaveTransactionsAsync_DataReader_BulkReadRecord_Exception()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perlvdtl>("PERLVDTL", It.IsAny<string[]>(), true)).ThrowsAsync(new Exception());
                await empLeaveTransRepository.GetEmployeeLeaveTransactionsAsync(0, 1);
            }

            [TestMethod]
            public async Task LeavePlanRepository_GetEmployeeLeaveTransactionsAsync()
            {
                var result = await empLeaveTransRepository.GetEmployeeLeaveTransactionsAsync(0, 1);

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                Assert.AreEqual(guid, result.Item1.FirstOrDefault().Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetEmployeeLeaveTransactionsByIdAsync_GetRecordInfoFromGuidAsync_Entity_Null()
            {
                dicResult[guid].Entity = null;
                await empLeaveTransRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetEmployeeLeaveTransactionsByIdAsync_GetRecordInfoFromGuidAsync_Invalid_Entity()
            {
                dicResult[guid].Entity = "LEAVEPLAN";
                await empLeaveTransRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetEmployeeLeaveTransactionsByIdAsync_GetRecordInfoFromGuidAsync_Invalid_PrimaryKey()
            {
                dicResult[guid].PrimaryKey = null;
                await empLeaveTransRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetEmployeeLeaveTransactionsByIdAsync_ReadRecordAsync_Record_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Perlvdtl>("PERLVDTL", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await empLeaveTransRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);
            }

            [TestMethod]
            public async Task LeaveRepository_GetEmployeeLeaveTransactionsByIdAsync()
            {
                var result = await empLeaveTransRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Guid);
            }
        }
    }
}
