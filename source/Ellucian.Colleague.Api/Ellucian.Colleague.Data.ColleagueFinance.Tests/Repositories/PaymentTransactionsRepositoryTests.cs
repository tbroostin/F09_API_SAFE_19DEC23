using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
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

                private ArPayments arPayments;
                private PaymentMethods paymentMethods;
                private ArDepositItems arDepositItems;
                private Address address;
                private IntlParams intlParams;

                private Dictionary<string, GuidLookupResult> voucherCheckIds;

                private PaymentTransactionsRepository paymentTransactionsRepository;
                private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e000";


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
                        new Vouchers() { Recordkey = "1", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e000", VouMiscName = new List<string>() { "Misc_1","Misc_2" }, VouEcommerceSession="Session_1", VouEcommerceTransNo="Trans_001", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "P", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="1",VouTotalAmt=10, VouCurrencyCode = "CAD" },
                        new Vouchers() { Recordkey = "2", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e001", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession = "Session_2",  VouEcommerceTransNo="Trans_002" , VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="2", VouArPayment = "1", VouTotalAmt=10, VouCurrencyCode = "EUR"  },
                        new Vouchers() { Recordkey = "3", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e002", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession="Session_3",  VouEcommerceTransNo="Trans_003", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="3", VouArDepositItems = new List<string>(){"1","2"}, VouTotalAmt=0 },
                        new Vouchers() { Recordkey = "4", RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e003", VouMiscName = new List<string>() { "Misc_1", "Misc_2" }, VouEcommerceSession="Session_4" ,  VouEcommerceTransNo="Trans_004", VoucherStatusEntityAssociation = new List<VouchersVoucherStatus>(){ new VouchersVoucherStatus() { VouStatusAssocMember = "K", VouStatusDateAssocMember = DateTime.Now  }, new VouchersVoucherStatus() { VouStatusAssocMember = "R", VouStatusDateAssocMember = DateTime.Now }, new VouchersVoucherStatus() { VouStatusAssocMember = "V", VouStatusDateAssocMember = DateTime.Now } }, VouVendor="4", VouAltFlag="Y" }  };

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
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new List<string>() { "9999eed8-5fe7-4120-b1cf-f23266b9e000", "9999eed8-5fe7-4120-b1cf-f23266b9e111" }.ToArray<string>());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ArPayments>(It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(arPayments);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<PaymentMethods>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(paymentMethods);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ArDepositItems>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(arDepositItems);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Address>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(address);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(voucherCheckIds);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Vouchers>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(vouchers.FirstOrDefault());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(persons.FirstOrDefault());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Checks>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(checks.FirstOrDefault());
                    
                }

                #endregion

                #region GETALL

                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync()
                {
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Invoice);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Person_Null()
                {
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "2a49eed8-5fe7-4120-b1cf-f23266b9e000", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Null_Check()
                {
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Vouchers>("VOUCHERS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Vouchers>());
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Guid_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund);
                }
                
                [TestMethod]
                [ExpectedException(typeof(ApplicationException))]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsAsync_Invalid_Status()
                {
                    checks.FirstOrDefault().ChkStatEntityAssociation.FirstOrDefault().ChkStatusAssocMember = "A";
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Checks>("CHECKS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(checks);
                    await paymentTransactionsRepository.GetPaymentTransactionsAsync(0, 10, "", Domain.ColleagueFinance.Entities.InvoiceOrRefund.Refund);
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(ArgumentException))]
                public async Task GetPaymentTransactionsByGuidAsync_Guid_Null()
                {
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GetPaymentTransactionsByGuidAsync_Dictionary_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(ApplicationException))]
                public async Task GetPaymentTransactionsByGuidAsync_Dictionary_Application()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ThrowsAsync(new ApplicationException());
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GetPaymentTransactionsByGuidAsync_Dictionary_Value_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1",null } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GetPaymentTransactionsByGuidAsync_Dictionary_SecKey_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity="VOUCHERS", PrimaryKey="1", SecondaryKey="" } } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GetPaymentTransactionsByGuidAsync_Dictionary_Key_Empty()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "CHECKS", PrimaryKey = "", SecondaryKey = "1" } } });
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GetPaymentTransactionsByGuidAsync_Check_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Checks>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentException))]
                public async Task GetPaymentTransactionsByGuidAsync_Person_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
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
                public async Task GetPaymentTransactionsByGuidAsync_Voucher_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });

                    dataReaderMock.Setup(d => d.ReadRecordAsync<Vouchers>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentException))]
                public async Task GetPaymentTransactionsByGuidAsync_VPerson_Null()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                    await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                }
                [TestMethod]
                public async Task PaymentTransactionsRepository_GetPaymentTransactionsByGuidAsync_Voucher()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "VOUCHERS", PrimaryKey = "1", SecondaryKey = "1" } } });
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync("9999eed8-5fe7-4120-b1cf-f23266b9e000");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Guid, "9999eed8-5fe7-4120-b1cf-f23266b9e000");
                }

                [TestMethod]
                public async Task GetPaymentTransactionsIdFromGuidAsync() {
                    var result = await paymentTransactionsRepository.GetPaymentTransactionsIdFromGuidAsync("2a49eed8-5fe7-4120-b1cf-f23266b9e000");
                    Assert.IsNotNull(result);
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
