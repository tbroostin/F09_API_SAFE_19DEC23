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
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayScalesRepositoryTests_V11
    {
        [TestClass]
        public class PayScalesRepositoryTests_GET : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PayScalesRepository payScalesRepository;

            private Dictionary<string, GuidLookupResult> dicResult;

            private string[] keys = new string[] { "1", "2" };

            private Collection<Swver> swvers;

            private Collection<Swtables> swtables;

            private Base.DataContracts.IntlParams intlParms;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                payScalesRepository = new PayScalesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                intlParms = new Base.DataContracts.IntlParams() { Recordkey = "1", HostCountry = "USA" };

                dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "SWVER", PrimaryKey = "1" } } };

                swvers = new Collection<Swver>()
                {
                    new Swver()
                    {
                        RecordGuid = guid,
                        Recordkey = "1",
                        SwvSwtablesId = "1",
                        SwvDesc = "desc",
                        SwvStartDate = DateTime.Today,
                        SwvEndDate = DateTime.Today.AddDays(100),
                        SwvWageEntityAssociation = new List<SwverSwvWage>()
                        {
                            new SwverSwvWage()
                            {
                                SwvWageAmountAssocMember = "100",
                                SwvWageGradeStepAssocMember = "1*1"
                            }
                        }
                    }
                };

                swtables = new Collection<Swtables>()
                {
                    new Swtables() { RecordGuid = guid, Recordkey = "1", SwtHrlyOrSlry = "H" }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(r => r.SelectAsync("SWVER", It.IsAny<string>())).ReturnsAsync(keys);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Swver>(It.IsAny<string[]>(), true)).ReturnsAsync(swvers);
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Swtables>(It.IsAny<string[]>(), true)).ReturnsAsync(swtables);
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swver>("SWVER", It.IsAny<string>(), true)).ReturnsAsync(swvers.FirstOrDefault());
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swtables>("SWTABLES", It.IsAny<string>(), true)).ReturnsAsync(swtables.FirstOrDefault());
                dataReaderMock.Setup(r => r.ReadRecordAsync<Base.DataContracts.IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(intlParms);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PayScalesRepository_GetPayScalesAsync_DataReader_Select_Exception()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWVER", It.IsAny<string>())).ThrowsAsync(new Exception());
                await payScalesRepository.GetPayScalesAsync();
            }

            [TestMethod]
            public async Task PayScalesRepository_GetPayScalesAsync_DataReader_Select_Gives_Null()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWVER", It.IsAny<string>())).ReturnsAsync(() => null);
                var result = await payScalesRepository.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task PayScalesRepository_GetPayScalesAsync_DataReader_Select_Gives_Empty()
            {
                dataReaderMock.Setup(r => r.SelectAsync("SWVER", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await payScalesRepository.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PayScalesRepository_GetPayScalesAsync_DataReader_BulkReadRecord_Exception()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Swver>(It.IsAny<string[]>(), true)).ThrowsAsync(new Exception());
                await payScalesRepository.GetPayScalesAsync();
            }

            [TestMethod]
            public async Task PayScalesRepository_GetPayScalesAsync()
            {
                var result = await payScalesRepository.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(guid, result.FirstOrDefault().Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesRepository_GetPayScalesByIdAsync_DataReader_RecordInfo_Entity_Null()
            {
                dicResult[guid].Entity = null;
                await payScalesRepository.GetPayScalesByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesRepository_GetPayScalesByIdAsync_DataReader_RecordInfo_Invalid_Entity()
            {
                dicResult[guid].Entity = "SWVERS";
                await payScalesRepository.GetPayScalesByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesRepository_GetPayScalesByIdAsync_DataReader_RecordInfo_PrimaryKey_Null()
            {
                dicResult[guid].PrimaryKey = null;
                await payScalesRepository.GetPayScalesByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesRepository_GetPayScalesByIdAsync_DataReader_PayScaleRecord_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swver>("SWVER", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await payScalesRepository.GetPayScalesByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesRepository_GetPayScalesByIdAsync_DataReader_PayTableRecord_Null()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<Swtables>("SWTABLES", It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await payScalesRepository.GetPayScalesByIdAsync(guid);
            }

            [TestMethod]
            public async Task PayScalesRepository_GetPayScalesByIdAsync()
            {
                swvers.FirstOrDefault().SwvWageEntityAssociation.FirstOrDefault().SwvWageAmountAssocMember = null;

                var result = await payScalesRepository.GetPayScalesByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Guid);
            }

            [TestMethod]
            public async Task PayScalesRepository_GetHostCountryAsync()
            {
                var result = await payScalesRepository.GetHostCountryAsync();

                //Assert.IsNotNull(result);
                //Assert.AreEqual("USA", result);
            }
        }
    }
}
