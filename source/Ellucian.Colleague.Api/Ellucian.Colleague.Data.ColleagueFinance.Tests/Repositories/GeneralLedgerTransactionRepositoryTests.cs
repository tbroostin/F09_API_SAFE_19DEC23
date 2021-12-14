// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Web.Http.Configuration;
//using Ellucian.Colleague.Coordination.ColleagueFinance.Services;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class GeneralLedgerTransactionRepositoryTests : BaseRepositorySetup
    {

        private Mock<ICacheProvider> _iCacheProviderMock;
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;
        private ApiSettings apiSettings;

        private GeneralLedgerTransactionRepository _generalLedgerTransactionsRepository;
        private GeneralLedgerTransaction _generalLedgerTransaction;
        private Dictionary<string, GuidLookupResult> _guidLookupResults;

        private Ellucian.Colleague.Data.Base.DataContracts.Person _personContract;
        private CreateGLPostingRequest _createGlPostingRequest;
        private CreateGLPostingResponse _createGlPostingResponse;
        private TestGeneralLedgerConfigurationRepository testGlConfigurationRepository;

        private string _id = string.Empty;
        private string _recKey = string.Empty;
        private const string AccountNumber = "01-02-03-04-05550-66077";
        private const string ReferenceNumber = "GL122312321";
        private const string ProjectId = "A1";

        public static string FUND_CODE = "FUND";
        public static string SOURCE_CODE = "SOURCE";
        public static string LOCATION_CODE = "LOCATION";
        public static string LOCATION_SUBCLASS_CODE = "LOCATION_SUBCLASS";
        public static string FUNCTION_CODE = "FUNCTION";
        public static string UNIT_CODE = "UNIT";
        public static string UNIT_SUBCLASS_CODE = "UNIT_SUBCLASS";
        public static string OBJECT_CODE = "OBJECT";
        public static string GL_SUBSCLASS_CODE = "GL_SUBCLASS";
        [TestInitialize]
        public void Initialize()
        {
            _iCacheProviderMock = new Mock<ICacheProvider>();
            _iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            _iLoggerMock = new Mock<ILogger>();
            _dataReaderMock = new Mock<IColleagueDataReader>();
            _iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader())
                .Returns(_dataReaderMock.Object);
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker())
                .Returns(_iColleagueTransactionInvokerMock.Object);
            apiSettings = new ApiSettings("TEST");

            BuildObjects();

            _generalLedgerTransactionsRepository = new GeneralLedgerTransactionRepository(
                _iCacheProviderMock.Object,
                _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _generalLedgerTransactionsRepository = null;
            _generalLedgerTransaction = null;
            _guidLookupResults = null;

            _personContract = null;
            _createGlPostingRequest = null;
            _id = string.Empty;
            _recKey = string.Empty;
        }
  
        [TestMethod]
        public async Task GeneralLedgerTransactionRepo_GeneralLedgerTransaction_GetById()
        {
            BuildDataAndMocks();

            var result =
                await _generalLedgerTransactionsRepository.GetByIdAsync(_recKey, "0002024", GlAccessLevel.Full_Access);
            Assert.IsNotNull(result);
            Assert.AreEqual("Update", result.ProcessMode);
            Assert.AreEqual("123456", result.SubmittedBy);


            Assert.IsNotNull(result.GeneralLedgerTransactions);

            var generalLedgerTransaction =
                result.GeneralLedgerTransactions.FirstOrDefault(glt => glt.ReferenceNumber == "12345");
            Assert.IsNotNull(generalLedgerTransaction);
            Assert.AreEqual("12345", generalLedgerTransaction.ReferenceNumber);
            Assert.AreEqual("0002024", generalLedgerTransaction.ReferencePersonId);
            Assert.AreEqual("PL", generalLedgerTransaction.Source);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.LedgerDate.Date);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.TransactionTypeReferenceDate.Value.Date);

            var generalLedgerTransactionDetail =
                generalLedgerTransaction.TransactionDetailLines.FirstOrDefault(detail => detail.SequenceNumber == 1);
            Assert.IsNotNull(generalLedgerTransactionDetail);
            Assert.AreEqual("19", generalLedgerTransactionDetail.ProjectId);
            Assert.AreEqual("123456", generalLedgerTransactionDetail.SubmittedBy);
            Assert.AreEqual(CreditOrDebit.Credit, generalLedgerTransactionDetail.Type);

            var generalLedgerTransactionDetailAcct = generalLedgerTransactionDetail.GlAccount;
            Assert.IsNotNull(generalLedgerTransactionDetailAcct);
            Assert.AreEqual("desc", generalLedgerTransactionDetailAcct.GlAccountDescription);
            Assert.AreEqual("123456", generalLedgerTransactionDetailAcct.GlAccountNumber);

        }

        private void BuildDataAndMocks()
        {

            _recKey = "0012297";
            _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "INTG.GL.POSTINGS",
                    new GuidLookupResult() {Entity = "INTG.GL.POSTINGS", PrimaryKey = _recKey, SecondaryKey = ""}
                }
            }; // 

            var intgGlPostingsTranDetail = new IntgGlPostingsTranDetail()
            {
                IgpAcctIdAssocMember = "0002024",
                IgpRefNoAssocMember = "12345",
                IgpSourceAssocMember = "PL",
                IgpSysDateAssocMember = DateTime.Now.Date,
                IgpTrDateAssocMember = DateTime.Now.Date,
                IgpTranDetailsAssocMember = "19",
                IgpTranNoAssocMember = "123DHJGSHJGSDJHGSDJKGHSDJHSDGH"
            };

            var intgGlPosting = new IntgGlPostings();
            intgGlPosting.IgpAcctId = new List<string>() { "0002024" };
            intgGlPosting.IgpRefNo = new List<string>() { "12345" };
            intgGlPosting.IgpSource = new List<string>() { "PL" };
            intgGlPosting.IgpSysDate = new List<DateTime?>() { DateTime.Now.Date };
            intgGlPosting.Recordkey = _recKey;
            intgGlPosting.RecordGuid = "D79063C0-3793-455E-A244-1381DD1BC0C4";
            intgGlPosting.RecordModelName = "general-ledger-transactions";
            intgGlPosting.IgpTrDate = new List<DateTime?>() { DateTime.Now.Date };
            intgGlPosting.IgpSubmittedBy = "123456";
            intgGlPosting.TranDetailEntityAssociation = new List<IntgGlPostingsTranDetail>()
            {
                intgGlPostingsTranDetail
            };
            intgGlPosting.IgpTranNo = new List<string>() { "123DHJGSHJGSDJHGSDJKGHSDJHSDGH" };
            intgGlPosting.IgpTranDetails = new List<string>() { "19" };
            var intgGlPostings = new Collection<IntgGlPostings>() { intgGlPosting };

            decimal? credit = 25;
            decimal? debit = 65;

            var intgGlPostingsDetailIgpdTranDetails = new IntgGlPostingsDetailIgpdTranDetails()
            {
                IgpdCreditAssocMember = credit,
                IgpdDebitAssocMember = debit,
                IgpdDescriptionAssocMember = "desc",
                IgpdGlNoAssocMember = "123456",
                IgpdPrjItemsIdsAssocMember = "19",
                IgpdProjectIdsAssocMember = "19",
                IgpdTranSeqNoAssocMember = "1",
                IgpdSubmittedByAssocMember = "123456"
            };

            var intgGlPostingsDetail = new IntgGlPostingsDetail()
            {
                Recordkey = "19",
                IgpdCredit = new List<decimal?>() { credit },
                IgpdSubmittedBy = new List<string>() { "123456" },
                IgpdDescription = new List<string>() { "desc" },
                IgpdGlNo = new List<string>() { "123456" },
                IgpdPrjItemsIds = new List<string>() { "19" },
                IgpdTranSeqNo = new List<string>() { "1" },
                IgpdTranDetailsEntityAssociation =
                    new List<IntgGlPostingsDetailIgpdTranDetails>() { intgGlPostingsDetailIgpdTranDetails }
            };


            var detailIds = intgGlPostings.SelectMany(igp => igp.IgpTranDetails).ToArray();

            _dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            _dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(intgGlPostings);

            _dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds, It.IsAny<bool>()))
                .ReturnsAsync(new Collection<IntgGlPostingsDetail>() { intgGlPostingsDetail });

            _dataReaderMock.Setup(x => x.ReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(intgGlPosting);
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionRepo_GetGeneralLedgerTransactionAsync()
        {
            var intgGlPostingsTranDetail = new IntgGlPostingsTranDetail()
            {
                IgpAcctIdAssocMember = "0002024",
                IgpRefNoAssocMember = "12345",
                IgpSourceAssocMember = "PL",
                IgpSysDateAssocMember = DateTime.Now.Date,
                IgpTrDateAssocMember = DateTime.Now.Date,
                IgpTranDetailsAssocMember = "19",
                IgpTranNoAssocMember = "123DHJGSHJGSDJHGSDJKGHSDJHSDGH"
            };

            var intgGlPosting = new IntgGlPostings();
            intgGlPosting.IgpAcctId = new List<string>() {"0002024"};
            intgGlPosting.IgpRefNo = new List<string>() {"12345"};
            intgGlPosting.IgpSource = new List<string>() {"PL"};
            intgGlPosting.IgpSysDate = new List<DateTime?>() {DateTime.Now.Date};
            intgGlPosting.Recordkey = _recKey;
            intgGlPosting.RecordGuid = "D79063C0-3793-455E-A244-1381DD1BC0C4";
            intgGlPosting.RecordModelName = "general-ledger-transactions";
            intgGlPosting.IgpTrDate = new List<DateTime?>() {DateTime.Now.Date};
            intgGlPosting.IgpSubmittedBy = "123456";
            intgGlPosting.TranDetailEntityAssociation = new List<IntgGlPostingsTranDetail>()
            {
                intgGlPostingsTranDetail
            };
            intgGlPosting.IgpTranNo = new List<string>() {"123DHJGSHJGSDJHGSDJKGHSDJHSDGH"};
            intgGlPosting.IgpTranDetails = new List<string>() {"19"};
            var intgGlPostings = new Collection<IntgGlPostings>() {intgGlPosting};

            decimal? credit = 25;
            decimal? debit = 65;

            var intgGlPostingsDetailIgpdTranDetails = new IntgGlPostingsDetailIgpdTranDetails()
            {
                IgpdCreditAssocMember = credit,
                IgpdDebitAssocMember = debit,
                IgpdDescriptionAssocMember = "desc",
                IgpdGlNoAssocMember = "123456",
                IgpdPrjItemsIdsAssocMember = "19",
                IgpdProjectIdsAssocMember = "19",
                IgpdTranSeqNoAssocMember = "1",
                IgpdSubmittedByAssocMember = "123456"
            };

            var intgGlPostingsDetail = new IntgGlPostingsDetail()
            {
                Recordkey = "19",
                IgpdCredit = new List<decimal?>() {credit},
                IgpdDescription = new List<string>() {"desc"},
                IgpdSubmittedBy = new List<string>() { "123456" },
                IgpdGlNo = new List<string>() {"123456"},
                IgpdPrjItemsIds = new List<string>() {"19"},
                IgpdTranSeqNo = new List<string>() {"1"},
                IgpdTranDetailsEntityAssociation =
                    new List<IntgGlPostingsDetailIgpdTranDetails>() {intgGlPostingsDetailIgpdTranDetails}
            };

            _dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(intgGlPostings);

            var detailIds = intgGlPostings.SelectMany(igp => igp.IgpTranDetails).ToArray();
            _dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostingsDetail>(detailIds, It.IsAny<bool>()))
                .ReturnsAsync(new Collection<IntgGlPostingsDetail>() {intgGlPostingsDetail});

            var results = await _generalLedgerTransactionsRepository.GetAsync("0002024", GlAccessLevel.Full_Access);
            Assert.IsNotNull(results);

            var result = results.FirstOrDefault(r => r.Id == "D79063C0-3793-455E-A244-1381DD1BC0C4");
            Assert.IsNotNull(result);
            Assert.AreEqual("Update", result.ProcessMode);
            Assert.AreEqual("123456", result.SubmittedBy);
            Assert.IsNotNull(result.GeneralLedgerTransactions);

            var generalLedgerTransaction =
                result.GeneralLedgerTransactions.FirstOrDefault(glt => glt.ReferenceNumber == "12345");
            Assert.IsNotNull(generalLedgerTransaction);
            Assert.AreEqual("12345", generalLedgerTransaction.ReferenceNumber);
            Assert.AreEqual("0002024", generalLedgerTransaction.ReferencePersonId);
            Assert.AreEqual("PL", generalLedgerTransaction.Source);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.LedgerDate.Date);
            Assert.AreEqual(DateTime.Now.Date, generalLedgerTransaction.TransactionTypeReferenceDate.Value.Date);

            var generalLedgerTransactionDetail =
                generalLedgerTransaction.TransactionDetailLines.FirstOrDefault(detail => detail.SequenceNumber == 1);
            Assert.IsNotNull(generalLedgerTransactionDetail);
            Assert.AreEqual("19", generalLedgerTransactionDetail.ProjectId);
            Assert.AreEqual("123456", generalLedgerTransactionDetail.SubmittedBy);
            Assert.AreEqual(CreditOrDebit.Credit, generalLedgerTransactionDetail.Type);

            var generalLedgerTransactionDetailAcct = generalLedgerTransactionDetail.GlAccount;
            Assert.IsNotNull(generalLedgerTransactionDetailAcct);
            Assert.AreEqual("desc", generalLedgerTransactionDetailAcct.GlAccountDescription);
            Assert.AreEqual("123456", generalLedgerTransactionDetailAcct.GlAccountNumber);

        }

        [TestMethod]
        public async Task GeneralLedgerTransactionRepo_UpdateGeneralLedgerTransactionAsync()
        {
            _recKey = "0012297";

            _createGlPostingRequest = new CreateGLPostingRequest()
            {
                PostingGuid = _id,
                Mode = "Update",
                SubmittedBy = "123456",
                TranAcctId = new List<string>() {"0002024"},
                TranDetAmt = new List<string>() {"25"},
                TranDetGl = new List<string>() {"30"},
                TranDetProj = new List<string>() {"A1"},
                TranDetDesc = new List<string>() {"Description"},
                TranDetSeqNo = new List<string>() {"1"},
                TranDetType = new List<string>() {"credit"},
                TranNo = new List<string>() {"454323"},
                TranRefDate = new List<DateTime?>() {DateTime.Now.Date},
                TranRefNo = new List<string>() {"12345"},
                TranTrDate = new List<DateTime?>() {DateTime.Now.Date},
                TranType = new List<string>() {"credit"},
                TranDetSubmittedBy = new List<string>() {"123456"}



            };
            _createGlPostingResponse = new CreateGLPostingResponse()
            {
                PostingGuid = _id,
                PostingId = _recKey
            };

            _iColleagueTransactionInvokerMock.Setup(
                i =>
                    i.ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(
                        It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(_createGlPostingResponse);

            _generalLedgerTransaction.Id = Guid.Empty.ToString();
            _generalLedgerTransaction.SubmittedBy = "123456";
            BuildDataAndMocks();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();

            var testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var result =
                await
                    _generalLedgerTransactionsRepository.CreateAsync(_generalLedgerTransaction, "0002024",
                        GlAccessLevel.Full_Access, testGlAccountStructure);

            Assert.IsNotNull(result);
            //Assert.AreEqual(_id, result.Id);
            //Assert.IsNotNull(result.GeneralLedgerTransactions);
            //Assert.AreEqual("123456", result.SubmittedBy);
            

            //var generalLedgerTran =
            //    result.GeneralLedgerTransactions.FirstOrDefault(glt => glt.ReferenceNumber == "GL45645");
            //Assert.IsNotNull(generalLedgerTran);
            //Assert.AreEqual("DN", generalLedgerTran.Source);

            //var generalLedgerTranDetail =
            //    generalLedgerTran.TransactionDetailLines.FirstOrDefault(gltd => gltd.ProjectId == "A1");
            //Assert.IsNotNull(generalLedgerTranDetail);
            //Assert.AreEqual(CreditOrDebit.Credit, generalLedgerTranDetail.Type);
            //Assert.AreEqual("123456", generalLedgerTranDetail.SubmittedBy);

            //var generalLedgerTranDetailGlAcct = generalLedgerTranDetail.GlAccount;
            //Assert.IsNotNull(generalLedgerTranDetailGlAcct);
            //Assert.AreEqual("DESC", generalLedgerTranDetailGlAcct.GlAccountDescription);
            //Assert.AreEqual("01-02-03-04-05550-66077", generalLedgerTranDetailGlAcct.GlAccountNumber);

        }

        [TestMethod]
        public async Task GeneralLedgerTransactionRepo_TestDifferentGLNoDelimiters()
        {
            _recKey = "0012297";

            _createGlPostingRequest = new CreateGLPostingRequest()
            {
                PostingGuid = _id,
                Mode = "Update",
                TranAcctId = new List<string>() { "0002024" },
                TranDetAmt = new List<string>() { "25" },
                TranDetGl = new List<string>() { "30" },
                TranDetProj = new List<string>() { "A1" },
                TranDetDesc = new List<string>() { "Description" },
                TranDetSeqNo = new List<string>() { "1" },
                TranDetType = new List<string>() { "credit" },
                TranNo = new List<string>() { "454323" },
                TranRefDate = new List<DateTime?>() { DateTime.Now.Date },
                TranRefNo = new List<string>() { "12345" },
                TranTrDate = new List<DateTime?>() { DateTime.Now.Date },
                TranType = new List<string>() { "credit" },
                TranDetSubmittedBy = new List<string>() {"123456"}


            };
            _createGlPostingResponse = new CreateGLPostingResponse()
            {
                PostingGuid = _id,
                PostingId = _recKey
            };

            _iColleagueTransactionInvokerMock.Setup(
                i =>
                    i.ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(
                        It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(_createGlPostingResponse);


            //use same as the init build but change the GL delimiter.
            var glTransaction = new GeneralLedgerTransaction { Id = "001234" };
            var generalLedgerTransactions = new List<GenLedgrTransaction>();

            var genLedgrTransaction = new GenLedgrTransaction("DN", DateTimeOffset.Now)
            {
                ReferenceNumber = "GL45645",
                TransactionNumber = "1"
            };
            var genLedgrTransactionDetails = new List<GenLedgrTransactionDetail>();
            var genLedgrTransactionDetail = new GenLedgrTransactionDetail("01_02_03_04_05550_66077", ProjectId, "DESC",
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";
            genLedgrTransactionDetails.Add(genLedgrTransactionDetail);
            genLedgrTransaction.TransactionDetailLines = genLedgrTransactionDetails;
            generalLedgerTransactions.Add(genLedgrTransaction);
            glTransaction.GeneralLedgerTransactions = generalLedgerTransactions;

            _generalLedgerTransaction = glTransaction;

            _generalLedgerTransaction.Id = Guid.Empty.ToString();

            BuildDataAndMocks();
            //_guidLookupResults = new Dictionary<string, GuidLookupResult>
            //{
            //    {
            //        "INTG.GL.POSTINGS",
            //        new GuidLookupResult() {Entity = "INTG.GL.POSTINGS", PrimaryKey = _recKey, SecondaryKey = ""}
            //    }
            //};
            //_dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();

            var testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var result =
                await
                    _generalLedgerTransactionsRepository.CreateAsync(_generalLedgerTransaction, "0002024",
                        GlAccessLevel.Full_Access, testGlAccountStructure);

            Assert.IsNotNull(result);
            //Assert.AreEqual(_id, result.Id);
            //Assert.IsNotNull(result.GeneralLedgerTransactions);

            //var generalLedgerTran =
            //    result.GeneralLedgerTransactions.FirstOrDefault(glt => glt.ReferenceNumber == "GL45645");
            //Assert.IsNotNull(generalLedgerTran);
            //Assert.AreEqual("DN", generalLedgerTran.Source);

            //var generalLedgerTranDetail =
            //    generalLedgerTran.TransactionDetailLines.FirstOrDefault(gltd => gltd.ProjectId == "A1");
            //Assert.IsNotNull(generalLedgerTranDetail);
            //Assert.AreEqual(CreditOrDebit.Credit, generalLedgerTranDetail.Type);

            //var generalLedgerTranDetailGlAcct = generalLedgerTranDetail.GlAccount;
            //Assert.IsNotNull(generalLedgerTranDetailGlAcct);
            //Assert.AreEqual("DESC", generalLedgerTranDetailGlAcct.GlAccountDescription);
            //Assert.AreEqual("01_02_03_04_05550_66077", generalLedgerTranDetailGlAcct.GlAccountNumber);

        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GeneralLedgerTransactionRepo_UpdateGeneralLedgerTransactionAsync_Invalid()
        {
            _recKey = "0012297";
            _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "INTG.GL.POSTINGS",
                    new GuidLookupResult() {Entity = "INTG.GL.POSTINGS", PrimaryKey = _recKey, SecondaryKey = ""}
                }
            };

            _dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(_guidLookupResults);

            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();

            var testGlAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            await _generalLedgerTransactionsRepository.CreateAsync(_generalLedgerTransaction, "0002024",
                GlAccessLevel.Full_Access, testGlAccountStructure);
        }


        private GeneralLedgerTransaction BuildGeneralLedgerTransaction()
        {
            var glTransaction = new GeneralLedgerTransaction { Id = "001234" };
            var generalLedgerTransactions = new List<GenLedgrTransaction>();

            var genLedgrTransaction = new GenLedgrTransaction("DN", DateTimeOffset.Now)
            {
                ReferenceNumber = "GL45645",
                TransactionNumber = "1"
            };
            var genLedgrTransactionDetails = new List<GenLedgrTransactionDetail>();
            var genLedgrTransactionDetail = new GenLedgrTransactionDetail(AccountNumber, ProjectId, "DESC",
                CreditOrDebit.Credit, new AmountAndCurrency(25, CurrencyCodes.USD));
            genLedgrTransactionDetail.SubmittedBy = "123456";
            genLedgrTransactionDetails.Add(genLedgrTransactionDetail);
            genLedgrTransaction.TransactionDetailLines = genLedgrTransactionDetails;
            generalLedgerTransactions.Add(genLedgrTransaction);
            glTransaction.GeneralLedgerTransactions = generalLedgerTransactions;
            return glTransaction;
        }
        private void BuildObjects()
        {
            _id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            _recKey = "0012297";
            _guidLookupResults = new Dictionary<string, GuidLookupResult>
            {
                {
                    "INTG.GL.POSTINGS",
                    new GuidLookupResult() {Entity = "INTG.GL.POSTINGS", PrimaryKey = "0012297", SecondaryKey = ""}
                }
            };

            _createGlPostingRequest = new CreateGLPostingRequest {};

            _generalLedgerTransaction = BuildGeneralLedgerTransaction();
        }
        
    }

    [TestClass]
    public class GeneralLedgerTransactionRepositoryTests_V12  {
        [TestClass]
        public class GeneralLedgerTransactionServiceTests_GET_POST_PUT : BaseRepositorySetup
        {

            #region DECLARATION

            private IEnumerable<GeneralLedgerTransaction> generalLedgerTransactions;
            private Collection<IntgGlPostings> glPostings;
            private Collection<IntgGlPostingsDetail> glPostingDetails;
            private List<GeneralLedgerComponent> glComponents;
         
            private CostCenterStructure costCenterStructure;
            private GeneralLedgerTransactionRepository generalLedgerTransactionRepository;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            private Dictionary<string, GuidLookupResult> lookUpResult;
            private GeneralLedgerAccountStructure generalLedgerAccountStructure;
            private CreateGLPostingResponse response;

            IDictionary<string, string> projectReferenceIds;

            private Collection<Projects> projects;

            public static string FUND_CODE = "FUND";
            public static string SOURCE_CODE = "SOURCE";
            public static string LOCATION_CODE = "LOCATION";
            public static string LOCATION_SUBCLASS_CODE = "LOCATION_SUBCLASS";
            public static string FUNCTION_CODE = "FUNCTION";
            public static string UNIT_CODE = "UNIT";
            public static string UNIT_SUBCLASS_CODE = "UNIT_SUBCLASS";
            public static string OBJECT_CODE = "OBJECT";
            public static string GL_SUBSCLASS_CODE = "GL_SUBCLASS";
            #endregion

            #region TEST SETUP
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                generalLedgerTransactionRepository = new GeneralLedgerTransactionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new ApiSettings("TEST"));
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                generalLedgerTransactionRepository = null;
            }

            private void InitializeTestData()
            {
                glPostings = new Collection<IntgGlPostings>() {
                    new IntgGlPostings(){ Recordkey="1", RecordGuid="3a46eef8-5fe7-4120-b1cf-f23266b9e001", IgpTranDetails = new List<string>(){"AA1","BB1","CC1" }, IgpComments="comment_1", IgpRefNo=new List<string>(){"0001","0002" }, IgpSubmittedBy="0004319", IgpSource = new List<string>(){"source_1", "source_2","source_3" }, IgpAcctId= new List<string>(){"acct_0001","acct_0002" }, IgpSysDate = new List<DateTime?>(){ DateTime.Now }, IgpTranNo = new List<string>(){ "Tran_0001", "Tran_0002" }, IgpTrDate = new List<DateTime?>(){ DateTime.Now }, TranDetailEntityAssociation = new List<IntgGlPostingsTranDetail>(){ new IntgGlPostingsTranDetail() { IgpAcctIdAssocMember="Amem_001", IgpRefNoAssocMember="Ref_AMem_001", IgpSourceAssocMember="SAMem_001", IgpSysDateAssocMember=DateTime.Now, IgpTranDetailsAssocMember="1", IgpTranNoAssocMember="Tran_AMem_001", IgpTrDateAssocMember = DateTime.Now } } },
                    new IntgGlPostings(){ Recordkey="2", RecordGuid="3a46eef8-5fe7-4120-b1cf-f23266b9e002", IgpTranDetails = new List<string>(){"AA2","BB2","CC2" }, IgpComments="comment_2", IgpRefNo=new List<string>(){"0003","0004" }, IgpSubmittedBy="0004310", IgpSource = new List<string>(){"source_4", "source_5","source_6" }, IgpAcctId= new List<string>(){"acct_0003","acct_0004" }, IgpSysDate = new List<DateTime?>(){ DateTime.Now }, IgpTranNo = new List<string>(){ "Tran_0003", "Tran_0004" }, IgpTrDate = new List<DateTime?>(){ DateTime.Now }, TranDetailEntityAssociation = new List<IntgGlPostingsTranDetail>(){ new IntgGlPostingsTranDetail() { IgpAcctIdAssocMember="Amem_002", IgpRefNoAssocMember="Ref_AMem_002", IgpSourceAssocMember="SAMem_002", IgpSysDateAssocMember=DateTime.Now, IgpTranDetailsAssocMember="2", IgpTranNoAssocMember="Tran_AMem_002", IgpTrDateAssocMember = DateTime.Now } } },
                };

                glPostingDetails = new Collection<IntgGlPostingsDetail>() {
                    new IntgGlPostingsDetail(){ Recordkey="1", IgpdCredit = new List<decimal?>(){10,20}, IgpdDebit = new List<decimal?>(){10,20 }, IgpdDescription = new List<string>(){"desc_1", "desc_2" }, IgpdEncAdjType = new List<string>(){ "adj_001" }, IgpdEncCommitmentType = new List<string>(){ "commit_1" }, IgpdEncLineItemNo = new List<string>(){ "ln_001", "ln_002" }, IgpdEncRefNo = new List<string>(){ "enc_ref_001", "encRef_002" }, IgpdEncSeqNo= new List<string>(){"seq_001", "seq_002" }, IgpdGiftUnits = new List<string>(){"gift_001", "gift_002" }, IgpdGlNo= new List<string>(){"Gl_001","Gl_002" }, IgpdPrjItemsIds= new List<string>(){"Item_001", "Item_002" }, IgpdProjectIds = new List<string>(){ "Proj_001", "Proj_002" }, IgpdSubmittedBy = new List<string>(){"0004319", "0004329" }, IgpdTranSeqNo = new List<string>(){"Tran_seq_001", "Tran_seq_002" }, IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>(){ new IntgGlPostingsDetailIgpdTranDetails() { IgpdCreditAssocMember = 10, IgpdDebitAssocMember = 10, IgpdDescriptionAssocMember="desc_1", IgpdEncAdjTypeAssocMember="enc_Adg_type_001", IgpdEncCommitmentTypeAssocMember="commitment_001", IgpdEncLineItemNoAssocMember="item_001", IgpdEncRefNoAssocMember="AMem_001", IgpdEncSeqNoAssocMember="1", IgpdGiftUnitsAssocMember ="gift_001", IgpdGlNoAssocMember="gl_001", IgpdPrjItemsIdsAssocMember ="proj_items_001", IgpdProjectIdsAssocMember="proj_001", IgpdSubmittedByAssocMember="0004319", IgpdTranSeqNoAssocMember="1"  } } },
                    new IntgGlPostingsDetail(){ Recordkey="2", IgpdCredit = new List<decimal?>(){10,20}, IgpdDebit = new List<decimal?>(){10,20 }, IgpdDescription = new List<string>(){"desc_1", "desc_2" }, IgpdEncAdjType = new List<string>(){ "adj_001" }, IgpdEncCommitmentType = new List<string>(){ "commit_1" }, IgpdEncLineItemNo = new List<string>(){ "ln_001", "ln_002" }, IgpdEncRefNo = new List<string>(){ "enc_ref_001", "encRef_002" }, IgpdEncSeqNo= new List<string>(){"seq_001", "seq_002" }, IgpdGiftUnits = new List<string>(){"gift_001", "gift_002" }, IgpdGlNo= new List<string>(){"Gl_001","Gl_002" }, IgpdPrjItemsIds= new List<string>(){"Item_001", "Item_002" }, IgpdProjectIds = new List<string>(){ "Proj_001", "Proj_002" }, IgpdSubmittedBy = new List<string>(){"0004319", "0004329" }, IgpdTranSeqNo = new List<string>(){"Tran_seq_001", "Tran_seq_002" }, IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>(){ new IntgGlPostingsDetailIgpdTranDetails() { IgpdDebitAssocMember = 10, IgpdDescriptionAssocMember="desc_1", IgpdEncAdjTypeAssocMember="enc_Adg_type_001", IgpdEncCommitmentTypeAssocMember="commitment_001", IgpdEncLineItemNoAssocMember="item_001", IgpdEncRefNoAssocMember="AMem_001", IgpdEncSeqNoAssocMember="001", IgpdGiftUnitsAssocMember ="gift_001", IgpdGlNoAssocMember="gl_001", IgpdPrjItemsIdsAssocMember ="proj_items_001", IgpdProjectIdsAssocMember="proj_001", IgpdSubmittedByAssocMember="0004319", IgpdTranSeqNoAssocMember= "tran_seq_mem_001" } } },
                    new IntgGlPostingsDetail(){ Recordkey="1", IgpdCredit = new List<decimal?>(){10,20}, IgpdDebit = new List<decimal?>(){10,20 }, IgpdDescription = new List<string>(){"desc_1", "desc_2" }, IgpdEncAdjType = new List<string>(){ "adj_001" }, IgpdEncCommitmentType = new List<string>(){ "commit_1" }, IgpdEncLineItemNo = new List<string>(){ "ln_001", "ln_002" }, IgpdEncRefNo = new List<string>(){ "enc_ref_001", "encRef_002" }, IgpdEncSeqNo= new List<string>(){"seq_001", "seq_002" }, IgpdGiftUnits = new List<string>(){"gift_001", "gift_002" }, IgpdGlNo= new List<string>(){"Gl_001","Gl_002" }, IgpdPrjItemsIds= new List<string>(){"Item_001", "Item_002" }, IgpdProjectIds = new List<string>(){ "Proj_001", "Proj_002" }, IgpdSubmittedBy = new List<string>(){"0004319", "0004329" }, IgpdTranSeqNo = new List<string>(){"Tran_seq_001", "Tran_seq_002" }, IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>(){ new IntgGlPostingsDetailIgpdTranDetails() { IgpdDebitAssocMember = 0, IgpdDescriptionAssocMember="desc_1", IgpdEncAdjTypeAssocMember="enc_Adg_type_001", IgpdEncCommitmentTypeAssocMember="commitment_001", IgpdEncLineItemNoAssocMember="item_001", IgpdEncRefNoAssocMember="AMem_001", IgpdEncSeqNoAssocMember="seq_001", IgpdGiftUnitsAssocMember ="gift_001", IgpdGlNoAssocMember="gl_001", IgpdPrjItemsIdsAssocMember ="proj_items_001", IgpdProjectIdsAssocMember="proj_001", IgpdSubmittedByAssocMember="0004319", IgpdTranSeqNoAssocMember="tran_seq_mem_001"  } } },
                    new IntgGlPostingsDetail(){ Recordkey="2", IgpdCredit = new List<decimal?>(){10,20}, IgpdDebit = new List<decimal?>(){10,20 }, IgpdDescription = new List<string>(){"desc_1", "desc_2" }, IgpdEncAdjType = new List<string>(){ "adj_001" }, IgpdEncCommitmentType = new List<string>(){ "commit_1" }, IgpdEncLineItemNo = new List<string>(){ "ln_001", "ln_002" }, IgpdEncRefNo = new List<string>(){ "enc_ref_001", "encRef_002" }, IgpdEncSeqNo= new List<string>(){"seq_001", "seq_002" }, IgpdGiftUnits = new List<string>(){"gift_001", "gift_002" }, IgpdGlNo= new List<string>(){"Gl_001","Gl_002" }, IgpdPrjItemsIds= new List<string>(){"Item_001", "Item_002" }, IgpdProjectIds = new List<string>(){ "Proj_001", "Proj_002" }, IgpdSubmittedBy = new List<string>(){"0004319", "0004329" }, IgpdTranSeqNo = new List<string>(){"Tran_seq_001", "Tran_seq_002" }, IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>(){ new IntgGlPostingsDetailIgpdTranDetails() { IgpdDescriptionAssocMember="desc_1", IgpdEncAdjTypeAssocMember="enc_Adg_type_001", IgpdEncCommitmentTypeAssocMember="commitment_001", IgpdEncLineItemNoAssocMember="item_001", IgpdEncRefNoAssocMember="AMem_001", IgpdEncSeqNoAssocMember="seq_001", IgpdGiftUnitsAssocMember ="gift_001", IgpdGlNoAssocMember="gl_001", IgpdPrjItemsIdsAssocMember ="proj_items_001", IgpdProjectIdsAssocMember="proj_001", IgpdSubmittedByAssocMember="0004319", IgpdTranSeqNoAssocMember="tran_seq_mem_001"  } } },
                    new IntgGlPostingsDetail(){ Recordkey="1", IgpdCredit = new List<decimal?>(){10,20}, IgpdDebit = new List<decimal?>(){10,20 }, IgpdDescription = new List<string>(){"desc_1", "desc_2" }, IgpdEncAdjType = new List<string>(){ "adj_001" }, IgpdEncCommitmentType = new List<string>(){ "commit_1" }, IgpdEncLineItemNo = new List<string>(){ "ln_001", "ln_002" }, IgpdEncRefNo = new List<string>(){ "enc_ref_001", "encRef_002" }, IgpdEncSeqNo= new List<string>(){"seq_001", "seq_002" }, IgpdGiftUnits = new List<string>(){"gift_001", "gift_002" }, IgpdGlNo= new List<string>(){"Gl_001","Gl_002" }, IgpdPrjItemsIds= new List<string>(){"Item_001", "Item_002" }, IgpdProjectIds = new List<string>(){ "Proj_001", "Proj_002" }, IgpdSubmittedBy = new List<string>(){"0004319", "0004329" }, IgpdTranSeqNo = new List<string>(){"Tran_seq_001", "Tran_seq_002" }, IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>(){ new IntgGlPostingsDetailIgpdTranDetails() { IgpdCreditAssocMember = 0, IgpdDescriptionAssocMember="desc_1", IgpdEncAdjTypeAssocMember="enc_Adg_type_001", IgpdEncCommitmentTypeAssocMember="commitment_001", IgpdEncLineItemNoAssocMember="item_001", IgpdEncRefNoAssocMember="AMem_001", IgpdEncSeqNoAssocMember="seq_001", IgpdGiftUnitsAssocMember ="gift_001", IgpdGlNoAssocMember="gl_001", IgpdPrjItemsIdsAssocMember ="proj_items_001", IgpdProjectIdsAssocMember="proj_001", IgpdSubmittedByAssocMember="0004319", IgpdTranSeqNoAssocMember="tran_seq_mem_001"  } } }
                };


                generalLedgerAccountStructure = new GeneralLedgerAccountStructure()
                {
                  
                    AccountOverrideTokens = new List<string>() { "T1", "T2", "T3" },
                    CheckAvailableFunds = "Check",
                    FullAccessRole = "ALL-ACCOUNTS", glDelimiter = "-"
                };
                glComponents = new List<GeneralLedgerComponent>();
                //accountStructure.SetMajorComponentStartPositions(new List<string>() { "1", "4", "7", "10", "13", "19" });
                glComponents.Add(new GeneralLedgerComponent(FUND_CODE, true, GeneralLedgerComponentType.Fund, "1", "2"));
                glComponents.Add(new GeneralLedgerComponent(SOURCE_CODE, true, GeneralLedgerComponentType.Source, "4", "2"));
                glComponents.Add(new GeneralLedgerComponent(LOCATION_CODE, true, GeneralLedgerComponentType.Location, "7", "2"));
                glComponents.Add(new GeneralLedgerComponent(LOCATION_SUBCLASS_CODE, true, GeneralLedgerComponentType.Location, "7", "1"));
                glComponents.Add(new GeneralLedgerComponent(FUNCTION_CODE, true, GeneralLedgerComponentType.Function, "10", "2"));
                glComponents.Add(new GeneralLedgerComponent(UNIT_CODE, true, GeneralLedgerComponentType.Unit, "13", "5"));
                glComponents.Add(new GeneralLedgerComponent(UNIT_SUBCLASS_CODE, true, GeneralLedgerComponentType.Unit, "13", "3"));
                glComponents.Add(new GeneralLedgerComponent(OBJECT_CODE, true, GeneralLedgerComponentType.Object, "19", "5"));
                glComponents.Add(new GeneralLedgerComponent(GL_SUBSCLASS_CODE, true, GeneralLedgerComponentType.Object, "19", "2"));


                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == LOCATION_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUNCTION_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == OBJECT_CODE));

                costCenterStructure = new CostCenterStructure();
                costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE));
                costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE));
                costCenterStructure.AddCostCenterComponent(glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE));


                generalLedgerTransactions = new List<GeneralLedgerTransaction>() {
                    new GeneralLedgerTransaction() {Id = "00000000-0000-0000-0000-000000000000",  ProcessMode = "PM", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("source_1", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123890656342566190", "Proj1_001", "desc", CreditOrDebit.Credit, amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber="0001",EncAdjustmentType= "PARTIAL", EncCommitmentType="COMMITTED", EncSequenceNumber = 100, EncGiftUnits ="10", EncLineItemNumber="001", SequenceNumber=10 }  }   } } } ,
                    new GeneralLedgerTransaction() { Id = "", Comment = "", ProcessMode = "UPDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("DN", DateTime.Now) { BudgetPeriodDate = new DateTime(2007,10,20,10,50,20), ReferenceNumber = "ref_num1", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", CreditOrDebit.Debit, amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "ADJUSTMENT", EncCommitmentType = "UNCOMMITTED", EncGiftUnits = "" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "UPDATEIMMEDIATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("DNE", DateTime.Now) { BudgetPeriodDate = new DateTime(2016, 10, 20, 10, 50, 20), ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "TOTAL", EncCommitmentType="NONCOMMITTED", EncGiftUnits = "20.90", SubmittedBy= "0004319" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "UPDATEBATCH", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("PL", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "ref_num", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncRefNumber = "0001", EncAdjustmentType = "HALF", EncCommitmentType = "NONCOMMITTED" } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", ProcessMode = "VALIDATE", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("PLE", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferenceNumber = "", ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) { EncAdjustmentType = "", EncCommitmentType = "", EncSequenceNumber=100, SequenceNumber=10 } } } } },
                    new GeneralLedgerTransaction() { Id = "", Comment = "comment1", SubmittedBy = "Vaidya", GeneralLedgerTransactions = new List<GenLedgrTransaction>() { new GenLedgrTransaction("source_1", DateTime.Now) { BudgetPeriodDate = DateTime.Now, ReferencePersonId = "000011", TransactionNumber = "Tran_1", TransactionTypeReferenceDate = DateTimeOffset.Now, TransactionDetailLines = new List<GenLedgrTransactionDetail>() { new GenLedgrTransactionDetail("000123", "Proj1_001", "desc", type: new CreditOrDebit(), amount: new AmountAndCurrency(10, CurrencyCodes.USD)) } } } } };

                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { Entity = "INTG.GL.POSTINGS", PrimaryKey = "1", SecondaryKey = "1" } }, { "2", new GuidLookupResult { Entity = "INTG.GL.POSTINGS", PrimaryKey = "2", SecondaryKey = "2" } } };

                response = new CreateGLPostingResponse()
                {
                    PostingGuid = guid,
                    WarningMessages = new List<string>() { "Warning_Test_0001" },
                };
                projects = new Collection<Projects>() { new Projects() { Recordkey = "1", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e001" }, new Projects() { Recordkey = "2", RecordGuid = "3a46eef8-5fe7-4120-b1cf-f23266b9e002" } };

            }

            private void InitializeTestMock()
            {

                dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), true)).ReturnsAsync(glPostings);
                dataReaderMock.Setup(x => x.ReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), true)).ReturnsAsync(glPostings.FirstOrDefault());
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostingsDetail>(It.IsAny<string[]>(), true)).ReturnsAsync(glPostingDetails);
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(projects);
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(response);
            }

            #endregion

            #region GETALL

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_Get2Async()
            {

                var result = await generalLedgerTransactionRepository.Get2Async("0002024", GlAccessLevel.Full_Access);
                Assert.IsTrue(result.Count() == 2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GeneralLedgerTransactionRepository_Get2Async_GlDetail_KeyNotFoundException()
            {
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<IntgGlPostingsDetail>(It.IsAny<string[]>(), true)).ReturnsAsync(() => null);
                await generalLedgerTransactionRepository.Get2Async("0002024", GlAccessLevel.Full_Access);
            }
            #endregion

            #region GETBYID

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_GetById2Async()
            {
                var result = await generalLedgerTransactionRepository.GetById2Async(guid, "0002024", GlAccessLevel.Full_Access);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionRepository_GetById2Async_Id_As_Null()
            {
                await generalLedgerTransactionRepository.GetById2Async("", "0002024", GlAccessLevel.Full_Access);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GeneralLedgerTransactionRepository_GetById2Async_RecordInfo_null()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { };
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                await generalLedgerTransactionRepository.GetById2Async(guid, "0002024", GlAccessLevel.Full_Access);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GeneralLedgerTransactionRepository_GetById2Async_RecordInfo_PrimaryKey_Null()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { Entity = "INTG.GL.POSTINGS", PrimaryKey = "", SecondaryKey = "1" } } };
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                await generalLedgerTransactionRepository.GetById2Async(guid, "0002024", GlAccessLevel.Full_Access);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GeneralLedgerTransactionRepository_GetById2Async_RecordInfo_Entity_Wrong()
            {
                lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult { Entity = "GL.POSTINGS", PrimaryKey = "1", SecondaryKey = "1" } } };
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                await generalLedgerTransactionRepository.GetById2Async(guid, "0002024", GlAccessLevel.Full_Access);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GeneralLedgerTransactionRepository_GetById2Async_GlPosting_Null()
            {
                dataReaderMock.Setup(x => x.ReadRecordAsync<IntgGlPostings>(It.IsAny<string>(), true)).ReturnsAsync(() => null);
                await generalLedgerTransactionRepository.GetById2Async(guid, "0002024", GlAccessLevel.Full_Access);
            }

            #endregion

            #region POST

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_Create2Async()
            {
                var result = await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_Create2Async_PostingIdEmpty()
            {
                response = new CreateGLPostingResponse()
                {
                    PostingGuid = "",
                    WarningMessages = new List<string>() { "Warning_Test_0001" },
                };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(response);
                var result = await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GeneralLedgerTransactionRepository_Create2Async_MajorComponentsInvalid()
            {
                generalLedgerAccountStructure = new GeneralLedgerAccountStructure()
                {

                    AccountOverrideTokens = new List<string>() { "T1", "T2", "T3" },
                    CheckAvailableFunds = "Check",
                    FullAccessRole = "ALL-ACCOUNTS",
                    glDelimiter = "-"
                };
                glComponents = new List<GeneralLedgerComponent>();

                glComponents.Add(new GeneralLedgerComponent(FUND_CODE, true, GeneralLedgerComponentType.Fund, "1", "2"));
                glComponents.Add(new GeneralLedgerComponent(SOURCE_CODE, true, GeneralLedgerComponentType.Source, "4", "2"));
                glComponents.Add(new GeneralLedgerComponent(LOCATION_CODE, true, GeneralLedgerComponentType.Location, "7", "2"));
                glComponents.Add(new GeneralLedgerComponent(FUNCTION_CODE, true, GeneralLedgerComponentType.Function, "10", "2"));
                glComponents.Add(new GeneralLedgerComponent(UNIT_CODE, true, GeneralLedgerComponentType.Unit, "13", "5"));
                glComponents.Add(new GeneralLedgerComponent(OBJECT_CODE, true, GeneralLedgerComponentType.Object, "19", "5"));
                glComponents.Add(new GeneralLedgerComponent("ERROR", true, GeneralLedgerComponentType.Object, "25", "8"));

                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUND_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == SOURCE_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == LOCATION_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == FUNCTION_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == UNIT_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == OBJECT_CODE));
                generalLedgerAccountStructure.AddMajorComponent(glComponents.FirstOrDefault(x => x.ComponentName == "ERROR"));

                await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionRepository_Create2Async_RecordInfo_Exists()
            {
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                generalLedgerTransactions.FirstOrDefault().Id = guid;
                await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GeneralLedgerTransactionRepository_Create2Async_TransactionInvoker_error()
            {
                response = new CreateGLPostingResponse()
                {
                    CreateGlPostingError = new List<CreateGlPostingError>() { new CreateGlPostingError() { ErrorMessages = "Error_001", ErrorCodes = "01" } },
                    Error = "Error While Posting",
                };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(response);
                await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_GetProjectReferenceIds() {

                string[] projs = new string[] { "1", "2"};
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(projects);
                var result = await generalLedgerTransactionRepository.GetProjectReferenceIds(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count,2);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_GetProjectReferenceIds_null()
            {

                string[] projs = new string[] { "1", "2" };
                dataReaderMock.Setup(x => x.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await generalLedgerTransactionRepository.GetProjectReferenceIds(projs);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Count, 0);
            }


            #endregion

            #region PUT

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_Update2Async()
            {
                var result = await generalLedgerTransactionRepository.Update2Async(guid, generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionRepository_Update2Async_PostingIdEmpty()
            {
                response = new CreateGLPostingResponse()
                {
                    PostingGuid = "",
                    WarningMessages = new List<string>() { "Warning_Test_0001" },
                };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(response);
                var result = await generalLedgerTransactionRepository.Update2Async(guid,generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GeneralLedgerTransactionRepository_Update2Async_Id_Null()
            {
                await generalLedgerTransactionRepository.Update2Async("", generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GeneralLedgerTransactionRepository_Update2Async_RecordInfo_Exists()
            {
                dataReaderMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                generalLedgerTransactions.FirstOrDefault().Id = guid;
                await generalLedgerTransactionRepository.Update2Async(guid, generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GeneralLedgerTransactionRepository_Update2Async_TransactionInvoker_error()
            {
                response = new CreateGLPostingResponse()
                {
                    CreateGlPostingError = new List<CreateGlPostingError>() { new CreateGlPostingError() { ErrorMessages = "Error_001", ErrorCodes = "01" } },
                    Error = "Error While Posting",
                };
                transFactoryMock.Setup(x => x.GetTransactionInvoker().ExecuteAsync<CreateGLPostingRequest, CreateGLPostingResponse>(It.IsAny<CreateGLPostingRequest>())).ReturnsAsync(response);
                await generalLedgerTransactionRepository.Update2Async(guid, generalLedgerTransactions.FirstOrDefault(), "0002024", GlAccessLevel.Full_Access, generalLedgerAccountStructure);
            }

            #endregion

        }
    }

}
