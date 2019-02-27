// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
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
    public class BudgetAdjustmentsRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        private BudgetAdjustmentsRepository actualBudgetAdjustmentRepository;
        private BudgetAdjustmentsRepository actualBudgetAdjustmentRepository2;
        private TestBudgetAdjustmentRepository testBudgetAdjustmentRepository;
        private TestDraftBudgetAdjustmentRepository testDraftRepository;
        private BudgetAdjustment budgetAdjustmentDomainEntity;

        private BudgetEntries budgetEntriesDataContract;
        private Collection<BudgetEntries> budgetEntriesDataContracts;
        private Collection<DraftBudgetEntries> draftDataContracts;
        private string[] budgetEntriesIds = new string[] { "B000111", "B000222", "B000333", "B000444" };
        private string[] budgetEntriesNotApprovedIds = new string[] { "B009999", "B008888", "B007777", "B006666", "B005555" };
        private string[] draftBudgetEntriesIds = new string[] { "1", "2", "3", "4", "5" };
        private Staff staffDataContract;
        private Collection<Staff> staffDataContracts;
        private string[] staffIds = new string[] { "0000001" };
        private string[] pendingApprovalStaffIds = new string[] { "0000001" };
        private string personId = "0000001";
        private List<string> personIds;
        private List<string> hierarchies;

        private IEnumerable<BudgetAdjustmentSummary> budgetAdjustmentSummaries;
        private IEnumerable<BudgetAdjustmentPendingApprovalSummary> budgetAdjustmentPendingApprovalSummaries;

        private TxUpdateBudgetAdjustmentResponse budgetAdjustmentResponse;
        private GetHierarchyNamesForIdsResponse getNamesforIdsResponse;
        private TxDeleteDraftBudgetAdjustmentResponse deleteDraftResponse;

        // This is the budget adjustment returned by the CTX when creating one.
        private string journalNumber = "B123456";
        private BudgetEntries budgetEntriesDataContract2 = new BudgetEntries()
        {
            Recordkey = "B123456",
            BgteSource = "BU",
            BudgetEntriesAddopr = "0000001",
            BgteAuthor = "Andy Kleehammer",
            BgteComments = "additional justificaton",
            BgteTrDate = DateTime.Now.Date,
            BgteStatus = new List<string>() { "C" },
            BgteStatEntityAssociation = new List<BudgetEntriesBgteStat>()
                {
                    new BudgetEntriesBgteStat
                    {
                        BgteStatusAssocMember = "C",
                        BgteStatusDateAssocMember = DateTime.Now
                    }
                },
            BgteDataEntityAssociation = new List<BudgetEntriesBgteData>()
                {
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51000",
                        BgteDescriptionAssocMember = "more money",
                        BgteDebitAssocMember = 100.00m,
                        BgteCreditAssocMember = 0m
                    },
                    new BudgetEntriesBgteData
                    {
                        BgteGlNoAssocMember = "11_10_00_01_20601_51001",
                        BgteDescriptionAssocMember = "more money",
                        BgteDebitAssocMember = 0m,
                        BgteCreditAssocMember = 100.00m
                    }
                },
            BgteApprovalEntityAssociation = new List<BudgetEntriesBgteApproval>()
            {
                new BudgetEntriesBgteApproval
                {
                    BgteNextApprovalIdsAssocMember = "AER",
                    BgteApprovalLevelsAssocMember = ""
                },
                new BudgetEntriesBgteApproval
                {
                    BgteNextApprovalIdsAssocMember = "MEL",
                    BgteApprovalLevelsAssocMember = ""
                }
            },
            BgteNextApprovalIds = new List<string>() { "AER", "MEL" }
        };

        private Collection<DataContracts.Opers> opersResponse;
        private Collection<DataContracts.Opers> opersDataContracts;
        private string criteria = " ";

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            actualBudgetAdjustmentRepository = new BudgetAdjustmentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            actualBudgetAdjustmentRepository2 = new BudgetAdjustmentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            testBudgetAdjustmentRepository = new TestBudgetAdjustmentRepository();
            testDraftRepository = new TestDraftBudgetAdjustmentRepository();

            // Initialize the data contracts.
            budgetEntriesDataContract = new BudgetEntries();
            budgetEntriesDataContracts = new Collection<BudgetEntries>();
            draftDataContracts = new Collection<DraftBudgetEntries>();
            staffDataContract = new Staff();
            staffDataContracts = new Collection<Staff>();

            InitializeMockStatements();

            budgetAdjustmentSummaries = new List<BudgetAdjustmentSummary>();
            budgetAdjustmentPendingApprovalSummaries = new List<BudgetAdjustmentPendingApprovalSummary>();

            personIds = new List<string>() { "0010475", "0013272" };
            hierarchies = new List<string>() { "PREFERRED", "PREFERRED" };
            getNamesforIdsResponse.IoPersonIds = new List<string>() { "0010475", "0013272" };
            getNamesforIdsResponse.IoHierarchies = new List<string>() { "PREFERRED", "PREFERRED" };
            getNamesforIdsResponse.OutPersonNames = new List<string>() { "Anjana Tesa Gutierrez", "Teresa P Castro" };
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualBudgetAdjustmentRepository = null;
            actualBudgetAdjustmentRepository2 = null;
            testBudgetAdjustmentRepository = null;
            testDraftRepository = null;
            budgetAdjustmentDomainEntity = null;
            budgetEntriesDataContract = null;
            budgetEntriesDataContracts = null;
            draftDataContracts = null;
            budgetAdjustmentSummaries = null;
            budgetAdjustmentPendingApprovalSummaries = null;
            staffDataContract = null;
            staffDataContracts = null;
        }
        #endregion

        #region Tests for POST
        [TestMethod]
        public async Task CreateAsync_Success()
        {
            var transactionDate = DateTime.Now.Date;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("11_10_00_01_20601_51001", 100.00m, 0.00m),
                new AdjustmentLine("11_10_00_01_20601_51000", 0m, 100.00m)
            };

            var adjustmentInputEntity = new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            var nextApprovers = new List<NextApprover>()
            {
                new NextApprover("AER"),
                new NextApprover("MEL")
            };
            adjustmentInputEntity.NextApprovers = nextApprovers;
            adjustmentInputEntity.Approvers = new List<Approver>();

            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.CreateAsync(adjustmentInputEntity);

            Assert.AreEqual(this.journalNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(adjustmentInputEntity.TransactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(adjustmentInputEntity.Initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(adjustmentInputEntity.Reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(adjustmentInputEntity.Comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(true, adjustmentOutputEntity.DraftDeletionSuccessfulOrUnnecessary);
            Assert.AreEqual(null, adjustmentOutputEntity.ErrorMessages);
            Assert.AreEqual(2, adjustmentOutputEntity.NextApprovers.Count);
            Assert.AreEqual(0, adjustmentOutputEntity.Approvers.Count);

            foreach (var adjustmentLine in adjustmentInputEntity.AdjustmentLines)
            {
                var outputAdjustmentLine = adjustmentOutputEntity.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(outputAdjustmentLine);
            }

            foreach (var nextApprover in adjustmentInputEntity.NextApprovers)
            {
                var outputNextApprover = adjustmentOutputEntity.NextApprovers.FirstOrDefault(x =>
                    x.NextApproverId == nextApprover.NextApproverId);
                Assert.IsNotNull(outputNextApprover);
            }
        }

        [TestMethod]
        public async Task CreateAsync_DraftNotDeleted()
        {
            var transactionDate = DateTime.Now.Date;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            var draftId = "2";
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("11_10_00_01_20601_51001", 100.00m, 0.00m),
                new AdjustmentLine("11_10_00_01_20601_51000", 0m, 100.00m)
            };

            var adjustmentInputEntity = new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.DraftBudgetAdjustmentId = draftId;
            var nextApprovers = new List<NextApprover>()
            {
                new NextApprover("AER"),
                new NextApprover("MEL")
            };
            adjustmentInputEntity.NextApprovers = nextApprovers;
            adjustmentInputEntity.Approvers = new List<Approver>();

            // Set up the failure response.
            deleteDraftResponse.AErrorCode = "MissingRecord";
            deleteDraftResponse.AErrorMessage = "Record Locked - unable to delete DRAFT.BUDGET.ENTRIES record for ID = <V.A.DRAFT.BE.ID>";

            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.CreateAsync(adjustmentInputEntity);

            Assert.AreEqual(journalNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(false, adjustmentOutputEntity.DraftDeletionSuccessfulOrUnnecessary);
            Assert.AreEqual(null, adjustmentOutputEntity.ErrorMessages);

            foreach (var adjustmentLine in adjustmentInputEntity.AdjustmentLines)
            {
                var outputAdjustmentLine = adjustmentOutputEntity.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(outputAdjustmentLine);
            }

            foreach (var nextApprover in adjustmentInputEntity.NextApprovers)
            {
                var outputNextApprover = adjustmentOutputEntity.NextApprovers.FirstOrDefault(x =>
                    x.NextApproverId == nextApprover.NextApproverId);
                Assert.IsNotNull(outputNextApprover);
            }
        }

        [TestMethod]
        public async Task CreateAsync_ErrorReturned()
        {
            // Set up the request.
            var transactionDate = DateTime.Now;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("1", 100m, 0m),
                new AdjustmentLine("2", 0m, 100m)
            };

            // Set up the failure response.
            budgetAdjustmentResponse.ABudgetEntryId = "";
            budgetAdjustmentResponse.AlMessage = new List<string>() { "One or more of the GL accounts has insufficient funds.", "Security not assigned for GL account 1234." };

            var adjustmentInputEntity = new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.CreateAsync(adjustmentInputEntity);

            foreach (var errorMessage in budgetAdjustmentResponse.AlMessage)
            {
                var matchingMessage = adjustmentOutputEntity.ErrorMessages.FirstOrDefault(x => x == errorMessage);
                Assert.IsNotNull(matchingMessage);
            }
            Assert.IsTrue(string.IsNullOrEmpty(adjustmentOutputEntity.Id));
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);

            foreach (var adjustmentLine in adjustmentInputEntity.AdjustmentLines)
            {
                var outputAdjustmentLine = adjustmentOutputEntity.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(outputAdjustmentLine);
            }
        }
        #endregion

        #region Tests for PUT
        [TestMethod]
        public async Task UpdateAsync__WithNextApprovers_Success()
        {
            var transactionDate = DateTime.Now.Date;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            var id = "B0000001";
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("11_10_00_01_20601_51001", 100.00m, 0.00m),
                new AdjustmentLine("11_10_00_01_20601_51000", 0m, 100.00m)
            };
            var nextApprovers = new List<NextApprover>()
            {
                new NextApprover("AER"),
                new NextApprover("MEL")
            };

            var adjustmentInputEntity = new BudgetAdjustment(id, transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.NextApprovers = nextApprovers;

            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.UpdateAsync(adjustmentInputEntity.Id, adjustmentInputEntity);

            Assert.AreEqual(this.journalNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(true, adjustmentOutputEntity.DraftDeletionSuccessfulOrUnnecessary);
            Assert.AreEqual(null, adjustmentOutputEntity.ErrorMessages);

            foreach (var adjustmentLine in adjustmentInputEntity.AdjustmentLines)
            {
                var outputAdjustmentLine = adjustmentOutputEntity.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(outputAdjustmentLine);
            }

            foreach (var nextApprover in adjustmentInputEntity.NextApprovers)
            {
                var outputNextApprover = adjustmentOutputEntity.NextApprovers.FirstOrDefault(x =>
                x.NextApproverId == nextApprover.NextApproverId);
                Assert.IsNotNull(outputNextApprover);
            }
        }

        [TestMethod]
        public async Task UpdateAsync_ErrorReturned()
        {
            // Set up the request.
            var transactionDate = DateTime.Now;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            var id = "B0000001";
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("1", 100m, 0m),
                new AdjustmentLine("2", 0m, 100m)
            };

            // Set up the failure response.
            budgetAdjustmentResponse.ABudgetEntryId = "";
            budgetAdjustmentResponse.AlMessage = new List<string>() { "One or more of the GL accounts has insufficient funds.", "Security not assigned for GL account 1234." };

            var adjustmentInputEntity = new BudgetAdjustment(id, transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.UpdateAsync(adjustmentInputEntity.Id, adjustmentInputEntity);

            foreach (var errorMessage in budgetAdjustmentResponse.AlMessage)
            {
                var matchingMessage = adjustmentOutputEntity.ErrorMessages.FirstOrDefault(x => x == errorMessage);
                Assert.IsNotNull(matchingMessage);
            }
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);

            foreach (var adjustmentLine in adjustmentInputEntity.AdjustmentLines)
            {
                var outputAdjustmentLine = adjustmentOutputEntity.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(outputAdjustmentLine);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAsync_NullId()
        {
            // Set up the request.
            var transactionDate = DateTime.Now;
            var initiator = "Andy Kleehammer";
            var reason = "more money";
            var comments = "additional justificaton";
            var personId = "0000001";
            string id = null;
            var adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("1", 100m, 0m),
                new AdjustmentLine("2", 0m, 100m)
            };

            var adjustmentInputEntity = new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.UpdateAsync(id, adjustmentInputEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAsync_NullInputDto()
        {
            // Set up the request.
            string id = "B0000001";

            BudgetAdjustment adjustmentInputEntity = null;

            var adjustmentOutputEntity = await actualBudgetAdjustmentRepository.UpdateAsync(id, adjustmentInputEntity);
        }

        #endregion

        #region Test for GET

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_Success_Complete_NumericAddOper()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000111");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "ALPHALOGIN";
            staffDataContract.Recordkey = "0000001";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000111");

            Assert.AreEqual(budgetAdjustmentDomainEntity.Comments, actualBudgetAdjustment.Comments);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Id, actualBudgetAdjustment.Id);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Initiator, actualBudgetAdjustment.Initiator);
            Assert.AreEqual(budgetAdjustmentDomainEntity.PersonId, actualBudgetAdjustment.PersonId);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Reason, actualBudgetAdjustment.Reason);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Status, actualBudgetAdjustment.Status);
            Assert.AreEqual(budgetAdjustmentDomainEntity.TransactionDate, actualBudgetAdjustment.TransactionDate);

            Assert.AreEqual(budgetAdjustmentDomainEntity.AdjustmentLines.Count(), actualBudgetAdjustment.AdjustmentLines.Count());

            foreach (var adjustmentLine in budgetAdjustmentDomainEntity.AdjustmentLines)
            {
                Assert.IsTrue(actualBudgetAdjustment.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.FromAmount
                && x.GlNumber == adjustmentLine.GlNumber
                && x.ToAmount == adjustmentLine.ToAmount));
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_Success_AlphaAddOper()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000111");
            ConvertDomainEntitiesIntoDataContracts();
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000111");

            Assert.AreEqual(budgetAdjustmentDomainEntity.Comments, actualBudgetAdjustment.Comments);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Id, actualBudgetAdjustment.Id);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Initiator, actualBudgetAdjustment.Initiator);
            Assert.AreEqual(budgetAdjustmentDomainEntity.PersonId, actualBudgetAdjustment.PersonId);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Reason, actualBudgetAdjustment.Reason);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Status, actualBudgetAdjustment.Status);
            Assert.AreEqual(budgetAdjustmentDomainEntity.TransactionDate, actualBudgetAdjustment.TransactionDate);

            Assert.AreEqual(budgetAdjustmentDomainEntity.AdjustmentLines.Count(), actualBudgetAdjustment.AdjustmentLines.Count());

            foreach (var adjustmentLine in budgetAdjustmentDomainEntity.AdjustmentLines)
            {
                Assert.IsTrue(actualBudgetAdjustment.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.FromAmount
                && x.GlNumber == adjustmentLine.GlNumber
                && x.ToAmount == adjustmentLine.ToAmount));
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_Success_Unfinished()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000444");
            ConvertDomainEntitiesIntoDataContracts();
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000444");

            Assert.AreEqual(budgetAdjustmentDomainEntity.Id, actualBudgetAdjustment.Id);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Status, actualBudgetAdjustment.Status);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_Success_NotApproved()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000333");
            ConvertDomainEntitiesIntoDataContracts();
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000333");

            Assert.AreEqual(budgetAdjustmentDomainEntity.Id, actualBudgetAdjustment.Id);
            Assert.AreEqual(budgetAdjustmentDomainEntity.Status, actualBudgetAdjustment.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_NullArgument()
        {
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_EmptyArgument()
        {
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetAdjustmentAsync_NoBudgetEntriesDataContract()
        {
            budgetEntriesDataContract = null;
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000111");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_IncorrectSourceCode()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999999");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BgteSource = "BC";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999999");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_NullAddOpr()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999998");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = null;
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999998");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_EmptyAddOpr()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999998");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999998");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetAdjustmentAsync_NullStaffIdSelectedForAlphaAddOper()
        {
            staffIds = null;
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000444");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "XYZ";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000444");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetAdjustmentAsync_EmptyStaffIdSelectedForAlphaAddOper()
        {
            staffIds = new string[] { };
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000444");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "XYZ";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000444");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetAdjustmentAsync_NoStaffRecord()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000444");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "ALPHALOGIN";
            staffDataContract = null;
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B000444");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_MoreThanOneStaff()
        {
            staffIds = new string[] { "0000001", "0000002" };
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999999");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "TWOSTAFF";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999999");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_EmptyLoginId()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999999");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BudgetEntriesAddopr = "";
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999999");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_NoStatus()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999999");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BgteStatus = null;
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999999");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentAsync_WrongStatus()
        {
            budgetAdjustmentDomainEntity = await testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B999999");
            ConvertDomainEntitiesIntoDataContracts();
            budgetEntriesDataContract.BgteStatus = new List<string>() { "X" };
            var actualBudgetAdjustment = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentAsync("B999999");
        }

        #endregion

        #region Tests for Budget Adjustment Summaries

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_Success_WithOnlyBudgetEntries()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();

            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_Success_WithOnlyrDraftsEntries()
        {
            budgetEntriesDataContracts = null;
            draftDataContracts = await testDraftRepository.GetDraftBudgetEntriesRecords();
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), draftDataContracts.Count());
            foreach (var summary in budgetAdjustmentSummaries)
            {
                Assert.IsNull(summary.BudgetAdjustmentNumber);
                var dataContractFoDraftBudgetEntry = draftDataContracts.FirstOrDefault(x => x.Recordkey == summary.DraftBudgetAdjustmentId);
                Assert.AreEqual(summary.DraftBudgetAdjustmentId, dataContractFoDraftBudgetEntry.Recordkey);
                Assert.AreEqual(summary.PersonId, dataContractFoDraftBudgetEntry.DraftBudgetEntriesAddopr);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.Draft);
                Assert.AreEqual(summary.TransactionDate, dataContractFoDraftBudgetEntry.DbgteTrDate);
                Assert.AreEqual(summary.Reason, dataContractFoDraftBudgetEntry.DbgteReason);
                if (dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation != null && dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation.Any())
                {
                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_Success_WithBothDraftsAndBudgetEntries()
        {
            draftDataContracts = await testDraftRepository.GetDraftBudgetEntriesRecords();
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();

            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count() + draftDataContracts.Count());

            var draftSummaries = budgetAdjustmentSummaries.Where(x => x.BudgetAdjustmentNumber == null).ToList();
            foreach (var summary in draftSummaries)
            {
                Assert.IsNull(summary.BudgetAdjustmentNumber);
                var dataContractFoDraftBudgetEntry = draftDataContracts.FirstOrDefault(x => x.Recordkey == summary.DraftBudgetAdjustmentId);
                Assert.AreEqual(summary.DraftBudgetAdjustmentId, dataContractFoDraftBudgetEntry.Recordkey);
                Assert.AreEqual(summary.PersonId, dataContractFoDraftBudgetEntry.DraftBudgetEntriesAddopr);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.Draft);
                Assert.AreEqual(summary.TransactionDate, dataContractFoDraftBudgetEntry.DbgteTrDate);
                Assert.AreEqual(summary.Reason, dataContractFoDraftBudgetEntry.DbgteReason);
                if (dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation != null && dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation.Any())
                {
                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }

            var baSummaries = budgetAdjustmentSummaries.Where(x => x.DraftBudgetAdjustmentId == null).ToList();
            foreach (var summary in baSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsSummaryAsync_NullParameter()
        {
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsSummaryAsync_EmptyParameter()
        {
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync("");
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_ListOfZeroDrafts()
        {
            draftDataContracts = new Collection<DraftBudgetEntries>();
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();

            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            var draftSummaries = budgetAdjustmentSummaries.Where(x => x.BudgetAdjustmentNumber == null).ToList();
            Assert.AreEqual(draftSummaries.Count(), 0);
            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_DraftContractKeyIsNull()
        {
            draftDataContracts = await testDraftRepository.GetDraftBudgetEntriesRecords();
            draftDataContracts[0].Recordkey = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();

            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count() + draftDataContracts.Count() - 1);

            var draftSummaries = budgetAdjustmentSummaries.Where(x => x.BudgetAdjustmentNumber == null).ToList();
            foreach (var summary in draftSummaries)
            {
                Assert.IsNull(summary.BudgetAdjustmentNumber);
                var dataContractFoDraftBudgetEntry = draftDataContracts.FirstOrDefault(x => x.Recordkey == summary.DraftBudgetAdjustmentId);
                Assert.AreEqual(summary.DraftBudgetAdjustmentId, dataContractFoDraftBudgetEntry.Recordkey);
                Assert.AreEqual(summary.PersonId, dataContractFoDraftBudgetEntry.DraftBudgetEntriesAddopr);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.Draft);
                Assert.AreEqual(summary.TransactionDate, dataContractFoDraftBudgetEntry.DbgteTrDate);
                Assert.AreEqual(summary.Reason, dataContractFoDraftBudgetEntry.DbgteReason);
                if (dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation != null && dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation.Any())
                {
                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }

            var baSummaries = budgetAdjustmentSummaries.Where(x => x.DraftBudgetAdjustmentId == null).ToList();
            foreach (var summary in baSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_BudgetEntrytContractKeyIsNull()
        {
            draftDataContracts = await testDraftRepository.GetDraftBudgetEntriesRecords();
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].Recordkey = null;

            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count() + draftDataContracts.Count() - 1);

            var draftSummaries = budgetAdjustmentSummaries.Where(x => x.BudgetAdjustmentNumber == null).ToList();
            foreach (var summary in draftSummaries)
            {
                Assert.IsNull(summary.BudgetAdjustmentNumber);
                var dataContractFoDraftBudgetEntry = draftDataContracts.FirstOrDefault(x => x.Recordkey == summary.DraftBudgetAdjustmentId);
                Assert.AreEqual(summary.DraftBudgetAdjustmentId, dataContractFoDraftBudgetEntry.Recordkey);
                Assert.AreEqual(summary.PersonId, dataContractFoDraftBudgetEntry.DraftBudgetEntriesAddopr);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.Draft);
                Assert.AreEqual(summary.TransactionDate, dataContractFoDraftBudgetEntry.DbgteTrDate);
                Assert.AreEqual(summary.Reason, dataContractFoDraftBudgetEntry.DbgteReason);
                if (dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation != null && dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation.Any())
                {
                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }

            var baSummaries = budgetAdjustmentSummaries.Where(x => x.DraftBudgetAdjustmentId == null).ToList();
            foreach (var summary in baSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync_DraftContractIsNull()
        {
            budgetEntriesDataContracts = new Collection<BudgetEntries>();
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync__DraftReasonIsNull()
        {
            budgetEntriesDataContracts = null;
            draftDataContracts = await testDraftRepository.GetDraftBudgetEntriesRecords();
            draftDataContracts[0].DbgteReason = null;
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), draftDataContracts.Count());
            Assert.AreEqual(budgetAdjustmentSummaries.First().Reason, " ");
            foreach (var summary in budgetAdjustmentSummaries)
            {
                Assert.IsNull(summary.BudgetAdjustmentNumber);
                var dataContractFoDraftBudgetEntry = draftDataContracts.FirstOrDefault(x => x.Recordkey == summary.DraftBudgetAdjustmentId);
                Assert.AreEqual(summary.DraftBudgetAdjustmentId, dataContractFoDraftBudgetEntry.Recordkey);
                Assert.AreEqual(summary.PersonId, dataContractFoDraftBudgetEntry.DraftBudgetEntriesAddopr);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.Draft);
                Assert.AreEqual(summary.TransactionDate, dataContractFoDraftBudgetEntry.DbgteTrDate);
                if (dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation != null && dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation.Any())
                {
                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractFoDraftBudgetEntry.DbgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.DbgteDebitAssocMember.HasValue ? associationMember.DbgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync__BudgetEntryReasonIsBlank()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].BgteDataEntityAssociation.First().BgteDescriptionAssocMember = null;
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count(), budgetEntriesDataContracts.Count());
            Assert.AreEqual(budgetAdjustmentSummaries.First().Reason, " ");
            foreach (var summary in budgetAdjustmentSummaries)
            {
                Assert.IsNull(summary.DraftBudgetAdjustmentId);
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                Assert.AreEqual(summary.Status, status);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                {
                    // Validate the reason against the first description in the association.
                    if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                    {
                        Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                    }
                    else
                    {
                        Assert.AreEqual(summary.Reason, " ");
                    }

                    // Validate the ToAmount.
                    decimal contractToAmount = 0;
                    foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                    {
                        if (associationMember != null)
                        {
                            var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                            contractToAmount += toAmount;
                        }
                    }
                    Assert.AreEqual(summary.ToAmount, contractToAmount);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsSummaryAsync__NoStaffRecord()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].BgteDataEntityAssociation.First().BgteDescriptionAssocMember = null;
            string personId = "0000001";
            staffDataContract = null;
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsSummaryAsync__NoStaffLogin()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].BgteDataEntityAssociation.First().BgteDescriptionAssocMember = null;
            string personId = "0000001";
            staffDataContract.StaffLoginId = null;
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync__BudgetEntryIncorrectStatus()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].BgteStatus = new List<string> { "x" };
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count() + 1, budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentSummaries)
            {
                if (summary.BudgetAdjustmentNumber != budgetEntriesDataContracts[0].Recordkey)
                {
                    Assert.IsNull(summary.DraftBudgetAdjustmentId);
                    var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                    Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                    var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                    Assert.AreEqual(summary.Status, status);
                    Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                    if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                    {
                        // Validate the reason against the first description in the association.
                        if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                        {
                            Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                        }
                        else
                        {
                            Assert.AreEqual(summary.Reason, " ");
                        }

                        // Validate the ToAmount.
                        decimal contractToAmount = 0;
                        foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                        {
                            if (associationMember != null)
                            {
                                var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                                contractToAmount += toAmount;
                            }
                        }
                        Assert.AreEqual(summary.ToAmount, contractToAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsSummaryAsync__NoStatus()
        {
            draftDataContracts = null;
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetBudgetEntriesRecords();
            budgetEntriesDataContracts[0].BgteStatus = null;
            string personId = "0000001";
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "LOGINFORID0000001";
            budgetAdjustmentSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentSummaries.Count() + 1, budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentSummaries)
            {
                if (summary.BudgetAdjustmentNumber != budgetEntriesDataContracts[0].Recordkey)
                {
                    Assert.IsNull(summary.DraftBudgetAdjustmentId);
                    var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                    Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                    var status = GetBudgetAdjustmentDataContractStatus(dataContractForBudgetEntry.BgteStatEntityAssociation.First().BgteStatusAssocMember);
                    Assert.AreEqual(summary.Status, status);
                    Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                    if (dataContractForBudgetEntry.BgteDataEntityAssociation != null && dataContractForBudgetEntry.BgteDataEntityAssociation.Any())
                    {
                        // Validate the reason against the first description in the association.
                        if (!string.IsNullOrEmpty(dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember))
                        {
                            Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);
                        }
                        else
                        {
                            Assert.AreEqual(summary.Reason, " ");
                        }

                        // Validate the ToAmount.
                        decimal contractToAmount = 0;
                        foreach (var associationMember in dataContractForBudgetEntry.BgteDataEntityAssociation)
                        {
                            if (associationMember != null)
                            {
                                var toAmount = associationMember.BgteDebitAssocMember.HasValue ? associationMember.BgteDebitAssocMember.Value : 0m;
                                contractToAmount += toAmount;
                            }
                        }
                        Assert.AreEqual(summary.ToAmount, contractToAmount);
                    }
                }
            }
        }

        #endregion

        #region Tests for Budget Adjustment Pending Approval Summaries

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_Success()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);
                Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                Assert.IsNull(summary.InitiatorLoginId);

                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(dataContractForBudgetEntry.BudgetEntriesAddopr))];
                Assert.AreEqual(summary.InitiatorName, initiatorName);

                // Validate the reason against the first description in the association.
                Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NullPersonId()
        {
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_EmptyPersonId()
        {
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NullStaffRecord()
        {
            staffDataContract.Recordkey = "0000001";
            staffDataContract = null;
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NullStaffLoginId()
        {
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = null;
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NoSummariesReturned()
        {
            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "XYZ";
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), 0);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NullRecordKey()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            budgetEntriesDataContracts[0].Recordkey = null;
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count() - 1);
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);
                Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                Assert.IsNull(summary.InitiatorLoginId);

                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(dataContractForBudgetEntry.BudgetEntriesAddopr))];
                Assert.AreEqual(summary.InitiatorName, initiatorName);

                // Validate the reason against the first description in the association.
                Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_OneIsComplete()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            budgetEntriesDataContracts[0].BgteStatus = new List<string> { "C" };
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count() - 1);
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);
                Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                Assert.IsNull(summary.InitiatorLoginId);

                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(dataContractForBudgetEntry.BudgetEntriesAddopr))];
                Assert.AreEqual(summary.InitiatorName, initiatorName);

                // Validate the reason against the first description in the association.
                Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_OneDoesNotHaveTheApprover()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            budgetEntriesDataContracts[0].BgteNextApprovalIds = new List<string>();
            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count() - 1);
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);
                Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                Assert.IsNull(summary.InitiatorLoginId);

                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(dataContractForBudgetEntry.BudgetEntriesAddopr))];
                Assert.AreEqual(summary.InitiatorName, initiatorName);

                // Validate the reason against the first description in the association.
                Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_NoReason()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            budgetEntriesDataContracts[0].BgteDataEntityAssociation = new List<BudgetEntriesBgteData>();

            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);
                Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                Assert.IsNull(summary.InitiatorLoginId);

                if (summary.BudgetAdjustmentNumber == budgetEntriesDataContracts[0].Recordkey)
                {
                    Assert.AreEqual(summary.Reason, null);
                }
                else
                {
                    Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation[0].BgteDescriptionAssocMember);
                }
                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(dataContractForBudgetEntry.BudgetEntriesAddopr))];
                Assert.AreEqual(summary.InitiatorName, initiatorName); ;

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount); Assert.AreEqual(summary.ToAmount, contractToAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentsPendingApprovalSummaryAsync_AddOperIsLogin()
        {
            budgetEntriesDataContracts = await testBudgetAdjustmentRepository.GetNotApprovedBudgetEntriesRecords();

            staffDataContract.Recordkey = "0000001";
            staffDataContract.StaffLoginId = "TGL";
            staffDataContracts.Add(staffDataContract);
            budgetEntriesDataContracts[0].BudgetEntriesAddopr = "TGL";
            getNamesforIdsResponse.IoPersonIds = new List<string>() { "0000001", "0010475", "0013272" };
            getNamesforIdsResponse.IoHierarchies = new List<string>() { "PREFERRED", "PREFERRED", "PREFERRED" };
            getNamesforIdsResponse.OutPersonNames = new List<string>() { "Name for 0000001", "Name for 0010475", "Name for 0013272" };

            budgetAdjustmentPendingApprovalSummaries = await actualBudgetAdjustmentRepository2.GetBudgetAdjustmentsPendingApprovalSummaryAsync(personId);

            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaries.Count(), budgetEntriesDataContracts.Count());
            foreach (var summary in budgetAdjustmentPendingApprovalSummaries)
            {
                var dataContractForBudgetEntry = budgetEntriesDataContracts.FirstOrDefault(x => x.Recordkey == summary.BudgetAdjustmentNumber);
                Assert.AreEqual(summary.BudgetAdjustmentNumber, dataContractForBudgetEntry.Recordkey);
                Assert.AreEqual(summary.Status, BudgetEntryStatus.NotApproved);
                Assert.AreEqual(summary.TransactionDate, dataContractForBudgetEntry.BgteTrDate);

                if (summary.InitiatorLoginId != null)
                {
                    Assert.AreEqual(summary.InitiatorId, staffDataContract.Recordkey);
                    Assert.AreEqual(summary.InitiatorLoginId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                }
                else
                {
                    Assert.AreEqual(summary.InitiatorId, dataContractForBudgetEntry.BudgetEntriesAddopr);
                    Assert.IsNull(summary.InitiatorLoginId);
                }

                // Validate the reason against the first description in the association.
                Assert.AreEqual(summary.Reason, dataContractForBudgetEntry.BgteDataEntityAssociation.FirstOrDefault().BgteDescriptionAssocMember);

                var initiatorName = getNamesforIdsResponse.OutPersonNames[getNamesforIdsResponse.IoPersonIds.FindIndex(x => x.Equals(summary.InitiatorId))];
                Assert.AreEqual(summary.InitiatorName, initiatorName);

                // Validate the ToAmount.
                decimal? contractToAmount = 0;
                contractToAmount = dataContractForBudgetEntry.BgteDataEntityAssociation.Sum(am => am.BgteDebitAssocMember);
                Assert.AreEqual(summary.ToAmount, contractToAmount); Assert.AreEqual(summary.ToAmount, contractToAmount);
            }
        }

        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            budgetAdjustmentResponse = new TxUpdateBudgetAdjustmentResponse()
            {
                ABudgetEntryId = this.journalNumber,
                AError = "",
                AlMessage = new List<string>()
            };
            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateBudgetAdjustmentRequest, TxUpdateBudgetAdjustmentResponse>(It.IsAny<TxUpdateBudgetAdjustmentRequest>())).Returns(() =>
            {
                return Task.FromResult(budgetAdjustmentResponse);
            });

            getNamesforIdsResponse = new GetHierarchyNamesForIdsResponse()
            {
                IoPersonIds = new List<string>() { "0010475", "0013272" },
                IoHierarchies = new List<string>() { "PREFERRED", "PREFERRED" },
                OutPersonNames = new List<string>() { "Anjana Tesa Gutierrez", "Teresa P Castro" }
            };

            transManagerMock.Setup(txio => txio.ExecuteAsync<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(It.IsAny<GetHierarchyNamesForIdsRequest>())).Returns(() =>
            {
                return Task.FromResult(getNamesforIdsResponse);
            });

            deleteDraftResponse = new TxDeleteDraftBudgetAdjustmentResponse()
            {
                AErrorCode = "",
                AErrorMessage = ""
            };

            transManagerMock.Setup(tio => tio.ExecuteAsync<TxDeleteDraftBudgetAdjustmentRequest, TxDeleteDraftBudgetAdjustmentResponse>(It.IsAny<TxDeleteDraftBudgetAdjustmentRequest>())).Returns(() =>
            {
                return Task.FromResult(deleteDraftResponse);
            });

            // Mock ReadRecord to return a pre-defined Opers data contract.
            // Mock bulk read UT.OPERS bulk read
            opersResponse = new Collection<DataContracts.Opers>()
                {
                    //new DataContracts.Opers()
                    //{
                    //     "AJK"
                    //    Recordkey = "AJK", SysPersonId = "0000001", SysUserName = "Andy Kleehammer"
                    //},
                    //new DataContracts.Opers()
                    //{
                    //     "GTT"
                    //    Recordkey = "GTT",  SysPersonId = "0000002", SysUserName = "Gary Thorne"
                    //},
                    //new DataContracts.Opers()
                    //{
                    //     "TGL"
                    //    Recordkey = "TGL",  SysPersonId = "0000003", SysUserName = "Teresa Longerbeam"
                    //},
                    new DataContracts.Opers()
                    {
                        // "AER"
                        Recordkey = "AER",  SysPersonId = "0000004", SysUserName = "Aimee Rodgers"
                    },
                    new DataContracts.Opers()
                    {
                        // "MEL"
                        Recordkey = "MEL",  SysPersonId = "0000005", SysUserName = "Maurlin Lowks"
                    }
                };
            dataReaderMock.Setup<Task<Collection<DataContracts.Opers>>>(acc => acc.BulkReadRecordAsync<DataContracts.Opers>("UT.OPERS", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(opersResponse);
            });

            // Mock any Budget Entries read that is NOT for B123456
            dataReaderMock.Setup<Task<BudgetEntries>>(dc => dc.ReadRecordAsync<BudgetEntries>(It.IsNotIn("B123456"), true)).Returns(() =>
            {
                return Task.FromResult(budgetEntriesDataContract);
            });

            // Mock ReadRecordAsync to read a budget entries RECORD with a key other of B123456.
            dataReaderMock.Setup<Task<BudgetEntries>>(dc => dc.ReadRecordAsync<BudgetEntries>("B123456", true)).Returns(() =>
            {
                return Task.FromResult(budgetEntriesDataContract2);
            });

            // Mock the selection of DRAFT.BUDGET.ENTRIES record.
            dataReaderMock.Setup(li => li.SelectAsync("DRAFT.BUDGET.ENTRIES", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(draftBudgetEntriesIds);
            });

            // Mock the BulkReadRecord to return a list of DRAFT.BUDGET.ENTRIES data contracts.
            dataReaderMock.Setup(dc => dc.BulkReadRecordAsync<DraftBudgetEntries>("DRAFT.BUDGET.ENTRIES", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(draftDataContracts);
            });

            // Mock the selection of BUDGET.ENTRIES record.
            dataReaderMock.Setup(li => li.SelectAsync("BUDGET.ENTRIES", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(budgetEntriesIds);
            });

            // Mock the BulkReadRecord to return a list of BUDGET.ENTRIES data contracts.
            dataReaderMock.Setup(dc => dc.BulkReadRecordAsync<BudgetEntries>("BUDGET.ENTRIES", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(budgetEntriesDataContracts);
            });

            // Mock the selection of staff records.
            dataReaderMock.Setup(x => x.SelectAsync("STAFF", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(staffIds);
            });

            // Mock the selection of staff records for budget adjustment pending approval summary.
            dataReaderMock.Setup(li => li.SelectAsync("STAFF", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(() =>
            {
                return Task.FromResult(pendingApprovalStaffIds);
            });

            dataReaderMock.Setup(sr => sr.BulkReadRecordAsync<Staff>("STAFF", It.IsAny<string[]>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContracts);
            });
            // Mock ReadRecordAsync to read a staff record for GetBudgetAdjustmentsSummaryAsync and GetBudgetAdjustmentsPendingApprovalSummaryAsync.
            dataReaderMock.Setup<Task<Staff>>(dc => dc.ReadRecordAsync<Staff>("STAFF", personId, true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });

            //Mock ReadRecordAsync to read a staff record for GetBudgetAdjustmentAsync.
            dataReaderMock.Setup<Task<Staff>>(dc => dc.ReadRecordAsync<Staff>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(staffDataContract);
            });
        }

        private void ConvertDomainEntitiesIntoDataContracts()
        {
            // Convert the Budget Adjustment domain entity into a data contract object.
            budgetEntriesDataContract.Recordkey = budgetAdjustmentDomainEntity.Id;
            budgetEntriesDataContract.BgteAuthor = budgetAdjustmentDomainEntity.PersonId;
            budgetEntriesDataContract.BgteAuthor = budgetAdjustmentDomainEntity.Initiator;
            budgetEntriesDataContract.BgteSource = "BU";
            budgetEntriesDataContract.BgteTrDate = budgetAdjustmentDomainEntity.TransactionDate;
            budgetEntriesDataContract.BgteComments = budgetAdjustmentDomainEntity.Comments;

            budgetEntriesDataContract.BgteStatus = new List<string>();
            switch (budgetAdjustmentDomainEntity.Status)
            {
                case BudgetEntryStatus.Complete:
                    budgetEntriesDataContract.BgteStatus.Add("C");
                    break;
                case BudgetEntryStatus.NotApproved:
                    budgetEntriesDataContract.BgteStatus.Add("N");
                    break;
                case BudgetEntryStatus.Unfinished:
                    budgetEntriesDataContract.BgteStatus.Add("U");
                    break;
                default:
                    throw new Exception("Invalid status specified in BudgetAdjustmentRepositoryTests");
            }
            budgetEntriesDataContract.BudgetEntriesAddopr = "0000001";

            budgetEntriesDataContract.BgteDataEntityAssociation = new List<BudgetEntriesBgteData>();

            foreach (var adjustmentLine in budgetAdjustmentDomainEntity.AdjustmentLines)
            {
                budgetEntriesDataContract.BgteDataEntityAssociation.Add(new BudgetEntriesBgteData()
                {
                    BgteGlNoAssocMember = adjustmentLine.GlNumber,
                    BgteDescriptionAssocMember = budgetAdjustmentDomainEntity.Reason,
                    BgteDebitAssocMember = adjustmentLine.ToAmount,
                    BgteCreditAssocMember = adjustmentLine.FromAmount
                });
            };
        }

        private BudgetEntryStatus GetBudgetAdjustmentDataContractStatus(string status)
        {
            // Translate and assign the status. Budget Adjustments can have one of several statuses.
            BudgetEntryStatus budgetEntryStatus = new BudgetEntryStatus();

            // Get the first status in the list of budget entry statuses, and check that it has a value.
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToUpper())
                {
                    case "U":
                        budgetEntryStatus = BudgetEntryStatus.Unfinished;
                        break;
                    case "N":
                        budgetEntryStatus = BudgetEntryStatus.NotApproved;
                        break;
                    case "C":
                        budgetEntryStatus = BudgetEntryStatus.Complete;
                        break;
                    default:
                        // if we get here, we have corrupt data.
                        throw new ApplicationException("Invalid status for budget adjustment: " + status);
                }
            }
            else
            {
                throw new ApplicationException("Missing status for budget adjustment: " + status);
            }
            return budgetEntryStatus;
        }

        #endregion
    }
}