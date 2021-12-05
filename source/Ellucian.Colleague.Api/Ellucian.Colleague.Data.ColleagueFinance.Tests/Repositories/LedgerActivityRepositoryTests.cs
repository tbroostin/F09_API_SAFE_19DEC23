using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class LedgerActivityRepositoryTests_V11
    {
        [TestClass]
        public class LedgerActivityRepositoryTests_GET : BaseRepositorySetup
        {
            #region DECLARATIONS

            private LedgerActivityRepository ledgerActivityRepository;

            private Dictionary<string, RecordKeyLookupResult> lookupResult;
            private Dictionary<string, GuidLookupResult> dicResult;

            private Base.DataContracts.Defaults defaults;
            private Base.DataContracts.Corp corp;

            private Collection<GlaFyr> glaDataContracts;
            private Collection<GlAccts> glAccts;
            private Collection<Projects> projects;

            private ApplValcodes applValcodes;

            private IntlParams intlParams;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                ledgerActivityRepository = new LedgerActivityRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                glAccts = new Collection<GlAccts>() { new GlAccts() { Recordkey = "1", RecordGuid = guid } };

                projects = new Collection<Projects>() { new Projects() { Recordkey = "2", RecordGuid = guid } };

                intlParams = new IntlParams()
                {
                    HostCountry = "USA",
                    HostShortDateFormat = "DMY",
                    HostDateDelimiter = "-"
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "GLA.2020", PrimaryKey = "1" } }
                };

                lookupResult = new Dictionary<string, RecordKeyLookupResult>()
                {
                    { string.Join("+", new string[] { "PERSON", guid }), new RecordKeyLookupResult() { Guid = guid } }
                };

                defaults = new Base.DataContracts.Defaults() { DefaultHostCorpId = "1" };

                corp = new Base.DataContracts.Corp() { CorpName = new List<string>() { "Name" } };

                applValcodes = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "1" , ValActionCode2AssocMember = "2"}
                    }
                };

                glaDataContracts = new Collection<GlaFyr>()
                {
                    new GlaFyr()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                        Recordkey = "1",
                        GlaDescription = "desc",
                        GlaSysDate = DateTime.Today,
                        GlaTrDate = DateTime.Today,
                        GlaDebit = 1000,
                        GlaCredit = 2000,
                        GlaSource = "1",
                        GlaRefNo = "1",
                        GlaAcctId = guid
                    },
                    new GlaFyr()
                    {
                        RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e875",
                        Recordkey = "2",
                        GlaDescription = "desc",
                        GlaSysDate = DateTime.Today,
                        GlaTrDate = DateTime.Today,
                        GlaDebit = 1000,
                        GlaCredit = 2000,
                        GlaSource = "1",
                        GlaRefNo = "1",
                    }
                };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(Task.FromResult(defaults));
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(corp);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1", "2" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(glaDataContracts);
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(applValcodes);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(lookupResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(glaDataContracts.FirstOrDefault());
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<GlAccts>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(glAccts);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Projects>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(projects);

                List<string> ids = new List<string>() { "1", "2" };
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllLedgerActivities",
                    Entity = "",
                    Sublist = ids,
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
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task LedgerActivityRepository_GetGlaFyrAsync_Empty_FiscalYear()
            {
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, null, "", "", "", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task LedgerActivityRepository_GetGlaFyrAsync_Person_NotFound_For_DefaultCorpId()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(Task.FromResult<Base.DataContracts.Defaults>(null));
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(new Base.DataContracts.Corp() { });
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task LedgerActivityRepository_GetGlaFyrAsync_Empty_DefaultCropNames_From_Repository()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(new Base.DataContracts.Corp() { CorpName = new List<string>() { } });
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name1", "");
            }

            [TestMethod]
             public async Task LedgerActivityRepository_GetGlaFyrAsync_Invalid_ReportingSegment()
            {
                var result = await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name1", "");
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task LedgerActivityRepository_BuildGlaSourceCode_Empty_GlaSource_From_Repository()
            {
                glaDataContracts.FirstOrDefault().GlaSource = null;
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task LedgerActivityRepository_GetGlSourceCodesAsync_Throws_Exception()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ThrowsAsync(new Exception());
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task LedgerActivityRepository_GetGlSourceCodesAsync_GlSourceCodes_Null_From_Repository()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task LedgerActivityRepository_BuildGlaSourceCode_GlSourceCode_NotFound()
            {
                glaDataContracts.FirstOrDefault().GlaSource = "2";
                await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetGlaFyrAsync()
            {
                 var result = await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetGlaFyrAsync_Empty_Result_With_TransactionDate_Filter()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<GlaFyr>() { });

                var result = await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", "", "", "Name", "2017-08-29");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetGlaFyrAsync_With_FiscalPeriod_Filter()
            {
                var result = await ledgerActivityRepository.GetGlaFyrAsync(0, 2, "2017", DateTime.Today.Month.ToString(), DateTime.Today.Year.ToString(), "Name", "");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync_Empty_Guid()
            {
                await ledgerActivityRepository.GetGlaFyrByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync_Record_NotFound()
            {
                dicResult[guid] = null;
                await ledgerActivityRepository.GetGlaFyrByIdAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync_InvaidEntity()
            {
                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "INVALID", PrimaryKey = "1" }
                 }
                };

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);

                await ledgerActivityRepository.GetGlaFyrByIdAsync(guid);
            }
        
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync_ReadRecordAsync_Returns_Null()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await ledgerActivityRepository.GetGlaFyrByIdAsync(guid);
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync()
            {
                var result = await ledgerActivityRepository.GetGlaFyrByIdAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.RecordGuid, guid);
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetGlaFyrByIdAsync_Without_GlaAccountId()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<GlaFyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(glaDataContracts.LastOrDefault());

                var result = await ledgerActivityRepository.GetGlaFyrByIdAsync("1a49eed8-5fe7-4120-b1cf-f23266b9e875");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.RecordGuid, "1a49eed8-5fe7-4120-b1cf-f23266b9e875");
            }

            [TestMethod]
            public async Task LedgerActivityRepository_GetUnidataFormattedDate()
            {
                var result = await ledgerActivityRepository.GetUnidataFormattedDate(DateTime.Today.ToString());

                Assert.IsNotNull(result);
                Assert.AreEqual(result, DateTime.Today.ToString("dd-MM-yyyy"));
                
            }
        }
    }
}
