// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the JournalEntry class as well as the abstract classes it extends. When we
    /// implement Budget Entries there will be no need to write tests for the extended classes.
    /// </summary>
    [TestClass]
    public class JournalEntryTests
    {
        #region Initialize and Cleanup
        private TestJournalEntryRepository testJournalEntryRepository;
        private string personId = "1";

        [TestInitialize]
        public void Initialize()
        {
            testJournalEntryRepository = new TestJournalEntryRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            testJournalEntryRepository = null;
        }


        #endregion

        #region Constructor Tests
        [TestMethod]
        public async Task JournalEntry_VerifyInheritance()
        {
            var repoJournalEntry = await testJournalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
            var testJournalEntry = new JournalEntry(repoJournalEntry.Id, repoJournalEntry.Date, repoJournalEntry.Status, repoJournalEntry.Type, repoJournalEntry.EnteredDate, repoJournalEntry.EnteredByName);
            
            // BaseFinanceDocument
            Assert.AreEqual(repoJournalEntry.Id, testJournalEntry.Id, "Document ID should be initialized.");
            Assert.AreEqual(repoJournalEntry.Date, testJournalEntry.Date, "Document date should be initialized.");
            Assert.IsTrue(testJournalEntry.Approvers is ReadOnlyCollection<Approver>, "The document approvers should be the correct type.");
            Assert.IsTrue(testJournalEntry.Approvers.Count() == 0, "The document approvers list should have 0 items.");

            // LedgerEntryDocument
            Assert.AreEqual(repoJournalEntry.EnteredDate, testJournalEntry.EnteredDate, "Entered date should be initialized.");
            Assert.AreEqual(repoJournalEntry.EnteredByName, testJournalEntry.EnteredByName, "Entered By should be initialized.");
        }

        [TestMethod]
        public async Task JournalEntry_ConstructorInitialization()
        {
            var repoJournalEntry = await testJournalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
            var testJournalEntry = new JournalEntry(repoJournalEntry.Id, repoJournalEntry.Date, repoJournalEntry.Status, repoJournalEntry.Type, repoJournalEntry.EnteredDate, repoJournalEntry.EnteredByName);

            // JournalEntry constructor
            Assert.AreEqual(repoJournalEntry.Status, testJournalEntry.Status, "Status should be initialized.");
            Assert.AreEqual(repoJournalEntry.Type, testJournalEntry.Type, "Type should be initialized.");
            Assert.IsTrue(testJournalEntry.Items is ReadOnlyCollection<JournalEntryItem>, "Journal entry items list should be the correct type.");
            Assert.IsTrue(testJournalEntry.Items.Count() == 0, "Journal entry items list should have 0 items.");

            Assert.IsTrue(testJournalEntry.TotalCredits == 0, "TotalCredits should be initialized to 0.");
            Assert.IsTrue(testJournalEntry.TotalDebits == 0, "TotalDebits should be initialized to 0.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JournalEntryConstructor_NullEnteredBy()
        {
            JournalEntry journalEntry = new JournalEntry("J000001", new DateTime(2015, 1, 10), JournalEntryStatus.Complete, JournalEntryType.General, new DateTime(2015, 2, 10), null);
        }

        #endregion
        
        #region AddLineItem Tests
        [TestMethod]
        public async Task AddLineItem_Success()
        {
            var journalEntry = await testJournalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
            var ItemCount = journalEntry.Items.Count();
            string description = "Adding another item",
                   glAccount = "11_00_01_00_33333_54400";

            var itemCount = journalEntry.Items.Count();
            journalEntry.AddItem(new JournalEntryItem(description, glAccount));
            var item = journalEntry.Items.Where(x => x.Description == "Adding another item").FirstOrDefault();

            Assert.IsTrue(journalEntry.Items.Count() == itemCount + 1, "Item count should reflect the new item.");
            Assert.AreEqual(description, item.Description, "The description should match.");
            Assert.AreEqual(glAccount, item.GlAccountNumber, "The GL number should match.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddLineItem_NullLineItem()
        {
            var journalEntry = await testJournalEntryRepository.GetJournalEntryAsync("J000001", personId, GlAccessLevel.Full_Access, null);
            journalEntry.AddItem(null);
        }
        
        [TestMethod]
        public void AddLineItem_DuplicateItemAllowed()
        {
            string number = "J000111";
            var status = JournalEntryStatus.Complete;
            var type = JournalEntryType.General;
            DateTime transactionDate = new DateTime(2015, 1, 10);
            DateTime enteredDate = Convert.ToDateTime("04/01/2015");
            string enteredBy = "TGL";
            JournalEntry journalEntry = new JournalEntry(number, transactionDate, status, type, enteredDate, enteredBy);

            Assert.IsTrue(journalEntry.Items.Count() == 0);

            string description = "Adding another item",
            glAccount = "11_00_01_00_33333_54400";
            journalEntry.AddItem(new JournalEntryItem(description, glAccount));
            Assert.IsTrue(journalEntry.Items.Count() == 1);

            // Journal Entries can have duplicate line items
            journalEntry.AddItem(new JournalEntryItem(description, glAccount));
            Assert.IsTrue(journalEntry.Items.Count() == 2);
        }
        #endregion
    }
}
