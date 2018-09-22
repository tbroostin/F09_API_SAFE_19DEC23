// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentPaymentRepositoryTests : BaseRepositorySetup
    {
        Mock<ICacheProvider> _cacheProvider;
        Mock<IColleagueTransactionFactory> _transactionFactory;
        Mock<ILogger> _logger;
        IStudentPaymentRepository _studentPaymentRepository;
        Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
        Mock<BaseColleagueRepository> _baseColleagueRepo;
       // Mock<IColleagueDataReader> dataReader;
        Mock<StudentPaymentRepository> _IStudentPaymentRepository;
        Mock<StudentPaymentRepository> StudentpaymentRepository;

        Collection<ArPayItemsIntg> studentPaymentsDataContracts;
        ArPayItemsIntg studentPaymentDataContract;
        string[] studentPaymentsIds = new[] { "234", "567" };
        string[] cashRcptsIds = new[] { "21" };
        string[] arInvoiceItemsIds = new[] { "223" };
        ArInvoiceItems arInvoiceItemDataContract;
        ArInvoices arInvoiceDataContract;
        CashRcpts cashRcptDataContract;
        Base.DataContracts.LdmDefaults ldmDefaultsDataContract;
        StudentPayment studentPayment;
        PostStudentPaymentsResponse updateResponse;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            _cacheProvider = new Mock<ICacheProvider>();
            _transactionFactory = new Mock<IColleagueTransactionFactory>();
            _logger = new Mock<ILogger>();
            _baseColleagueRepo = new Mock<BaseColleagueRepository>();
            dataReaderMock = new Mock<IColleagueDataReader>();
            iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);
            _IStudentPaymentRepository = new Mock<StudentPaymentRepository>();
            
            studentPaymentsDataContracts = new Collection<ArPayItemsIntg>();

            studentPaymentDataContract = new ArPayItemsIntg()
            {
                Recordkey = "234",
                RecordGuid = "22204000-222",
                ArpIntgArPaymentsIds = new List<string>() { "22222" },
                ArpIntgPersonId = "0000321",
                ArpIntgArType = "01",
                ArpIntgArCode = "TUI",
                ArpIntgTerm = "2016/FA",
                ArpIntgPaymentType = "sponsor",
                ArpIntgPaymentDate = new DateTime(2017, 01, 13),
                ArpIntgAmt = 10m,
                ArpIntgAmtCurrency = "USD",
                ArpIntgDistrMthd = "EVEL",
                ArpIntgComments = "This is a comment"
            };

            studentPaymentsDataContracts.Add(studentPaymentDataContract);

            studentPaymentDataContract = new ArPayItemsIntg()
            {
                Recordkey = "567",
                RecordGuid = "33304000-333",
                ArpIntgArPaymentsIds = new List<string>() { "33333" },
                ArpIntgPersonId = "0000322",
                ArpIntgArType = "01",
                ArpIntgArCode = "TUI",
                ArpIntgTerm = "2017/FA",
                ArpIntgPaymentType = "cash",
                ArpIntgPaymentDate = new DateTime(2017, 01, 15),
                ArpIntgAmt = 15m,
                ArpIntgAmtCurrency = "CAD",
                ArpIntgDistrMthd = "EVEL",
                ArpIntgComments = "This is a comment12345"
            };

            studentPaymentsDataContracts.Add(studentPaymentDataContract);

            studentPayment = new StudentPayment("0000322", "cash", new DateTime(2017, 01, 15))
            {
                AccountsReceivableCode = studentPaymentDataContract.ArpIntgArCode,
                AccountsReceivableTypeCode = studentPaymentDataContract.ArpIntgArType,
                PaymentAmount = studentPaymentDataContract.ArpIntgAmt,
                PaymentCurrency = studentPaymentDataContract.ArpIntgAmtCurrency,
                Comments = !string.IsNullOrEmpty(studentPaymentDataContract.ArpIntgComments) ? new List<string> { studentPaymentDataContract.ArpIntgComments } : null,
                Guid = "00000000-0000-0000-0000-000000000000",
                PaymentID = studentPaymentDataContract.ArpIntgArPaymentsIds.Any() ? studentPaymentDataContract.ArpIntgArPaymentsIds.ElementAt(0) : string.Empty,
                Term = studentPaymentDataContract.ArpIntgTerm,
                ChargeFromElevate = false,
                DistributionCode = "EVEL"
            };

            updateResponse = new PostStudentPaymentsResponse()
            {
                ArPayItemsIntgId = "576",
                ArpIntgGuid = "33304000-333"
            };

            cashRcptDataContract = new CashRcpts()
            {
                Recordkey = "1",
                RcptTenderGlDistrCode = "EVEL"
            };

            arInvoiceItemDataContract = new ArInvoiceItems()
            {
                Recordkey = "2",
                InviInvoice = "3"
            };

            arInvoiceDataContract = new ArInvoices()
            {
                Recordkey = "3"
            };

            ldmDefaultsDataContract = new Base.DataContracts.LdmDefaults()
            {
                Recordkey = "LDM.DEFAULTS",
                LdmdPaymentMethod = "EVEL",
                LdmdDefaultArType = "01"
            };

            _studentPaymentRepository = BuildValidPersonVisaRepository();

            dataReaderMock.Setup(m => m.SelectAsync("CASH.RCPTS", It.IsAny<string>())).ReturnsAsync(cashRcptsIds);
            dataReaderMock.Setup(i => i.ReadRecordAsync<CashRcpts>(cashRcptsIds[0], It.IsAny<bool>())).ReturnsAsync(cashRcptDataContract);

            dataReaderMock.Setup(m => m.SelectAsync("AR.INVOICES.ITEMS", It.IsAny<string>())).ReturnsAsync(arInvoiceItemsIds);
            dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvoiceItems>("2", It.IsAny<bool>())).ReturnsAsync(arInvoiceItemDataContract);
            dataReaderMock.Setup(i => i.ReadRecordAsync<ArInvoices>("3", It.IsAny<bool>())).ReturnsAsync(arInvoiceDataContract);

            dataReaderMock.Setup(i => i.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS",It.IsAny<bool>())).Returns(ldmDefaultsDataContract);
            

        }

        [TestCleanup]
        public void Cleanup()
        {
            _cacheProvider = null;
            _transactionFactory = null;
            _logger = null;
            _baseColleagueRepo = null;
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetByIdAsync()
        {
            
            dataReaderMock.Setup(i => i.ReadRecordAsync<ArPayItemsIntg>("567", It.IsAny<bool>())).ReturnsAsync(studentPaymentDataContract);

            var actual = await _studentPaymentRepository.GetByIdAsync("33304000-333");

            Assert.IsNotNull(actual);

            Assert.AreEqual(studentPaymentDataContract.ArpIntgPersonId, actual.PersonId);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentType, actual.PaymentType);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentDate, actual.PaymentDate);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArCode, actual.AccountsReceivableCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArType, actual.AccountsReceivableTypeCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmt, actual.PaymentAmount);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmtCurrency, actual.PaymentCurrency);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgComments, actual.Comments[0]);
            Assert.AreEqual(studentPaymentDataContract.RecordGuid, actual.Guid);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgTerm, actual.Term);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentPaymentRepoTest_GetByIdAsync_BadGuid()
        {
            var actual = await _studentPaymentRepository.GetByIdAsync("33304003-333");
        }
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentPaymentRepoTest_GetByIdAsync_NullEntity()
        {

            var actual = await _studentPaymentRepository.GetByIdAsync("33304000-333");

        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync()
        {
            
            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG", 
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true);

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());

            foreach(var actual in actuals.Item1)
            {
                var expected = studentPaymentsDataContracts.FirstOrDefault(x => x.RecordGuid == actual.Guid);

                Assert.AreEqual(expected.ArpIntgPersonId, actual.PersonId);
                Assert.AreEqual(expected.ArpIntgPaymentType, actual.PaymentType);
                Assert.AreEqual(expected.ArpIntgPaymentDate, actual.PaymentDate);
                Assert.AreEqual(expected.ArpIntgArCode, actual.AccountsReceivableCode);
                Assert.AreEqual(expected.ArpIntgArType, actual.AccountsReceivableTypeCode);
                Assert.AreEqual(expected.ArpIntgAmt, actual.PaymentAmount);
                Assert.AreEqual(expected.ArpIntgAmtCurrency, actual.PaymentCurrency);
                Assert.AreEqual(expected.ArpIntgComments, actual.Comments[0]);
                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
                Assert.AreEqual(expected.ArpIntgTerm, actual.Term);
            }
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_FilterPersonId()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PERSON.ID = '123'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true,"123");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_FilterTerm()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.TERM = '2016/FA'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true, "","2016/FA" );

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_FilterArCode()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.AR.CODE = '01'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true,"","","01");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_FilterPaymentType()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PAYMENT.TYPE = 'sponsor'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true,"","","","sponsor");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_AllFilters()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PERSON.ID = '123' AND WITH ARP.INTG.TERM = '2016/FA' AND WITH ARP.INTG.AR.CODE = '01' AND WITH ARP.INTG.PAYMENT.TYPE = 'sponsor'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(0, 100, true, "123", "2016/FA", "01", "sponsor");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync_offset()
        {
            studentPaymentsDataContracts = new Collection<ArPayItemsIntg>();
            studentPaymentsDataContracts.Add(studentPaymentDataContract);
            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync(1, 1, true);

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentPaymentRepoTest_GetAsync_nullReturn()
        {
            studentPaymentsDataContracts = new Collection<ArPayItemsIntg>();
            studentPaymentsDataContracts.Add(studentPaymentDataContract);
            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);

            var actuals = await _studentPaymentRepository.GetAsync(1, 1, true);
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true);

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());

            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentsDataContracts.FirstOrDefault(x => x.RecordGuid == actual.Guid);

                Assert.AreEqual(expected.ArpIntgPersonId, actual.PersonId);
                Assert.AreEqual(expected.ArpIntgPaymentType, actual.PaymentType);
                Assert.AreEqual(expected.ArpIntgPaymentDate, actual.PaymentDate);
                Assert.AreEqual(expected.ArpIntgArCode, actual.AccountsReceivableCode);
                Assert.AreEqual(expected.ArpIntgArType, actual.AccountsReceivableTypeCode);
                Assert.AreEqual(expected.ArpIntgAmt, actual.PaymentAmount);
                Assert.AreEqual(expected.ArpIntgAmtCurrency, actual.PaymentCurrency);
                Assert.AreEqual(expected.ArpIntgComments, actual.Comments[0]);
                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
                Assert.AreEqual(expected.ArpIntgTerm, actual.Term);
            }
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_FilterPersonId()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PERSON.ID = '123'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true, "123");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_FilterTerm()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.TERM = '2016/FA'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true, "", "2016/FA");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_FilterArCode()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.AR.CODE = '01'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true, "", "", "01");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_FilterPaymentType()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PAYMENT.TYPE = 'sponsor'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true, "", "", "", "sponsor");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_AllFilters()
        {

            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "WITH ARP.INTG.PERSON.ID = '123' AND WITH ARP.INTG.TERM = '2016/FA' AND WITH ARP.INTG.DISTR.MTHD = 'WEBA' AND WITH ARP.INTG.PAYMENT.TYPE = 'sponsor' AND WITH ARP.INTG.AR.TYPE = 'AP'")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(0, 100, true, "123", "2016/FA", "WEBA", "sponsor", "AP");

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_GetAsync2_offset()
        {
            studentPaymentsDataContracts = new Collection<ArPayItemsIntg>();
            studentPaymentsDataContracts.Add(studentPaymentDataContract);
            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPaymentsDataContracts);

            var actuals = await _studentPaymentRepository.GetAsync2(1, 1, true);

            Assert.IsNotNull(actuals.Item1);
            Assert.AreEqual(studentPaymentsDataContracts.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentPaymentRepoTest_GetAsync2_nullReturn()
        {
            studentPaymentsDataContracts = new Collection<ArPayItemsIntg>();
            studentPaymentsDataContracts.Add(studentPaymentDataContract);
            dataReaderMock.Setup(x => x.SelectAsync("AR.PAY.ITEMS.INTG", "")).ReturnsAsync(studentPaymentsIds);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG",
                It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(null);

            var actuals = await _studentPaymentRepository.GetAsync2(1, 1, true);
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_CreateAsync()
        {
            iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentPaymentsRequest, 
                PostStudentPaymentsResponse>(It.IsAny<PostStudentPaymentsRequest>())).ReturnsAsync(updateResponse);

            dataReaderMock.Setup(i => i.ReadRecordAsync<ArPayItemsIntg>("567", It.IsAny<bool>())).ReturnsAsync(studentPaymentDataContract);

            var actual = await _studentPaymentRepository.CreateAsync(studentPayment);

            Assert.IsNotNull(actual);

            Assert.AreEqual(studentPaymentDataContract.ArpIntgPersonId, actual.PersonId);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentType, actual.PaymentType);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentDate, actual.PaymentDate);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArCode, actual.AccountsReceivableCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArType, actual.AccountsReceivableTypeCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmt, actual.PaymentAmount);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmtCurrency, actual.PaymentCurrency);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgComments, actual.Comments[0]);
            Assert.AreEqual(studentPaymentDataContract.RecordGuid, actual.Guid);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgTerm, actual.Term);
            
        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_CreateAsync2()
        {
            iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentPaymentsRequest,
                PostStudentPaymentsResponse>(It.IsAny<PostStudentPaymentsRequest>())).ReturnsAsync(updateResponse);
            dataReaderMock.Setup(i => i.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true))
                .Returns(new LdmDefaults() { LdmdDefaultArCodes = new List<string>() { "WEBA" } });
            dataReaderMock.Setup(i => i.ReadRecordAsync<ArPayItemsIntg>("567", It.IsAny<bool>())).ReturnsAsync(studentPaymentDataContract);

            var actual = await _studentPaymentRepository.CreateAsync2(studentPayment);

            Assert.IsNotNull(actual);

            Assert.AreEqual(studentPaymentDataContract.ArpIntgPersonId, actual.PersonId);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentType, actual.PaymentType);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentDate, actual.PaymentDate);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArCode, actual.AccountsReceivableCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArType, actual.AccountsReceivableTypeCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmt, actual.PaymentAmount);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmtCurrency, actual.PaymentCurrency);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgComments, actual.Comments[0]);
            Assert.AreEqual(studentPaymentDataContract.RecordGuid, actual.Guid);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgTerm, actual.Term);

        }

        [TestMethod]
        public async Task StudentPaymentRepoTest_CreateAsync2_fromElevate()
        {
            studentPayment.ChargeFromElevate = true;
            iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentPaymentsRequest,
                PostStudentPaymentsResponse>(It.IsAny<PostStudentPaymentsRequest>())).ReturnsAsync(updateResponse);
            dataReaderMock.Setup(i => i.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true))
                .Returns(new LdmDefaults() { LdmdDefaultArCodes = new List<string>() { "WEBA" } });
            dataReaderMock.Setup(i => i.ReadRecordAsync<ArPayItemsIntg>("567", It.IsAny<bool>())).ReturnsAsync(studentPaymentDataContract);

            var actual = await _studentPaymentRepository.CreateAsync2(studentPayment);

            Assert.IsNotNull(actual);

            Assert.AreEqual(studentPaymentDataContract.ArpIntgPersonId, actual.PersonId);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentType, actual.PaymentType);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgPaymentDate, actual.PaymentDate);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArCode, actual.AccountsReceivableCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArType, actual.AccountsReceivableTypeCode);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmt, actual.PaymentAmount);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgAmtCurrency, actual.PaymentCurrency);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgComments, actual.Comments[0]);
            Assert.AreEqual(studentPaymentDataContract.RecordGuid, actual.Guid);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgArPaymentsIds.ElementAt(0), actual.PaymentID);
            Assert.AreEqual(studentPaymentDataContract.ArpIntgTerm, actual.Term);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentPaymentRepoTest_CreateAsync_RecordExistsError()
        {
            studentPayment.Guid = "33304000-333";
            var actual = await _studentPaymentRepository.CreateAsync(studentPayment);
            
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentPaymentRepoTest_CreateAsync_CTXerrors()
        {
            updateResponse.Error = "1";
            updateResponse.StudentPaymentErrors = new List<StudentPaymentErrors>()
            {
                new StudentPaymentErrors() {ErrorCodes = "123", ErrorMessages="testMsg1" },
                new StudentPaymentErrors() {ErrorCodes = "123", ErrorMessages="testMsg2" }
            };

            iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentPaymentsRequest,
                PostStudentPaymentsResponse>(It.IsAny<PostStudentPaymentsRequest>())).ReturnsAsync(updateResponse);

            var actual = await _studentPaymentRepository.CreateAsync(studentPayment);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StudentPaymentRepoTest_CreateAsync2_RecordExistsError()
        {
            studentPayment.Guid = "33304000-333";
            var actual = await _studentPaymentRepository.CreateAsync2(studentPayment);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentPaymentRepoTest_CreateAsync2_CTXerrors()
        {
            dataReaderMock.Setup(i => i.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true))
                .Returns(new LdmDefaults() { LdmdDefaultArCodes = new List<string>() { "WEBA" } });

            updateResponse.Error = "1";
            updateResponse.StudentPaymentErrors = new List<StudentPaymentErrors>()
            {
                new StudentPaymentErrors() {ErrorCodes = "123", ErrorMessages="testMsg1" },
                new StudentPaymentErrors() {ErrorCodes = "123", ErrorMessages="testMsg2" }
            };

            iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<PostStudentPaymentsRequest,
                PostStudentPaymentsResponse>(It.IsAny<PostStudentPaymentsRequest>())).ReturnsAsync(updateResponse);

            var actual = await _studentPaymentRepository.CreateAsync2(studentPayment);
        }


        public IStudentPaymentRepository BuildValidPersonVisaRepository()
        {
            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(stuPlan =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var sp in stuPlan)
                {
                    var rel = studentPaymentsDataContracts.FirstOrDefault(x => x.RecordGuid == sp.Guid);
                    result.Add(sp.Guid, rel == null ? null : new GuidLookupResult() { Entity = "AR.PAY.ITEMS.INTG", PrimaryKey = rel.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Build  repository
            _studentPaymentRepository = new StudentPaymentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return _studentPaymentRepository;
        }
    }
}
