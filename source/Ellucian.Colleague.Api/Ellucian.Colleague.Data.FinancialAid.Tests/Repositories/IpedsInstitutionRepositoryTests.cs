// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;


namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class IpedsInstitutionRepositoryTests
    {
        [TestClass]
        public class GetIpedsInstitutionsTests : BaseRepositorySetup
        {
            private Collection<IpedsInstitutions> ipedsInstitutionsResponseData;
            
            private IEnumerable<IpedsInstitution> expectedIpedsInstitutionList;
            private IEnumerable<IpedsInstitution> actualIpedsInstitutionList;

            private TestIpedsInstitutionRepository expectedRepository;
            private IpedsInstitutionRepository actualRepository;

            private UpdateIpedsInstitutionsRequest actualUpdateIpedsInstitutionTransactionRequest;

            private List<string> inputOpeIds;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestIpedsInstitutionRepository();
                inputOpeIds = expectedRepository.ipedsDbDataList.Select(d => d.opeId).ToList();
                expectedIpedsInstitutionList = await expectedRepository.GetIpedsInstitutionsAsync(inputOpeIds);

                ipedsInstitutionsResponseData = BuildIpedsDbDataContracts(expectedRepository.ipedsDbDataList);

                actualRepository = BuildIpedsInstitutionRepository();
                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(inputOpeIds);
            }



            [TestMethod]
            public void IpedsInstitutionTest()
            {
                Assert.IsNotNull(expectedIpedsInstitutionList);
                Assert.IsNotNull(actualIpedsInstitutionList);
                Assert.IsTrue(expectedIpedsInstitutionList.Count() > 0);
                Assert.IsTrue(actualIpedsInstitutionList.Count() > 0);
                Assert.AreEqual(expectedIpedsInstitutionList.Count(), actualIpedsInstitutionList.Count());
            }

            [TestMethod]
            public void DbRecordAttributesTest()
            {
                var objectsFromDbData = actualIpedsInstitutionList.Where(i => i.Id != string.Empty);
                foreach (var ipedsInstitution in objectsFromDbData)
                {
                    var expectedData = ipedsInstitutionsResponseData.FirstOrDefault(i => i.Recordkey == ipedsInstitution.Id);
                    Assert.IsNotNull(expectedData);

                    Assert.AreEqual(expectedData.IiUnitId, ipedsInstitution.UnitId);
                    Assert.AreEqual(expectedData.IiOpeId.TrimStart('0'), ipedsInstitution.OpeId);
                    Assert.AreEqual(expectedData.IiInstitutionName, ipedsInstitution.Name);

                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullOpeIdListThrowsExceptionTest()
            {
                await actualRepository.GetIpedsInstitutionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyOpeIdListThrowsExceptionTest()
            {
                await actualRepository.GetIpedsInstitutionsAsync(new List<string>());
            }

            [TestMethod]
            public async Task NoDuplicateIpedsInstitutionsTest()
            {
                var duplicateOpeId = inputOpeIds.First();
                inputOpeIds.Add(duplicateOpeId);

                var expectedOpeIds = inputOpeIds.Distinct();
                Assert.AreEqual(expectedOpeIds.Count() + 1, inputOpeIds.Count());

                actualRepository = BuildIpedsInstitutionRepository();
                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(inputOpeIds);
                var actualOpeIds = actualIpedsInstitutionList.Select(i => i.OpeId);

                Assert.AreEqual(expectedOpeIds.Count(), actualOpeIds.Count());
                Assert.AreEqual(actualOpeIds.Count(), actualOpeIds.Distinct().Count());
            }

            [TestMethod]
            public async Task PageBulkRecordReadsTest()
            {
                var dbOpeIds = ipedsInstitutionsResponseData.Select(i => i.IiOpeId);

                var opeQueue = new Queue<string>(dbOpeIds);
                apiSettings.BulkReadSize = 1;
                actualRepository = BuildIpedsInstitutionRepository();

                var numDataReaderCalls = 0;

                dataReaderMock.Setup<Task<Collection<IpedsInstitutions>>>(d => d.BulkReadRecordAsync<IpedsInstitutions>(It.IsAny<string[]>(), true))
                    .Returns<string[], bool>(
                        (a, b) => 
                        {
                            var response = ipedsInstitutionsResponseData.Where(i => i.IiOpeId == a[0]);
                            return Task.FromResult(new Collection<IpedsInstitutions>(response.ToList()));
                        }
                    ).Callback(
                        () =>
                            numDataReaderCalls++
                    );

                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(dbOpeIds);

                Assert.AreEqual(dbOpeIds.Count(), numDataReaderCalls);
            }

            [TestMethod]
            public async Task NoDbRecords_ReturnEmptyListTest()
            {
                ipedsInstitutionsResponseData = new Collection<IpedsInstitutions>();
                
                actualRepository = BuildIpedsInstitutionRepository();

                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(inputOpeIds);

                Assert.AreEqual(0, actualIpedsInstitutionList.Count());
            }

            [TestMethod]
            public async Task ObjectsCreatedFromDb_WriteToCacheTest()
            {
                var dbOpeIds = ipedsInstitutionsResponseData.Select(i => i.IiOpeId);

                string cacheKey = actualRepository.BuildFullCacheKey("AllIpedsInstitutions");
                cacheProviderMock.Setup(c => c.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(c => c.GetAndLockSemaphoreAsync(cacheKey, null)).Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1))));
                cacheProviderMock.Setup(c => c.AddAndUnlockSemaphore(cacheKey, It.IsAny<IEnumerable<IpedsInstitution>>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<IpedsInstitutions>("", true)).ReturnsAsync(ipedsInstitutionsResponseData);

                //Verify object list was returned, which means it came from the "db".
                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(dbOpeIds);
                Assert.AreEqual(dbOpeIds.Count(), actualIpedsInstitutionList.Count());

                cacheProviderMock.Verify(c => c.AddAndUnlockSemaphore(cacheKey, actualIpedsInstitutionList, It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task ObjectsCreatedFromDb_GetFromCacheTest()
            {
                var dbOpeIds = ipedsInstitutionsResponseData.Select(i => i.IiOpeId);

                string cacheKey = actualRepository.BuildFullCacheKey("AllIpedsInstitutions");
                cacheProviderMock.Setup(c => c.Contains(cacheKey, null)).Returns(true);
                cacheProviderMock.Setup(c => c.Get(cacheKey, null)).Returns(actualIpedsInstitutionList).Verifiable();

                dataReaderMock.Setup(d => d.BulkReadRecord<IpedsInstitutions>("", true)).Returns(new Collection<IpedsInstitutions>());

                actualIpedsInstitutionList = await actualRepository.GetIpedsInstitutionsAsync(dbOpeIds);
                Assert.IsTrue(actualIpedsInstitutionList.Count() >= 3);

                cacheProviderMock.Verify(c => c.Get(cacheKey, null));
            }

            
            private IpedsInstitutionRepository BuildIpedsInstitutionRepository()
            {
                //mock up the db calls for IpedsInsitutions and fahubParams
                var ipedsRecordsIds = (ipedsInstitutionsResponseData != null) ? ipedsInstitutionsResponseData.Select(i => i.IiOpeId).ToArray() : new string[0];
                dataReaderMock.Setup(d => d.SelectAsync("IPEDS.INSTITUTIONS", "")).ReturnsAsync(ipedsRecordsIds);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<IpedsInstitutions>(It.IsAny<string[]>(), true)).ReturnsAsync(ipedsInstitutionsResponseData);
                
                return new IpedsInstitutionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private Collection<IpedsInstitutions> BuildIpedsDbDataContracts(List<TestIpedsInstitutionRepository.IpedsDbDataItem> ipedsDbDataList)
            {
                var ipedsCollection = new Collection<IpedsInstitutions>();
                foreach (var ipedsDbDataItem in ipedsDbDataList)
                {
                    var ipedsDataContract = new IpedsInstitutions()
                    {
                        Recordkey = ipedsDbDataItem.id,
                        IiInstitutionName = ipedsDbDataItem.name,
                        IiUnitId = ipedsDbDataItem.unitId,
                        IiOpeId = ipedsDbDataItem.opeId,
                        IiIpedsLastModified = ipedsDbDataItem.lastModifiedDate
                    };
                    ipedsCollection.Add(ipedsDataContract);
                }
                return ipedsCollection;
            }

        }
    }
}
