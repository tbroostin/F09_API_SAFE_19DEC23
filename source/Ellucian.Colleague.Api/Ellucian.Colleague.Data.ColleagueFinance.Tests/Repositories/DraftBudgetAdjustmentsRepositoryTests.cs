// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class DraftBudgetAdjustmentsRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        private DraftBudgetAdjustmentsRepository actualRepository;
        private DraftBudgetEntries draftBudgetEntryRecord;
        private DraftBudgetAdjustment draftBudgetAdjustmentInput;
        private TxUpdateDraftBudgetAdjustmentResponse draftBudgetAdjustmentResponse;
        private TxDeleteDraftBudgetAdjustmentResponse deleteDraftResponse;
        private string draftNumber = "B1234567";
        private List<string> majorComponentStartPositions = new List<string>() { "1", "4", "7", "10", "13", "19" };

        // Create a list of DRAF.BUDGET.ENTRIES data contracts.
        public List<DraftBudgetEntries> draftBudgetEntriesRecords = new List<DraftBudgetEntries>()
        {
            new DraftBudgetEntries
            {
                Recordkey = "1",
                DbgteReason = "Draft budget adjutment 1 reason",
                DbgteTrDate = DateTime.Now.AddDays(1),
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
            },
            new DraftBudgetEntries
            {
                Recordkey = "2",
                DbgteReason = "Draft budget adjutment 2 reason",
                DbgteTrDate = DateTime.Now.AddDays(2),
                DbgteAuthor = "Initiator 2",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 22m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "3",
                DbgteReason = "Draft budget adjutment 3 reason",
                DbgteTrDate = DateTime.Now.AddDays(3),
                DbgteAuthor = "",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 33.33m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "4",
                DbgteReason = "Draft budget adjutment 4 reason",
                DbgteTrDate = DateTime.Now.AddDays(4),
                DbgteAuthor = "Initiator 4",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = null,
                        DbgteCreditAssocMember = null,
                        DbgteDebitAssocMember = null
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_54444",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            },
            new DraftBudgetEntries
            {
                Recordkey = "5",
                DbgteReason = "Draft budget adjutment 5 reason",
                DbgteTrDate = DateTime.Now.AddDays(5),
                DbgteAuthor = "Initiator 5",
                DraftBudgetEntriesAddopr = "0000001",
                DbgteDataEntityAssociation = new List<DraftBudgetEntriesDbgteData>()
                {
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "",
                        DbgteCreditAssocMember = 55m,
                        DbgteDebitAssocMember = 0m
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_55555",
                        DbgteCreditAssocMember = 56m,
                        DbgteDebitAssocMember = 0m
                    },
                    new DraftBudgetEntriesDbgteData
                    {
                        DbgteGlNoAssocMember = "11_00_01_00_11111_55557",
                        DbgteCreditAssocMember = 0m,
                        DbgteDebitAssocMember = 0m
                    }
                }
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            actualRepository = new DraftBudgetAdjustmentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            draftBudgetEntryRecord = new DraftBudgetEntries();
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            draftBudgetEntryRecord = null;
            draftBudgetAdjustmentInput = null;
            deleteDraftResponse = null;
        }
        #endregion

        #region CreateAsync Tests
        [TestMethod]
        public async Task CreateAsync_NoNextApprovers_Success()
        {
            var transactionDate = DateTime.Now;
            var initiator = "Frank N. Stein";
            var reason = "more money";
            var comments = "additional justificaton";
            var draftId = "";
            var adjustmentLines = new List<DraftAdjustmentLine>()
            {
                new DraftAdjustmentLine() { GlNumber = "1",  FromAmount = 100.00m,  ToAmount = 0.00m },
                new DraftAdjustmentLine() { GlNumber = "2",  FromAmount = 0m,  ToAmount = 100.00m }
            };
            var adjustmentInputEntity = new DraftBudgetAdjustment(reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLines;
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.TransactionDate = transactionDate;
            adjustmentInputEntity.Id = draftId;
            var adjustmentOutputEntity = await actualRepository.SaveAsync(adjustmentInputEntity, majorComponentStartPositions);

            adjustmentOutputEntity.TransactionDate = transactionDate;
            adjustmentOutputEntity.Initiator = initiator;
            adjustmentOutputEntity.Comments = comments;

            Assert.AreEqual(this.draftNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(0, adjustmentOutputEntity.ErrorMessages.Count);
            Assert.AreEqual(0, adjustmentOutputEntity.NextApprovers.Count);

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
        public async Task CreateAsync_NullGlAccounts_Success()
        {
            var transactionDate = DateTime.Now;
            var initiator = "Frank N. Stein";
            var reason = "more money";
            var comments = "additional justificaton";
            var draftId = "";
            var adjustmentLines = new List<DraftAdjustmentLine>()
            {
                new DraftAdjustmentLine() { GlNumber = null,  FromAmount = 0,  ToAmount = 5.00m }
            };
            var adjustmentInputEntity = new DraftBudgetAdjustment(reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLines;
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.TransactionDate = transactionDate;
            adjustmentInputEntity.Id = draftId;
            var adjustmentOutputEntity = await actualRepository.SaveAsync(adjustmentInputEntity, majorComponentStartPositions);

            adjustmentOutputEntity.TransactionDate = transactionDate;
            adjustmentOutputEntity.Initiator = initiator;
            adjustmentOutputEntity.Comments = comments;

            Assert.AreEqual(this.draftNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(0, adjustmentOutputEntity.ErrorMessages.Count);
            Assert.AreEqual(0, adjustmentOutputEntity.NextApprovers.Count);

            Assert.AreEqual(1, adjustmentOutputEntity.AdjustmentLines.Count);
            Assert.AreEqual(adjustmentInputEntity.AdjustmentLines[0].GlNumber, adjustmentOutputEntity.AdjustmentLines[0].GlNumber);
            Assert.AreEqual(adjustmentInputEntity.AdjustmentLines[0].FromAmount, adjustmentOutputEntity.AdjustmentLines[0].FromAmount);
            Assert.AreEqual(adjustmentInputEntity.AdjustmentLines[0].ToAmount, adjustmentOutputEntity.AdjustmentLines[0].ToAmount);
        }

        [TestMethod]
        public async Task CreateAsync_NoNextApprovers_NoAdjustmentLines_Success()
        {
            var transactionDate = DateTime.Now;
            var initiator = "Frank N. Stein";
            var reason = "more money";
            var comments = "additional justificaton";
            var draftId = "";
            var adjustmentLines = new List<DraftAdjustmentLine>();
            var adjustmentInputEntity = new DraftBudgetAdjustment(reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLines;
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.TransactionDate = transactionDate;
            adjustmentInputEntity.Id = draftId;
            adjustmentInputEntity.NextApprovers = null;
            var adjustmentOutputEntity = await actualRepository.SaveAsync(adjustmentInputEntity, majorComponentStartPositions);

            adjustmentOutputEntity.TransactionDate = transactionDate;
            adjustmentOutputEntity.Initiator = initiator;
            adjustmentOutputEntity.Comments = comments;

            Assert.AreEqual(this.draftNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(0, adjustmentOutputEntity.ErrorMessages.Count);
            Assert.AreEqual(0, adjustmentOutputEntity.NextApprovers.Count);
            Assert.AreEqual(0, adjustmentOutputEntity.AdjustmentLines.Count);
        }

        [TestMethod]
        public async Task CreateAsync_WithNextApprovers_Success()
        {
            var transactionDate = DateTime.Now;
            var initiator = "Frank N. Stein";
            var reason = "more money";
            var comments = "additional justificaton";
            var draftId = "";
            var adjustmentLines = new List<DraftAdjustmentLine>()
            {
                new DraftAdjustmentLine() { GlNumber = "1",  FromAmount = 100.00m,  ToAmount = 0.00m },
                new DraftAdjustmentLine() { GlNumber = "2",  FromAmount = 0m,  ToAmount = 100.00m }
            };
            var nextApprovers = new List<NextApprover>()
            {
                new NextApprover("TGL"),
                new NextApprover("GTT")
            };

            var adjustmentInputEntity = new DraftBudgetAdjustment(reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLines;
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.TransactionDate = transactionDate;
            adjustmentInputEntity.Id = draftId;
            adjustmentInputEntity.NextApprovers = nextApprovers;

            var adjustmentOutputEntity = await actualRepository.SaveAsync(adjustmentInputEntity, majorComponentStartPositions);
            adjustmentOutputEntity.TransactionDate = transactionDate;
            adjustmentOutputEntity.Initiator = initiator;
            adjustmentOutputEntity.Comments = comments;

            Assert.AreEqual(this.draftNumber, adjustmentOutputEntity.Id);
            Assert.AreEqual(transactionDate, adjustmentOutputEntity.TransactionDate);
            Assert.AreEqual(initiator, adjustmentOutputEntity.Initiator);
            Assert.AreEqual(reason, adjustmentOutputEntity.Reason);
            Assert.AreEqual(comments, adjustmentOutputEntity.Comments);
            Assert.AreEqual(0, adjustmentOutputEntity.ErrorMessages.Count);

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
            var initiator = "Frank N. Stein";
            var reason = "more money";
            var comments = "additional justificaton";
            var adjustmentLines = new List<DraftAdjustmentLine>()
            {
                new DraftAdjustmentLine() { GlNumber = "1",  FromAmount = 100m,  ToAmount = 0m },
                new DraftAdjustmentLine() { GlNumber = "2",  FromAmount = 0m,  ToAmount = 100m }
            };

            // Set up the failure response.
            draftBudgetAdjustmentResponse.ADraftBudgetEntryId = "";
            draftBudgetAdjustmentResponse.AlErrorMessages = new List<string>() { "One or more of the GL accounts has insufficient funds.", "Security not assigned for GL account 1234." };

            var adjustmentInputEntity = new DraftBudgetAdjustment(reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLines;
            adjustmentInputEntity.Initiator = initiator;
            adjustmentInputEntity.Comments = comments;
            adjustmentInputEntity.TransactionDate = transactionDate;
            var adjustmentOutputEntity = await actualRepository.SaveAsync(adjustmentInputEntity, majorComponentStartPositions);

            foreach (var errorMessage in draftBudgetAdjustmentResponse.AlErrorMessages)
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

        #region Test for GET

        [TestMethod]
        public async Task GetAsync_Success_NoAdjustmentLines()
        {
            draftBudgetEntryRecord = await GetAsync("1");
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("1");

            Assert.AreEqual(draftBudgetEntryRecord.Recordkey, draftBudgetAdjustmentEntity.Id);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteAuthor, draftBudgetAdjustmentEntity.Initiator);
            Assert.AreEqual(draftBudgetEntryRecord.DraftBudgetEntriesAddopr, draftBudgetAdjustmentEntity.PersonId);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteReason, draftBudgetAdjustmentEntity.Reason);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteTrDate, draftBudgetAdjustmentEntity.TransactionDate);

            Assert.AreEqual(draftBudgetEntryRecord.DbgteDataEntityAssociation.Count(), draftBudgetAdjustmentEntity.AdjustmentLines.Count());

            foreach (var adjustmentLine in draftBudgetEntryRecord.DbgteDataEntityAssociation)
            {
                Assert.IsTrue(draftBudgetAdjustmentEntity.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.DbgteCreditAssocMember
                && x.GlNumber == adjustmentLine.DbgteGlNoAssocMember
                && x.ToAmount == adjustmentLine.DbgteDebitAssocMember));
            }
        }

        [TestMethod]
        public async Task GetAsync_Success_OneAdjustmentLine_NoGlNumber_WithCredit()
        {
            draftBudgetEntryRecord = await GetAsync("2");
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("2");

            Assert.AreEqual(draftBudgetEntryRecord.Recordkey, draftBudgetAdjustmentEntity.Id);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteAuthor, draftBudgetAdjustmentEntity.Initiator);
            Assert.AreEqual(draftBudgetEntryRecord.DraftBudgetEntriesAddopr, draftBudgetAdjustmentEntity.PersonId);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteReason, draftBudgetAdjustmentEntity.Reason);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteTrDate, draftBudgetAdjustmentEntity.TransactionDate);

            Assert.AreEqual(draftBudgetEntryRecord.DbgteDataEntityAssociation.Count(), draftBudgetAdjustmentEntity.AdjustmentLines.Count());

            foreach (var adjustmentLine in draftBudgetEntryRecord.DbgteDataEntityAssociation)
            {
                Assert.IsTrue(draftBudgetAdjustmentEntity.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.DbgteCreditAssocMember
                && x.GlNumber == adjustmentLine.DbgteGlNoAssocMember
                && x.ToAmount == adjustmentLine.DbgteDebitAssocMember));
            }
        }

        [TestMethod]
        public async Task GetAsync_Success_OneAdjustmentLine_NoGlNumber_WithDebit()
        {
            draftBudgetEntryRecord = await GetAsync("3");
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("3");

            Assert.AreEqual(draftBudgetEntryRecord.Recordkey, draftBudgetAdjustmentEntity.Id);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteAuthor, draftBudgetAdjustmentEntity.Initiator);
            Assert.AreEqual(draftBudgetEntryRecord.DraftBudgetEntriesAddopr, draftBudgetAdjustmentEntity.PersonId);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteReason, draftBudgetAdjustmentEntity.Reason);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteTrDate, draftBudgetAdjustmentEntity.TransactionDate);

            Assert.AreEqual(draftBudgetEntryRecord.DbgteDataEntityAssociation.Count(), draftBudgetAdjustmentEntity.AdjustmentLines.Count());

            foreach (var adjustmentLine in draftBudgetEntryRecord.DbgteDataEntityAssociation)
            {
                Assert.IsTrue(draftBudgetAdjustmentEntity.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.DbgteCreditAssocMember
                && x.GlNumber == adjustmentLine.DbgteGlNoAssocMember
                && x.ToAmount == adjustmentLine.DbgteDebitAssocMember));
            }
        }

        [TestMethod]
        public async Task GetAsync_Success_OneAdjustmentLine_MoreThanOneAdjustmentLine()
        {
            draftBudgetEntryRecord = await GetAsync("5");
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("5");

            Assert.AreEqual(draftBudgetEntryRecord.Recordkey, draftBudgetAdjustmentEntity.Id);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteAuthor, draftBudgetAdjustmentEntity.Initiator);
            Assert.AreEqual(draftBudgetEntryRecord.DraftBudgetEntriesAddopr, draftBudgetAdjustmentEntity.PersonId);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteReason, draftBudgetAdjustmentEntity.Reason);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteTrDate, draftBudgetAdjustmentEntity.TransactionDate);

            Assert.AreEqual(draftBudgetEntryRecord.DbgteDataEntityAssociation.Count(), draftBudgetAdjustmentEntity.AdjustmentLines.Count());

            foreach (var adjustmentLine in draftBudgetEntryRecord.DbgteDataEntityAssociation)
            {
                Assert.IsTrue(draftBudgetAdjustmentEntity.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.DbgteCreditAssocMember
                && x.GlNumber == adjustmentLine.DbgteGlNoAssocMember
                && x.ToAmount == adjustmentLine.DbgteDebitAssocMember));
            }
        }

        [TestMethod]
        public async Task GetAsync_Success_OneAdjustmentLine_NullAdjustmentLine()
        {
            draftBudgetEntryRecord = await GetAsync("4");
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("4");

            Assert.AreEqual(draftBudgetEntryRecord.Recordkey, draftBudgetAdjustmentEntity.Id);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteAuthor, draftBudgetAdjustmentEntity.Initiator);
            Assert.AreEqual(draftBudgetEntryRecord.DraftBudgetEntriesAddopr, draftBudgetAdjustmentEntity.PersonId);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteReason, draftBudgetAdjustmentEntity.Reason);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteTrDate, draftBudgetAdjustmentEntity.TransactionDate);
            Assert.AreEqual(draftBudgetEntryRecord.DbgteDataEntityAssociation.Count(), draftBudgetAdjustmentEntity.AdjustmentLines.Count());

            // Remove the association member that is null.
            draftBudgetEntryRecord.DbgteDataEntityAssociation.Remove(draftBudgetEntryRecord.DbgteDataEntityAssociation[0]);

            foreach (var adjustmentLine in draftBudgetEntryRecord.DbgteDataEntityAssociation)
            {
                Assert.IsTrue(draftBudgetAdjustmentEntity.AdjustmentLines.Any(x =>
                x.FromAmount == adjustmentLine.DbgteCreditAssocMember
                && x.GlNumber == adjustmentLine.DbgteGlNoAssocMember
                && x.ToAmount == adjustmentLine.DbgteDebitAssocMember));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullArgument()
        {
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_EmptyArgument()
        {
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBudgetAdjustmentAsync_NoBudgetEntriesDataContract()
        {
            draftBudgetEntryRecord = null;
            var draftBudgetAdjustmentEntity = await actualRepository.GetAsync("1");
        }

        #endregion

        #region SaveAsync

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task SaveAsync_ColleagueSessionExpiredTest()
        {
            draftBudgetAdjustmentInput = new DraftBudgetAdjustment("reason");
            draftBudgetAdjustmentInput.NextApprovers = new List<NextApprover>();
            draftBudgetAdjustmentInput.TransactionDate = DateTime.Now;
            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateDraftBudgetAdjustmentRequest, TxUpdateDraftBudgetAdjustmentResponse>(It.IsAny<TxUpdateDraftBudgetAdjustmentRequest>())).Throws(new ColleagueSessionExpiredException("timeout"));
            await actualRepository.SaveAsync(draftBudgetAdjustmentInput, new List<string>());
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task NoTransactionErrors_NoExceptionsThrownTest()
        {
            bool exceptionThrown = false;
            try
            {
                await actualRepository.DeleteAsync(draftNumber);
            }
            catch { exceptionThrown = true; }
            Assert.IsFalse(exceptionThrown);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullRecordId_ArgumentNullExceptionThrownTest()
        {
            await actualRepository.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NoRecordWithSpecifiedId_KeyNotFoundExceptionThrownTest()
        {
            deleteDraftResponse.AErrorCode = "MissingRecord";
            await actualRepository.DeleteAsync("xyz");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task TransactionError_ApplicationExceptionThrownTest()
        {
            deleteDraftResponse.AErrorCode = "any code";
            await actualRepository.DeleteAsync(draftNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueSessionExpiredException))]
        public async Task DeleteAsync_ColleagueSessionExpiredTest()
        {
            transManagerMock.Setup(tm => tm.ExecuteAsync<TxDeleteDraftBudgetAdjustmentRequest, TxDeleteDraftBudgetAdjustmentResponse>(It.IsAny<TxDeleteDraftBudgetAdjustmentRequest>())).Throws(new ColleagueSessionExpiredException("timeout"));
            await actualRepository.DeleteAsync("1");
        }

        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            draftBudgetAdjustmentResponse = new TxUpdateDraftBudgetAdjustmentResponse()
            {
                ADraftBudgetEntryId = this.draftNumber,
                AError = "",
                AlErrorMessages = new List<string>()
            };
            transManagerMock.Setup(tio => tio.ExecuteAsync<TxUpdateDraftBudgetAdjustmentRequest, TxUpdateDraftBudgetAdjustmentResponse>(It.IsAny<TxUpdateDraftBudgetAdjustmentRequest>())).Returns(() =>
            {
                return Task.FromResult(draftBudgetAdjustmentResponse);
            });

            deleteDraftResponse = new TxDeleteDraftBudgetAdjustmentResponse();
            transManagerMock.Setup(tm => tm.ExecuteAsync<TxDeleteDraftBudgetAdjustmentRequest, TxDeleteDraftBudgetAdjustmentResponse>(It.IsAny<TxDeleteDraftBudgetAdjustmentRequest>())).Returns(() =>
            {
                return Task.FromResult(deleteDraftResponse);
            });

            dataReaderMock.Setup<Task<DraftBudgetEntries>>(dc => dc.ReadRecordAsync<DraftBudgetEntries>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(draftBudgetEntryRecord);
            });
        }

        // Return a single DRAFT.BUDGET.ENTRIES record.
        private async Task<DraftBudgetEntries> GetAsync(string id)
        {
            var draftBudgetAdjustment = await Task.Run(() => draftBudgetEntriesRecords.FirstOrDefault(x => x.Recordkey == id));
            return draftBudgetAdjustment;
        }
        #endregion
    }
}