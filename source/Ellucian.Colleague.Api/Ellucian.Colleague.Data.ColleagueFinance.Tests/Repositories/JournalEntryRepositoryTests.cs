// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class JournalEntryRepositoryTests
    {
        #region Initialize and Cleanup
        private Mock<IColleagueDataReader> dataReader = null;
        private Mock<IColleagueTransactionInvoker> transactionInvoker = null;
        private JournalEntryRepository journalEntryRepository;
        private TestJournalEntryRepository testJournalEntryRepository;
        private JournalEntry journalEntryDomainEntity;

        // Data contract objects
        private JrnlEnts journalEntryDataContract;
        private Opers operDataContract;
        private Collection<Opers> opersDataContracts;
        private Collection<Opers> opersDataContractsForApprovals;
        private Collection<Projects> projectDataContracts;
        private Collection<ProjectsLineItems> projectLineItemDataContracts;
        private GetGlAccountDescriptionResponse getGlAccountDescriptionResponse;

        private string personId = "1";

        [TestInitialize]
        public void Initialize()
        {
            // Set up a mock data reader.
            dataReader = new Mock<IColleagueDataReader>();

            // Set up a mock transaction invoker.
            transactionInvoker = new Mock<IColleagueTransactionInvoker>();

            // Initialize the journal entry data contract object
            journalEntryDataContract = new JrnlEnts();

            //Initialize the OPERS data contract for a single read
            operDataContract = new Opers();

            // Initialize the journal entry repository
            testJournalEntryRepository = new TestJournalEntryRepository();

            this.journalEntryRepository = BuildJournalEntryRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            dataReader = null;
            transactionInvoker = null;
            journalEntryDataContract = null;
            operDataContract = null;
            opersDataContracts = null;
            opersDataContractsForApprovals = null;
            projectDataContracts = null;
            projectLineItemDataContracts = null;
            testJournalEntryRepository = null;
            journalEntryDomainEntity = null;
            getGlAccountDescriptionResponse = null;
        }

        #endregion

        #region Test methods
        #region Base tests
        [TestMethod]
        public async Task GetJournalEntry_Base()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the journal entry are the same
            Assert.AreEqual(this.journalEntryDomainEntity.Author, journalEntry.Author);
            Assert.AreEqual(this.journalEntryDomainEntity.AutomaticReversal, journalEntry.AutomaticReversal);
            Assert.AreEqual(this.journalEntryDomainEntity.Comments, journalEntry.Comments);
            Assert.AreEqual(this.journalEntryDomainEntity.Date, journalEntry.Date);
            Assert.AreEqual(this.journalEntryDomainEntity.EnteredByName, journalEntry.EnteredByName);
            Assert.AreEqual(this.journalEntryDomainEntity.EnteredDate, journalEntry.EnteredDate);
            Assert.AreEqual(this.journalEntryDomainEntity.Id, journalEntry.Id);
            Assert.AreEqual(this.journalEntryDomainEntity.Status, journalEntry.Status);
            Assert.AreEqual(this.journalEntryDomainEntity.TotalCredits, journalEntry.TotalCredits);
            Assert.AreEqual(this.journalEntryDomainEntity.TotalDebits, journalEntry.TotalDebits);
            Assert.AreEqual(this.journalEntryDomainEntity.Type, journalEntry.Type);
        }

        [TestMethod]
        public async Task GetJournalEntry_Base_TestSource_OpeningBalance()
        {
            string journalEntryId = "J000006";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            // Confirm that the SV properties for the journal entry are the same
            Assert.AreEqual(this.journalEntryDomainEntity.Author, journalEntry.Author);
            Assert.AreEqual(this.journalEntryDomainEntity.AutomaticReversal, journalEntry.AutomaticReversal);
            Assert.AreEqual(this.journalEntryDomainEntity.Comments, journalEntry.Comments);
            Assert.AreEqual(this.journalEntryDomainEntity.Date, journalEntry.Date);
            Assert.AreEqual(this.journalEntryDomainEntity.EnteredByName, journalEntry.EnteredByName);
            Assert.AreEqual(this.journalEntryDomainEntity.EnteredDate, journalEntry.EnteredDate);
            Assert.AreEqual(this.journalEntryDomainEntity.Id, journalEntry.Id);
            Assert.AreEqual(this.journalEntryDomainEntity.Status, journalEntry.Status);
            Assert.AreEqual(this.journalEntryDomainEntity.TotalCredits, journalEntry.TotalCredits);
            Assert.AreEqual(this.journalEntryDomainEntity.TotalDebits, journalEntry.TotalDebits);
            Assert.AreEqual(this.journalEntryDomainEntity.Type, journalEntry.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntry_NullId()
        {
            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync(null, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetJournalEntryAsync_EmptyId()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.Recordkey = "";
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullExpenseAccounts()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, null);

            Assert.AreEqual(0, journalEntry.Items.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetJournalEntry_NullJournalEntry()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var nullJournalEntryObject = new JrnlEnts();
            nullJournalEntryObject = null;
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(nullJournalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("1", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetJournalEntry_CStatus()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryStatus.Complete, journalEntry.Status);
        }

        [TestMethod]
        public async Task GetJournalEntry_NStatus()
        {
            string journalEntryId = "J000002";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryStatus.NotApproved, journalEntry.Status);
        }

        [TestMethod]
        public async Task GetJournalEntry_UStatus()
        {
            string journalEntryId = "J000003";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryStatus.Unfinished, journalEntry.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_InvalidJournalEntryStatus()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var journalEntryObject = new JrnlEnts()
            {
                Recordkey = "J000001",
                JrtsStatus = "Z"
            };
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(journalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullStatus()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsStatus = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryStatus.Complete, journalEntry.Status);
            
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyStatus()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsStatus = "";
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryStatus.Complete, journalEntry.Status);
        }

        //[TestMethod]
        //public async Task GetJournalEntryAsync_NullJournalEntryType()
        //{
        //    string journalEntryId = "J000001";
        //    this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    this.journalEntryDataContract.JrtsSource = null;
        //    var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.AreEqual(JournalEntryType.General, journalEntry.Type);
        //}

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyJournalEntryType()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsSource = "";
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(JournalEntryType.General, journalEntry.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_MissingPostingDate()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var journalEntryObject = new JrnlEnts()
            {
                Recordkey = "J000001",
                JrtsStatus = "C",
                JrtsSource = "JE",
                JrtsTrDate = null
            };
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(journalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_MissingAddDateDate()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var journalEntryObject = new JrnlEnts()
            {
                Recordkey = "J000001",
                JrtsStatus = "C",
                JrtsSource = "JE",
                JrtsTrDate = new DateTime(2015, 1, 1),
                JrnlEntsAddDate = null
            };
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(journalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_MissingAddOperator()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var journalEntryObject = new JrnlEnts()
            {
                Recordkey = "J000001",
                JrtsStatus = "C",
                JrtsSource = "JE",
                JrtsTrDate = new DateTime(2015, 1, 1),
                JrnlEntsAddDate = new DateTime(2015, 1, 1),
                JrnlEntsAddOperator = null
            };
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(journalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_OpersReadRecordReturnsNull()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrnlEntsAddOperator = "AJK";
            this.operDataContract = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrnlEntsAddOperator, journalEntry.EnteredByName);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullOpersUserName()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrnlEntsAddOperator = "AJK";
            this.operDataContract.SysUserName = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrnlEntsAddOperator, journalEntry.EnteredByName);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyOpersUserName()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrnlEntsAddOperator = "AJK";
            this.operDataContract.SysUserName = "";
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrnlEntsAddOperator, journalEntry.EnteredByName);
        }

        //[TestMethod]
        //public async Task GetJournalEntryAsync_NullReversalFlag()
        //{
        //    string journalEntryId = "J000001";
        //    this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    this.journalEntryDataContract.JrtsReversalFlag = null;
        //    var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.IsFalse(journalEntry.AutomaticReversal);
        //}

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyReversalFlag()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsReversalFlag = "";
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.IsFalse(journalEntry.AutomaticReversal);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullAuthorizations()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsAuthorizations = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsNextApprovalIds.Count, journalEntry.Approvers.Count);
            foreach (var approverEntity in journalEntry.Approvers)
            {
                Assert.IsTrue(this.journalEntryDataContract.JrtsNextApprovalIds.Contains(approverEntity.ApproverId));
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullNextApprovalIds()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsNextApprovalIds = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsAuthorizations.Count, journalEntry.Approvers.Count);
            foreach (var approverEntity in journalEntry.Approvers)
            {
                Assert.IsTrue(this.journalEntryDataContract.JrtsAuthorizations.Contains(approverEntity.ApproverId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_InvalidJournalEntryType()
        {
            // Mock ReadRecordAsync to return a pre-defined, null journal entries data contract
            var journalEntryObject = new JrnlEnts()
            {
                Recordkey = "J000001",
                JrtsStatus = "C",
                JrtsSource = "XY"
            };
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(Task.FromResult(journalEntryObject));

            var journalEntry = await this.journalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_OpersBulkReadReturnsNull()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersDataContractsForApprovals = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Approvers.Count);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_OpersBulkReadReturnsEmptyList()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.opersDataContractsForApprovals = new Collection<Opers>();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Approvers.Count);
        }

        //[TestMethod]
        //public async Task GetJournalEntryAsync_NullApproverRecordKey()
        //{
        //    string journalEntryId = "J000001";
        //    this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var opersContract in opersDataContractsForApprovals)
        //    {
        //        opersContract.Recordkey = null;
        //    }
        //    dataReader.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(opersDataContractsForApprovals));
        //    var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.AreEqual(0, journalEntry.Approvers.Count);
        //}

        //[TestMethod]
        //public async Task GetJournalEntryAsync_EmptyApproverRecordKey()
        //{
        //    string journalEntryId = "J000001";
        //    this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        //    var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
        //    ConvertDomainEntitiesIntoDataContracts();
        //    InitializeMockMethods();

        //    foreach (var opersContract in opersDataContractsForApprovals)
        //    {
        //        opersContract.Recordkey = "";
        //    }
        //    dataReader.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(Task.FromResult(opersDataContractsForApprovals));
        //    var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

        //    Assert.AreEqual(0, journalEntry.Approvers.Count);
        //}

        [TestMethod]
        public async Task GetJournalEntryAsync_NullApprovalAssociation()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsAuthEntityAssociation = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.opersDataContractsForApprovals.Count, journalEntry.Approvers.Count);
            foreach (var approver in journalEntry.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyApprovalAssociation()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsAuthEntityAssociation = new List<JrnlEntsJrtsAuth>();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.opersDataContractsForApprovals.Count, journalEntry.Approvers.Count);
            foreach (var approver in journalEntry.Approvers)
            {
                Assert.IsNull(approver.ApprovalDate);
            }
        }
        #endregion

        #region Items tests
        [TestMethod]
        public async Task GetJournalEntry_Items()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDomainEntity.Items.Count(), journalEntry.Items.Count(), "Journal Entry should have the same number of items.");

            foreach (var item in this.journalEntryDomainEntity.Items)
            {
                Assert.IsTrue(journalEntry.Items.Any(x =>
                    x.Credit == item.Credit
                    && x.Debit == item.Debit
                    && x.Description == item.Description
                    && x.GlAccountNumber == item.GlAccountNumber
                    && x.GlAccountDescription == item.GlAccountDescription
                    && x.ProjectId == item.ProjectId
                    && x.ProjectLineItemCode == item.ProjectLineItemCode
                    && x.ProjectLineItemId == item.ProjectLineItemId
                    && x.ProjectNumber == item.ProjectNumber));
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullItemsAssociation()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsDataEntityAssociation = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Items.Count);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyItemsAssociation()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.journalEntryDataContract.JrtsDataEntityAssociation = new List<JrnlEntsJrtsData>();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Items.Count);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullGlDistributionGlNumber()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsGlNoAssocMember = null;
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Items.Count);
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyGlDistributionGlNumber()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsGlNoAssocMember = "";
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(0, journalEntry.Items.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_MissingItemDescription()
        {
            string journalEntryId = "J000007";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_MissingItemGlAccount()
        {
            string journalEntryId = "J000008";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetJournalEntry_ItemWithBothDebitAndCredit()
        {
            string journalEntryId = "J000005";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);
        }
        #endregion
        #endregion

        #region Approvers and Next Approvers tests
        [TestMethod]
        public async Task GetJournalEntry_HasApproversAndNextApprovers()
        {
            string journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDomainEntity.Approvers.Count(), journalEntry.Approvers.Count());
            foreach (var approver in this.journalEntryDomainEntity.Approvers)
            {
                Assert.IsTrue(journalEntry.Approvers.Any(x =>
                    x.ApproverId == approver.ApproverId
                    && x.ApprovalName == approver.ApprovalName
                    && x.ApprovalDate == approver.ApprovalDate));
            }
        }
        #endregion

        #region GL security tests
        [TestMethod]
        public async Task UserHasFullAccess()
        {
            var journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDomainEntity.Items.Count(), journalEntry.Items.Count(), "All the journal entry items should be visible.");
            foreach (var item in this.journalEntryDomainEntity.Items)
            {
                Assert.IsTrue(journalEntry.Items.Any(x => 
                    x.Credit == item.Credit
                    && x.Debit == item.Debit
                    && x.Description == item.Description
                    && x.GlAccountNumber == item.GlAccountNumber
                    && x.ProjectId == item.ProjectId
                    && x.ProjectLineItemCode == item.ProjectLineItemCode
                    && x.ProjectLineItemId == item.ProjectLineItemId
                    && x.ProjectNumber == item.ProjectNumber));
            }

            Assert.AreEqual(journalEntry.Items.Sum(x => x.Credit), journalEntry.TotalCredits, "TotalCredits should equal the sum of the line items.");
            Assert.AreEqual(journalEntry.Items.Sum(x => x.Debit), journalEntry.TotalDebits, "TotalDebits should equal the sum of the line items.");
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_AllItemsAvailable()
        {
            var journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            // The test JE should have five items.
            Assert.AreEqual(5, this.journalEntryDomainEntity.Items.Count);

            // The actual JE should have the same number of items as the test JE.
            Assert.AreEqual(this.journalEntryDomainEntity.Items.Count, journalEntry.Items.Count());
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_PartialItemsAvailable()
        {
            var journalEntryId = "J000112";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            // The test JE should have five items.
            Assert.AreEqual(5, this.journalEntryDomainEntity.Items.Count);

            // The actual JE should have two items.
            Assert.AreEqual(2, journalEntry.Items.Count());

            var expectedTotalCredits = journalEntryDataContract.JrtsDataEntityAssociation.Sum(x => x.JrtsCreditAssocMember ?? 0);
            var expectedTotalDebits = journalEntryDataContract.JrtsDataEntityAssociation.Sum(x => x.JrtsDebitAssocMember ?? 0);
            Assert.AreEqual(expectedTotalCredits, journalEntry.TotalCredits, "TotalCredits should be equal to the sum of the credits in the data contract.");
            Assert.AreEqual(expectedTotalCredits, journalEntry.TotalDebits, "TotalDebits should be equal to the sum of the debits in the data contract.");

            Assert.IsTrue(journalEntry.Items.Sum(x => x.Credit) < journalEntry.TotalCredits, "The sum of the credit items should be less than the total credit amount.");
            Assert.IsTrue(journalEntry.Items.Sum(x => x.Debit) < journalEntry.TotalDebits, "The sum of the debit items should be less than the total debit amount.");
        }

        [TestMethod]
        public async Task UserHasPossibleAccess_NoItemsAvailable()
        {
            var journalEntryId = "J000113";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.IsTrue(journalEntry.Items.Count() == 0, "The journal entry should not have any items.");
        }

        [TestMethod]
        public async Task UserHasNoAccess()
        {
            var journalEntryId = "J000001";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.No_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.No_Access, expenseAccounts);

            Assert.IsTrue(journalEntry.Items.Count() == 0, "The journal entry should not have any items.");
        }
        #endregion

        [TestMethod]
        public async Task GetJournalEntryAsync_NullProjectIds()
        {
            string journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsProjectsCfIdAssocMember = null;
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsDataEntityAssociation.Count, journalEntry.Items.Count);
            foreach (var item in journalEntry.Items)
            {
                Assert.IsNull(item.ProjectId);
                Assert.IsNull(item.ProjectNumber);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyProjectIds()
        {
            string journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsProjectsCfIdAssocMember = "";
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsDataEntityAssociation.Count, journalEntry.Items.Count);
            foreach (var item in journalEntry.Items)
            {
                Assert.AreEqual("", item.ProjectId);
                Assert.IsNull(item.ProjectNumber);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_NullProjectLineItemIds()
        {
            string journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsPrjItemIdsAssocMember = null;
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsDataEntityAssociation.Count, journalEntry.Items.Count);
            foreach (var item in journalEntry.Items)
            {
                Assert.IsNull(item.ProjectLineItemId);
                Assert.IsNull(item.ProjectLineItemCode);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_EmptyProjectLineItemIds()
        {
            string journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            foreach (var glDist in this.journalEntryDataContract.JrtsDataEntityAssociation)
            {
                glDist.JrtsPrjItemIdsAssocMember = "";
            }
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsDataEntityAssociation.Count, journalEntry.Items.Count);
            foreach (var item in journalEntry.Items)
            {
                Assert.AreEqual("", item.ProjectLineItemId);
                Assert.IsNull(item.ProjectLineItemCode);
            }
        }

        [TestMethod]
        public async Task GetJournalEntryAsync_ProjectsAndProjectsLineItemsBulkReadsReturnNull()
        {
            string journalEntryId = "J000111";
            this.journalEntryDomainEntity = await testJournalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Full_Access, null);
            var expenseAccounts = CalculateGlNumbersForUser(journalEntryId);
            ConvertDomainEntitiesIntoDataContracts();
            InitializeMockMethods();

            this.projectDataContracts = null;
            this.projectLineItemDataContracts = null;
            var journalEntry = await journalEntryRepository.GetJournalEntryAsync(journalEntryId, personId, GlAccessLevel.Possible_Access, expenseAccounts);

            Assert.AreEqual(this.journalEntryDataContract.JrtsDataEntityAssociation.Count, journalEntry.Items.Count);
            foreach (var item in journalEntry.Items)
            {
                Assert.IsTrue(string.IsNullOrEmpty(item.ProjectNumber));
                Assert.IsTrue(string.IsNullOrEmpty(item.ProjectLineItemCode));
            }
        }

        #region Private methods
        private JournalEntryRepository BuildJournalEntryRepository()
        {
            // Instantiate all objects necessary to mock the data reader and CTX calls
            var cacheProviderObject = new Mock<ICacheProvider>().Object;
            var transactionFactory = new Mock<IColleagueTransactionFactory>();
            var transactionFactoryObject = transactionFactory.Object;
            var loggerObject = new Mock<ILogger>().Object;

            // The transaction factory has a method to get its data reader
            // Make sure that method returns our mock data reader
            transactionFactory.Setup(transFac => transFac.GetDataReader()).Returns(dataReader.Object);
            transactionFactory.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            return new JournalEntryRepository(cacheProviderObject, transactionFactoryObject, loggerObject);
        }

        private void InitializeMockMethods()
        {
            // Mock ReadRecordAsync to return a pre-defined Journal Entries data contract
            dataReader.Setup<Task<JrnlEnts>>(acc => acc.ReadRecordAsync<JrnlEnts>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.journalEntryDataContract);
                });

            // Mock a single read to OPERS for the person that entered the journal entry
            this.operDataContract = new Opers()
                {
                    Recordkey = "TGL",
                    SysUserName = "Teresa Longerbeam"
                };
            dataReader.Setup<Task<Opers>>(acc => acc.ReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(this.operDataContract);
                });

            // Mock ReadRecordAsync to return a pre-defined Opers data contract.
            // Mock bulk read UT.OPERS bulk read
            opersDataContractsForApprovals = new Collection<Opers>()
                {
                    new Opers()
                    {
                        // "0000001"
                        Recordkey = "0000001", SysUserName = "Teresa Longerbeam"
                    },
                    new Opers()
                    {
                        // ""
                        Recordkey = "0000002", SysUserName = "Gary Thorne"
                    },
                    new Opers()
                    {
                        // "0000003"
                        Recordkey = "0000003", SysUserName = "Andy Kleehammer"
                    },
                    new Opers()
                    {
                        // "0000004"
                        Recordkey = "0000004", SysUserName = "Aimee Rodgers"
                    }
                };
            dataReader.Setup<Task<Collection<Opers>>>(acc => acc.BulkReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(
                (string fileName, string[] approverIds, bool flag) =>
                {
                    Collection<Opers> result = new Collection<Opers>();

                    if (opersDataContractsForApprovals == null)
                    {
                        result = null;
                    }
                    else
                    {
                        foreach (var opersDataContract in opersDataContractsForApprovals)
                        {
                            if (approverIds.Contains(opersDataContract.Recordkey))
                            {
                                result.Add(opersDataContract);
                            }
                        }
                    }

                    return Task.FromResult(result);
                });

            // Mock BuldReadRecordAsync to return pre-defined Projects data contracts

            dataReader.Setup<Task<Collection<Projects>>>(acc => acc.BulkReadRecordAsync<Projects>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectDataContracts);
                });

            // Mock BulkReadRecordAsync to return pre-defined ProjectsLineItems data contracts
            dataReader.Setup<Task<Collection<ProjectsLineItems>>>(acc => acc.BulkReadRecordAsync<ProjectsLineItems>(It.IsAny<string[]>(), true)).Returns(() =>
                {
                    return Task.FromResult(projectLineItemDataContracts);
                });

            // Mock Execute within the transaction invoker to return a GetHierarchyNamesForIdsResponse object
            transactionInvoker.Setup(tio => tio.Execute<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(this.getGlAccountDescriptionResponse);
        }

        private List<string> CalculateGlNumbersForUser(string journalEntryId)
        {
            // Return a specific set of GL accounts depending on which journal entry we are processing
            var glAccountsToReturn = new List<string>();
            switch (journalEntryId)
            {
                case "J000111":
                    glAccountsToReturn = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54011", "11_00_01_00_33333_54030", "11_00_01_00_33333_54400", "11_00_01_00_33333_51000", };
                    break;
                case "J000112":
                    glAccountsToReturn = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54400" };
                    break;
                case "J000113":
                    // Do nothing; we want to return an empty list
                    break;
                default:
                    break;
            }

            return glAccountsToReturn;
        }

        private void ConvertDomainEntitiesIntoDataContracts()
        {
            // Convert the Journal Entry object
            this.journalEntryDataContract.Recordkey = this.journalEntryDomainEntity.Id;
            this.journalEntryDataContract.JrnlEntsAddDate = this.journalEntryDomainEntity.EnteredDate;
            this.journalEntryDataContract.JrnlEntsAddOperator = this.journalEntryDomainEntity.EnteredByName;
            this.journalEntryDataContract.JrtsAuthor = this.journalEntryDomainEntity.Author;
            this.journalEntryDataContract.JrtsComments = this.journalEntryDomainEntity.Comments;

            if (this.journalEntryDomainEntity.AutomaticReversal == true)
            {
                this.journalEntryDataContract.JrtsReversalFlag = "Y";
            }
            else
            {
                this.journalEntryDataContract.JrtsReversalFlag = "N";
            }

            this.journalEntryDataContract.JrtsTrDate = this.journalEntryDomainEntity.Date;

            switch (this.journalEntryDomainEntity.Status)
            {
                case JournalEntryStatus.Complete:
                    this.journalEntryDataContract.JrtsStatus = "C";
                    break;
                case JournalEntryStatus.NotApproved:
                    this.journalEntryDataContract.JrtsStatus = "N";
                    break;
                case JournalEntryStatus.Unfinished:
                    this.journalEntryDataContract.JrtsStatus = "U";
                    break;
                default:
                    throw new Exception("Invalid status specified in JournalEntryRepositoryTests");
            }

            switch (this.journalEntryDomainEntity.Type)
            {
                case JournalEntryType.General:
                    this.journalEntryDataContract.JrtsSource = "JE";
                    break;
                case JournalEntryType.OpeningBalance:
                    this.journalEntryDataContract.JrtsSource = "AA";
                    break;
                default:
                    throw new Exception("Invalid type specified in JournalEntryRepositoryTests");
            }

            this.projectDataContracts = new Collection<Projects>();
            this.projectLineItemDataContracts = new Collection<ProjectsLineItems>();

            this.journalEntryDataContract.JrtsGlNo = new List<string>();
            this.journalEntryDataContract.JrtsDescription = new List<string>();
            this.journalEntryDataContract.JrtsProjectsCfId = new List<string>();
            this.journalEntryDataContract.JrtsPrjItemIds = new List<string>();
            this.journalEntryDataContract.JrtsDebit = new List<decimal?>();
            this.journalEntryDataContract.JrtsCredit = new List<decimal?>();

            journalEntryDataContract.JrtsDataEntityAssociation = new List<JrnlEntsJrtsData>();
            foreach (var item in this.journalEntryDomainEntity.Items)
            {
                journalEntryDataContract.JrtsDataEntityAssociation.Add(new JrnlEntsJrtsData()
                {
                    JrtsGlNoAssocMember = item.GlAccountNumber == "null" ? null : item.GlAccountNumber,
                    JrtsDescriptionAssocMember = item.Description == "null" ? null : item.Description,
                    JrtsProjectsCfIdAssocMember = item.ProjectId,
                    JrtsPrjItemIdsAssocMember = item.ProjectLineItemId,
                    JrtsDebitAssocMember = item.Debit,
                    JrtsCreditAssocMember = item.Credit
                });

                this.projectDataContracts.Add(new Projects()
                {
                    Recordkey = item.ProjectId,
                    PrjRefNo = item.ProjectNumber
                });

                this.projectLineItemDataContracts.Add(new ProjectsLineItems()
                {
                    Recordkey = item.ProjectLineItemId,
                    PrjlnProjectItemCode = item.ProjectLineItemCode
                });
            }

            this.getGlAccountDescriptionResponse = new GetGlAccountDescriptionResponse();
            var GlAccountIds = new List<string>();
            var GlAccountDescriptions = new List<string>();
            foreach (var item in this.journalEntryDomainEntity.Items)
            {
                    GlAccountIds.Add(item.GlAccountNumber);
                    GlAccountDescriptions.Add(item.GlAccountDescription);
            }
            getGlAccountDescriptionResponse.GlAccountIds = GlAccountIds;
            getGlAccountDescriptionResponse.GlDescriptions = GlAccountDescriptions;

            // Build a list of Approver data contracts
            ConvertApproversIntoDataContracts();
        }

        private void ConvertApproversIntoDataContracts()
        {
            // Initialize the associations for approvers and next approvers.
            this.journalEntryDataContract.JrtsAuthEntityAssociation = new List<JrnlEntsJrtsAuth>();
            this.journalEntryDataContract.JrtsApprEntityAssociation = new List<JrnlEntsJrtsAppr>();
            this.opersDataContracts = new Collection<Opers>();
            this.journalEntryDataContract.JrtsAuthorizations = new List<string>();
            this.journalEntryDataContract.JrtsNextApprovalIds = new List<string>();

            foreach (var approver in this.journalEntryDomainEntity.Approvers)
            {
                if (approver.ApprovalDate != null)
                {
                    // Populate approvers
                    var dataContract = new JrnlEntsJrtsAuth()
                    {
                        JrtsAuthorizationsAssocMember = approver.ApproverId,
                        JrtsAuthorizationDatesAssocMember = approver.ApprovalDate
                    };

                    this.journalEntryDataContract.JrtsAuthEntityAssociation.Add(dataContract);
                    this.journalEntryDataContract.JrtsAuthorizations.Add(approver.ApproverId);
                }
                else
                {
                    // Populate next approvers
                    var nextApproverDataContract = new JrnlEntsJrtsAppr()
                    {
                        JrtsNextApprovalIdsAssocMember = approver.ApproverId
                    };
                    this.journalEntryDataContract.JrtsApprEntityAssociation.Add(nextApproverDataContract);
                    this.journalEntryDataContract.JrtsNextApprovalIds.Add(approver.ApproverId);
                }

                // Populate the Opers data contract
                this.opersDataContracts.Add(new Opers()
                {
                    Recordkey = approver.ApproverId,
                    SysUserName = approver.ApprovalName
                });
            }
        }

        #endregion
    }
}
