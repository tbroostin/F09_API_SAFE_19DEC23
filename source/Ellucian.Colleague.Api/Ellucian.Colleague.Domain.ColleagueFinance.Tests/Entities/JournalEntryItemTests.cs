// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// Test the valid and invalid conditions of an Journal Entry Item domain entity
    /// </summary>
    [TestClass]
    public class JournalEntryItemTests
    {
        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion

        #region Constructor tests

        [TestMethod]
        public void JournalEntryItem_Success()
        {
            string description = "First Item description";
            string glAccount = "11_00_01_00_33333_54005";
            
            var journalEntryItem = new JournalEntryItem(description, glAccount);

            Assert.AreEqual(description, journalEntryItem.Description);
            Assert.AreEqual(glAccount, journalEntryItem.GlAccountNumber);            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JournalEntryItem_NullDescription()
        {
            string description = null;
            string glAccount = "11_00_01_00_33333_54005";
            var journalEntryItem = new JournalEntryItem(description, glAccount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JournalEntryItem_NullGlAccount()
        {
            string description = "First Item description";
            string glAccount = null;
            var journalEntryItem = new JournalEntryItem(description, glAccount);
        }

        #endregion
    }
}
