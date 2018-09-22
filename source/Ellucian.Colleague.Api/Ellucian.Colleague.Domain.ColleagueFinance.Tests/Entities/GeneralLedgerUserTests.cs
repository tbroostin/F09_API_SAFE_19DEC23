// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests that GeneralLedgerUser object is initialized with an empty list
    /// of general ledger expense account ID.
    /// </summary>
    [TestClass]
    public class GeneralLedgerUserTests
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
        public void GeneralLedgerUserConstructor_Success()
        {
            string id = "0000001";
            string lastName = "Thorne";
            GeneralLedgerUser generalLedgerUser = new GeneralLedgerUser(id, lastName);
            Assert.IsTrue(!generalLedgerUser.AllAccounts.Any());
            Assert.IsTrue(!generalLedgerUser.ExpenseAccounts.Any());
            Assert.IsTrue(!generalLedgerUser.RevenueAccounts.Any());
            Assert.AreEqual(generalLedgerUser.GlAccessLevel, GlAccessLevel.No_Access);
        }
        #endregion

        #region AddAllAccounts
        [TestMethod]
        public void AddAllAccounts_Success()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddAllAccounts_InputIsNull()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            List<string> glAccountIds = null;
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list is still empty.
            Assert.IsFalse(glUser.AllAccounts.Any());
        }

        [TestMethod]
        public void AddAllAccounts_ListAlreadyPopulated()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }

            // Add a few more IDs to the list
            glAccountIds.Add("4");
            glAccountIds.Add("5");

            // Add the account IDs again...
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddAllAccounts_AddSameListTwice()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }

            // Add the account IDs again...
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddAllAccounts_InputListContainsNullAndEmptyValues()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", null, "3", "", "4" };
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the GL accounts list only contains the non-null/empty values.
            var expectedGlAccounts = glAccountIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
            Assert.AreEqual(expectedGlAccounts.Count, glUser.AllAccounts.Count);
            foreach (var expectedGlAccount in expectedGlAccounts)
            {
                var actualGlAccount = glUser.AllAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(actualGlAccount);
            }
        }
        #endregion

        #region AddExpenseAccounts
        [TestMethod]
        public void AddExpenseAccounts_Success()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.ExpenseAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.ExpenseAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddExpenseAccounts_InputIsNull()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.ExpenseAccounts.Any());

            // Add the account IDs
            List<string> glAccountIds = null;
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list is still empty.
            Assert.IsFalse(glUser.ExpenseAccounts.Any());
        }

        [TestMethod]
        public void AddExpenseAccounts_ListAlreadyPopulated()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.ExpenseAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.ExpenseAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(glAccountId));
            }

            // Add a few more IDs to the list
            glAccountIds.Add("4");
            glAccountIds.Add("5");

            // Add the account IDs again...
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.ExpenseAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddExpenseAccounts_AddSameListTwice()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.ExpenseAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.ExpenseAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(glAccountId));
            }

            // Add the account IDs again...
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the ExpenseAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.ExpenseAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.ExpenseAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddExpenseAccounts_InputListContainsNullAndEmptyValues()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.ExpenseAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", null, "3", "", "4" };
            glUser.AddExpenseAccounts(glAccountIds);

            // Make sure the GL accounts list only contains the non-null/empty values.
            var expectedGlAccounts = glAccountIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
            Assert.AreEqual(expectedGlAccounts.Count, glUser.ExpenseAccounts.Count);
            foreach (var expectedGlAccount in expectedGlAccounts)
            {
                var actualGlAccount = glUser.ExpenseAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(actualGlAccount);
            }
        }
        #endregion

        #region AddRevenueAccounts
        [TestMethod]
        public void AddRevenueAccounts_Success()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.RevenueAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.RevenueAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddRevenueAccounts_InputIsNull()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.RevenueAccounts.Any());

            // Add the account IDs
            List<string> glAccountIds = null;
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list is still empty.
            Assert.IsFalse(glUser.RevenueAccounts.Any());
        }

        [TestMethod]
        public void AddRevenueAccounts_ListAlreadyPopulated()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.RevenueAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.RevenueAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(glAccountId));
            }

            // Add a few more IDs to the list
            glAccountIds.Add("4");
            glAccountIds.Add("5");

            // Add the account IDs again...
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.RevenueAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddRevenueAccounts_AddSameListTwice()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.RevenueAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.RevenueAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(glAccountId));
            }

            // Add the account IDs again...
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the RevenueAccounts list matches the input list and does not contain duplicate values
            Assert.AreEqual(glAccountIds.Count, glUser.RevenueAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.RevenueAccounts.Contains(glAccountId));
            }
        }

        [TestMethod]
        public void AddRevenueAccounts_InputListContainsNullAndEmptyValues()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.RevenueAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", null, "3", "", "4" };
            glUser.AddRevenueAccounts(glAccountIds);

            // Make sure the GL accounts list only contains the non-null/empty values.
            var expectedGlAccounts = glAccountIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
            Assert.AreEqual(expectedGlAccounts.Count, glUser.RevenueAccounts.Count);
            foreach (var expectedGlAccount in expectedGlAccounts)
            {
                var actualGlAccount = glUser.RevenueAccounts.FirstOrDefault(x => x == expectedGlAccount);
                Assert.IsNotNull(actualGlAccount);
            }
        }
        #endregion

        #region Remove methods
        [TestMethod]
        public void RemoveAllAccounts()
        {
            // Set up the data
            var glUser = new GeneralLedgerUser("1", "Kleehammer");

            // Make sure the accounts list is blank
            Assert.IsFalse(glUser.AllAccounts.Any());

            // Add the account IDs
            var glAccountIds = new List<string>() { "1", "2", "3" };
            glUser.AddAllAccounts(glAccountIds);

            // Make sure the AllAccounts list matches the input list.
            Assert.AreEqual(glAccountIds.Count, glUser.AllAccounts.Count);
            foreach (var glAccountId in glAccountIds)
            {
                Assert.IsTrue(glUser.AllAccounts.Contains(glAccountId));
            }

            // Now remove all accounts and make sure the AllAccounts list is empty.
            glUser.RemoveAllAccounts();
            Assert.IsTrue(!glUser.AllAccounts.Any());
        }

        [TestMethod]
        public void RemoveAllExpenseAccounts()
        {
            string id = "0000001";
            string lastName = "Thorne";
            GeneralLedgerUser generalLedgerUser = new GeneralLedgerUser(id, lastName);
            var expenseIds = new List<string>();
            expenseIds.Add("1");
            expenseIds.Add("2");
            generalLedgerUser.AddExpenseAccounts(expenseIds);

            // Confirm that the number of accounts added is the same as the number in the local list
            Assert.AreEqual(expenseIds.Count(), generalLedgerUser.ExpenseAccounts.Count());

            // Confirm that the IDs of the local list match the IDs of the accounts in the GeneralLedgerUser object
            for (var i = 0; i < expenseIds.Count(); i++)
            {
                Assert.AreEqual(expenseIds[i], generalLedgerUser.ExpenseAccounts[i]);
            }

            // Now call the RemoveAll method and confirm that the list has been cleared.
            generalLedgerUser.RemoveAllExpenseAccounts();
            Assert.IsTrue(!generalLedgerUser.ExpenseAccounts.Any());
        }

        [TestMethod]
        public void RemoveAllRevenueAccounts()
        {
            string id = "0000001";
            string lastName = "Thorne";
            GeneralLedgerUser generalLedgerUser = new GeneralLedgerUser(id, lastName);
            var revenueIds = new List<string>();
            revenueIds.Add("1");
            revenueIds.Add("2");
            generalLedgerUser.AddRevenueAccounts(revenueIds);

            // Confirm that the number of accounts added is the same as the number in the local list
            Assert.AreEqual(revenueIds.Count(), generalLedgerUser.RevenueAccounts.Count());

            // Confirm that the IDs of the local list match the IDs of the accounts in the GeneralLedgerUser object
            for (var i = 0; i < revenueIds.Count(); i++)
            {
                Assert.AreEqual(revenueIds[i], generalLedgerUser.RevenueAccounts[i]);
            }

            // Now call the RemoveAll method and confirm that the list has been cleared.
            generalLedgerUser.RemoveAllRevenueAccounts();
            Assert.IsTrue(!generalLedgerUser.RevenueAccounts.Any());
        }
        #endregion

        #region tests for SetGlAccessLevel
        [TestMethod]
        public void SetGlAccessLevel_Full_Access()
        {
            string id = "0000001";
            string lastName = "Thorne";
            GeneralLedgerUser generalLedgerUser = new GeneralLedgerUser(id, lastName);
            generalLedgerUser.SetGlAccessLevel(GlAccessLevel.Full_Access);
            Assert.AreEqual(generalLedgerUser.GlAccessLevel, GlAccessLevel.Full_Access);
        }
        [TestMethod]
        public void SetGlAccessLevel_Possible_Access()
        {
            string id = "0000001";
            string lastName = "Thorne";
            GeneralLedgerUser generalLedgerUser = new GeneralLedgerUser(id, lastName);
            generalLedgerUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            Assert.AreEqual(generalLedgerUser.GlAccessLevel, GlAccessLevel.Possible_Access);
        }
        #endregion
    }
}