using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class PaymentTransactionsRepositoryTests
    {
        [TestClass]
        public class PaymentTransactionsRepositoryTests_V12
        {
            [TestClass]
            public class PaymentTransactionsRepositoryTests_GET_GETALL : BaseRepositorySetup
            {
                #region DECLARATION

                private Collection<Vouchers> vouchers;
                private Collection<Checks> checks;
                private Collection<Person> persons;
                private Collection<Items> items;

                private ArPayments arPayments;
                private PaymentMethods paymentMethods;
                private ArDepositItems arDepositItems;
                private Address address;
                private IntlParams intlParams;

                private Dictionary<string, GuidLookupResult> voucherCheckIds;

                private PaymentTransactionsRepository paymentTransactionsRepository;
                private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e000";
                string[] ids = new string[] { "1", "2", "3", "4" };

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    //baseColleagueRepository = new Mock<BaseColleagueRepository>();

                    MockInitialize();

                    InitializeTestData();

                    InitializeTestMock();

                    paymentTransactionsRepository = new PaymentTransactionsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    MockCleanup();

                    paymentTransactionsRepository = null;

                    //baseColleagueRepository = null;
                }
                private void InitializeTestData()
                {
                    vouchers = new Collection<Vouchers>() {
                        new Vouchers() { Recordkey = "1", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e000", VouMiscName = new List<string>() { "Misc_1","Misc_2" }, VouEcommerceSession="Session_1", VouEcommerceTransNo="Trans_001", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "P", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="1",VouTotalAmt=10, VouCurrencyCode = "CAD", VouItemsId = new List<string>(){"item1"}, VouPoNo = "PO1", VouApType = "AP"},
                        new Vouchers() { Recordkey = "2", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e001", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession = "Session_2",  VouEcommerceTransNo="Trans_002" , VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="2", VouArPayment = "1", VouTotalAmt=10, VouCurrencyCode = "EUR" , VouItemsId = new List<string>(){"item2"}, VouBpoId = "BPO1", VouApType = "AP" },
                        new Vouchers() { Recordkey = "3", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e002", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession="Session_3",  VouEcommerceTransNo="Trans_003", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="3", VouArDepositItems = new List<string>(){"1","2"}, VouTotalAmt=0 , VouItemsId = new List<string>(){"item3"}, VouRcvsId = "RECVOU1", VouApType = "AP"},
                        new Vouchers() { Recordkey = "4", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e003", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession="Session_4" ,  VouEcommerceTransNo="Trans_004", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "K", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="4", VouAltFlag="Y", VouApType = "AP" } };

                    checks = new Collection<Checks>() {
                        new Checks() { Recordkey = "1", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e000", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="1", ChkVouchersIds = new List<string>(){"1","2"} },
                        new Checks() { Recordkey = "2", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e001", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="2", ChkVouchersIds = new List<string>(){"1","2"} },
                        new Checks() { Recordkey = "3", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e002", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="3", ChkVouchersIds = new List<string>(){"1","2"} },
                        new Checks() { Recordkey = "4", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e003", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="P", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="4", ChkVouchersIds = new List<string>(){"1","2"} },

                        new Checks() { Recordkey = "4", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e003", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="4", ChkVouchersIds = new List<string>(){"1","2"} },
                        new Checks() { Recordkey = "4", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e003", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="4", ChkVouchersIds = new List<string>(){"1","2"} },
                        new Checks() { Recordkey = "4", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e003", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="4", ChkVouchersIds = new List<string>(){"1","2"} }
                    };

                    persons = new Collection<Person>() {
                        new Person() { Recordkey = "1", RecordGuid = "3a49eed8-5fe7-4120-b1cf-f23266b9e000", PersonCorpIndicator="Y", FirstName="Tom", LastName="Jerry" },
                        new Person() { Recordkey = "2", RecordGuid = "3a49eed8-5fe7-4120-b1cf-f23266b9e001", PreferredAddress = "1" },
                        new Person() { Recordkey = "3", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e002" },
                        new Person() { Recordkey = "4", RecordGuid = "3a49eed8-5fe7-4120-b1cf-f23266b9e003" } };

                    items = new Collection<Items>() {
                        new Items() { Recordkey = "item1", ItmDesc = new List<string>(){"item1 Desc" }, ItmVouExtPrice = 100},
                        new Items() { Recordkey = "item2", ItmDesc = new List<string>(){"item2 Desc" }, ItmVouExtPrice = 200 },
                        new Items() { Recordkey = "item3", ItmDesc = new List<string>(){"item3 Desc" }, ItmVouExtPrice = 300},
                        new Items() { Recordkey = "item4", ItmDesc = new List<string>(){"item4 Desc" }, ItmVouExtPrice = -100 } };


                    arPayments = new ArPayments() { Recordkey = "1", ArpAmt = 10, ArpArchive = "archive", ArpArType = "S", ArpCashRcpt = "Cash", ArpDate = DateTime.Now, ArpLocation = "location_001", ArpOrigPayMethod = "Cash", ArpPersonId = "1", ArpReversalAmt = 10, ArpReversedByPayment = "5", ArpTerm = "term" };
                    paymentMethods = new PaymentMethods() { Recordkey = "1", PmthCategory = "Category_001", PmthDescription = "desc", PmthEcommEnabledFlag = "flag" };
                    arDepositItems = new ArDepositItems() { Recordkey = "1", ArdiDate = DateTime.Now, ArdiDeposit = "dep_001", ArdiOrigPayMethod = "DebitCard" };
                    address = new Address() { Recordkey = "1", RecordGuid = "3a49eed8-5fe7-4120-b1cf-f23266b9e000" };
                    intlParams = new IntlParams() { Recordkey = "1", HostCountry = "USA", HostShortDateFormat = "MM/DD/YYYY" };

                    voucherCheckIds = new Dictionary<string, GuidLookupResult>() { {"1", new GuidLookupResult() { Entity= "CHECKS", PrimaryKey="1", SecondaryKey="1" } }, { "2", new GuidLookupResult() { Entity = "CHECKS", PrimaryKey = "2", SecondaryKey = "2" } }, { "3", new GuidLookupResult() { Entity = "CHECKS", PrimaryKey = "3", SecondaryKey = "3" } }, { "4", new GuidLookupResult() { Entity = "CHECKS", PrimaryKey = "4", SecondaryKey = "4" } } };
                }

                private void InitializeTestMock()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("CHECKS",It.IsAny<string>())).ReturnsAsync(new List<string>() {"1","2" }.ToArray<string>());
                    dataReaderMock.Setup(d => d.SelectAsync("VOUCHERS", It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "2" }.ToArray<string>());
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(),It.IsAny<bool>())).ReturnsAsync(vouchers);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Vouchers>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(vouchers);                    
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(checks);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(persons);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Items>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(items);
                    //dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new List<string>() { "9999eed8-5fe7-4120-b1cf-f23266b9e000", "9999eed8-5fe7-4120-b1cf-f23266b9e111" }.ToArray<string>());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ArPayments>(It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(arPayments);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<PaymentMethods>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(paymentMethods);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ArDepositItems>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(arDepositItems);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Address>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(address);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(voucherCheckIds);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Vouchers>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(vouchers.FirstOrDefault());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(persons.FirstOrDefault());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Checks>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(checks.FirstOrDefault());


                    dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "AP" });


                    var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var vou in vouchers)
                    {
                        recordLookupDict.Add("VOUCHERS+" + vou.Recordkey + "+" + vou.Recordkey,
                            new RecordKeyLookupResult() { Guid = vou.RecordGuid });
                    }

                    dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                    GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                    {
                        Offset = 0,
                        Limit = 1,
                        CacheName = "AllPaymentTransactionsFilter",
                        Entity = "",
                        Sublist = ids.ToList(),
                        TotalCount = ids.ToList().Count,                        
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

                #region GETALL

                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync()
                {
                    GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                    {
                        Offset = 0,
                        Limit = 1,
                        CacheName = "AllPaymentTransactionsFilter",
                        Entity = "",
                        Sublist = new List<string> { "1", "2" },
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
                    transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                    transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                        .ReturnsAsync(resp);
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Invoice, It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_With_Filters()
                {
                    GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                    {
                        Offset = 0,
                        Limit = 1,
                        CacheName = "AllPaymentTransactionsFilter",
                        Entity = "",
                        Sublist = new List<string> { "1", "2" },
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
                    List<string> refPoDoc = new List<string>() { "1", "2" }; 
                    transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
                    transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                        .ReturnsAsync(resp);
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Invoice, "1", refPoDoc, refPoDoc, refPoDoc);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Person_Null()
                {
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund, It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Null_Check()
                {
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Vouchers>());
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund, It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Guid_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(() => null);
                    dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund, It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                }
                
                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Invalid_Status()
                {
                    checks.FirstOrDefault().ChkStatEntityAssociation.FirstOrDefault().ChkStatusAssocMember = "A";
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(checks);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund, It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>());
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Guid_Null()
                {
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Dictionary_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Dictionary_Value_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1",null } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Dictionary_SecKey_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity="VOUCHERS", PrimaryKey="1", SecondaryKey="" } } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Dictionary_Key_Empty()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "CHECKS", PrimaryKey = "", SecondaryKey = "1" } } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Check_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Checks>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Person_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync()
                {
                    var result  = await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync("2a49eed8-5fe7-4120-b1cf-f23266b9e000");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Guid, "2a49eed8-5fe7-4120-b1cf-f23266b9e000");
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Voucher_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });

                    dataReaderMock.Setup(d => d.ReadRecordAsync<Vouchers>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_VPerson_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }
                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Voucher()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });
                     var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                    
                        recordLookupDict.Add("VOUCHERS+" + "1" + "+" + "1",
                            new RecordKeyLookupResult() { Guid = "9999eed8-5fe7-4120-b1cf-f23266b9e000" });
                   

                    dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync("9999eed8-5fe7-4120-b1cf-f23266b9e000");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Guid, "9999eed8-5fe7-4120-b1cf-f23266b9e000");
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Voucher_InvalidAPType()
                {
                    vouchers = new Collection<Vouchers>() {
                        new Vouchers() { Recordkey = "1", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e000", VouMiscName = new List<string>() { "Misc_1","Misc_2" }, VouEcommerceSession="Session_1", VouEcommerceTransNo="Trans_001", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "P", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="1",VouTotalAmt=10, VouCurrencyCode = "CAD", VouItemsId = new List<string>(){"item1"}, VouPoNo = "PO1", VouApType = "XX"}
                    };
                    dataReaderMock.Setup(repo => repo.SelectAsync("AP.TYPES", It.IsAny<string>())).ReturnsAsync(new string[] { "" });
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });
                    var recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                    recordLookupDict.Add("VOUCHERS+" + "1" + "+" + "1",
                        new RecordKeyLookupResult() { Guid = "9999eed8-5fe7-4120-b1cf-f23266b9e000" });


                    dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync("9999eed8-5fe7-4120-b1cf-f23266b9e000");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Guid, "9999eed8-5fe7-4120-b1cf-f23266b9e000");
                }

                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsIdFromGuidAsync() {
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsIdFromGuidAsync("2a49eed8-5fe7-4120-b1cf-f23266b9e000");
                    Assert.IsNotNull(result);
                }



                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPersonGuidsCollectionAsync_Valid()
                {
                    IEnumerable<string> sublist = new List<string>() { "1", "2" };
                    Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                    recordKeyLookupResults.Add("PURCHASE.ORDERS+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f" });
                    recordKeyLookupResults.Add("PURCHASE.ORDERS+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c" });
                    List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                    dataReaderMock.Setup(i => i.SelectAsync("PURCHASE.ORDERS", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
                    dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                    var results = await paymentTransactionsRepository.GetGuidsCollectionAsync(sublist, "PURCHASE.ORDERS");
                    Assert.IsNotNull(results);
                    Assert.AreEqual(2, results.Count());
                    foreach (var result in results)
                    {
                        RecordKeyLookupResult recordKeyLookupResult = null;
                        recordKeyLookupResults.TryGetValue(string.Concat("PURCHASE.ORDERS+", result.Key), out recordKeyLookupResult);

                        Assert.AreEqual(result.Value, recordKeyLookupResult.Guid);
                    }
                }



                //[TestMethod]
                //[ExpectedException(typeof(KeyNotFoundException))]
                //public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Voucher_Null()
                //{
                //    checks = new Collection<Checks>() {
                //        new Checks() { Recordkey = "1", RecordGuid = "2a49eed8-5fe7-4120-b1cf-f23266b9e000", ChkDate = DateTime.Now, ChkStatEntityAssociation = new List<ChecksChkStat>(){ new ChecksChkStat() { ChkStatusAssocMember="U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "U", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "N", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "O", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "R", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "V", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "X", ChkStatusDateAssocMember = DateTime.Now }, new ChecksChkStat() { ChkStatusAssocMember = "P", ChkStatusDateAssocMember = DateTime.Now } }, ChkVendor="1", ChkVouchersIds = new List<string>(){"6","7"} } };

                //    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(checks);

                //    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund);
                //}


                #endregion
            }

        }
    }
}
