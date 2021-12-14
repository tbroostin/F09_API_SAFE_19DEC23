using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System.Threading;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayClassificationsRepositoryTests_V11
    {
        [TestClass]
        public class PayClassificationsRepositoryTests_GET : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PayClassificationsRepository payClassificationsRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private string[] keys = new string[] { "1", "2" };

            private Collection<Swtables> swtables;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                payClassificationsRepository = new PayClassificationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "SWTABLES", PrimaryKey = "1" } } };

                swtables = new Collection<Swtables>()
                {
                    new Swtables() { RecordGuid = guid, Recordkey = "1", SwtHrlyOrSlry = "H" }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("SWTABLES", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Swtables>(It.IsAny<string[]>(), true)).ReturnsAsync(swtables);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swtables>("SWTABLES", It.IsAny<string>(), true)).ReturnsAsync(swtables.FirstOrDefault());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PayClassificationsRepository_GetPayClassificationsAsync_DataReader_Select_Exception()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWTABLES", It.IsAny<string>())).ThrowsAsync(new Exception());
                await payClassificationsRepository.GetPayClassificationsAsync();
            }

            [TestMethod]
            public async Task PayClassificationsRepository_GetPayClassificationsAsync_DataReader_Select_Gives_Null()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWTABLES", It.IsAny<string>())).ReturnsAsync(() => null);
                var result = await payClassificationsRepository.GetPayClassificationsAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task PayClassificationsRepository_GetPayClassificationsAsync_DataReader_Select_Gives_Empty()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWTABLES", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await payClassificationsRepository.GetPayClassificationsAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PayClassificationsRepository_GetPayClassificationsAsync_DataReader_BulkReadRecord_Exception()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Swtables>(It.IsAny<string[]>(), true)).ThrowsAsync(new Exception());
                await payClassificationsRepository.GetPayClassificationsAsync();
            }

            [TestMethod]
            public async Task PayClassificationsRepository_GetPayClassificationsAsync()
            {
                var result = await payClassificationsRepository.GetPayClassificationsAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(guid, result.FirstOrDefault().Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync_DataReader_RecordInfo_Entity_Null()
            {
                dicResult[guid].Entity = null;
                await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync_DataReader_RecordInfo_Invalid_Entity()
            {
                dicResult[guid].Entity = "SWTABLESS";
                await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync_DataReader_RecordInfo_PrimaryKey_Null()
            {
                dicResult[guid].PrimaryKey = null;
                await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync_DataReader_PayScaleRecord_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swtables>("SWTABLES", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync_DataReader_PayTableRecord_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swtables>("SWTABLES", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);
            }

            [TestMethod]
            public async Task PayClassificationsRepository_GetPayClassificationsByIdAsync()
            {
                var result = await payClassificationsRepository.GetPayClassificationsByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Guid);
            }
        }
    }
}
