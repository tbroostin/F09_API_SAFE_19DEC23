// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GlObjectCodeTests
    {
        #region Initialize and Cleanup
        private GlObjectCodeGlAccount nonPooledAccount1;
        private GlObjectCodeGlAccount nonPooledAccount2;
        private GlObjectCodeGlAccount umbrellaAccount1;
        private GlObjectCodeGlAccount umbrellaAccount2;
        private GlObjectCodeGlAccount pooleeAccount1;
        private GlObjectCodeGlAccount pooleeAccount2;
        private GlObjectCodeGlAccount pooleeAccount3;
        private GlObjectCodeBudgetPool budgetPool1;
        private GlObjectCodeBudgetPool budgetPool2;
        private GlObjectCode glObjectCode;
        private string objectId = "51000";

        [TestInitialize]
        public void Initialize()
        {
            // Initialize new GlAccount objects
            nonPooledAccount1 = new GlObjectCodeGlAccount("1000005308001", GlBudgetPoolType.None);
            nonPooledAccount1.BudgetAmount = 5839m;
            nonPooledAccount1.ActualAmount = 123m;
            nonPooledAccount1.EncumbranceAmount = 50m;

            nonPooledAccount2 = new GlObjectCodeGlAccount("1065305308001", GlBudgetPoolType.None);
            nonPooledAccount2.BudgetAmount = 94815m;
            nonPooledAccount2.ActualAmount = 658m;
            nonPooledAccount2.EncumbranceAmount = 5120m;

            umbrellaAccount1 = new GlObjectCodeGlAccount("1065305308002", GlBudgetPoolType.Umbrella);
            umbrellaAccount1.BudgetAmount = 8524m;
            umbrellaAccount1.ActualAmount = 430m;
            umbrellaAccount1.EncumbranceAmount = 150m;

            umbrellaAccount2 = new GlObjectCodeGlAccount("1065305308003", GlBudgetPoolType.Umbrella);
            umbrellaAccount2.BudgetAmount = 5433m;
            umbrellaAccount2.ActualAmount = 104m;
            umbrellaAccount2.EncumbranceAmount = 451m;

            pooleeAccount1 = new GlObjectCodeGlAccount("1065305308005", GlBudgetPoolType.Poolee);
            pooleeAccount1.BudgetAmount = 1209m;
            pooleeAccount1.ActualAmount = 98m;
            pooleeAccount1.EncumbranceAmount = 188m;

            pooleeAccount2 = new GlObjectCodeGlAccount("1065305308005", GlBudgetPoolType.Poolee);
            pooleeAccount2.BudgetAmount = 1209m;
            pooleeAccount2.ActualAmount = 98m;
            pooleeAccount2.EncumbranceAmount = 188m;

            pooleeAccount3 = new GlObjectCodeGlAccount("1065305308006", GlBudgetPoolType.Poolee);
            pooleeAccount3.BudgetAmount = 1209m;
            pooleeAccount3.ActualAmount = 98m;
            pooleeAccount3.EncumbranceAmount = 188m;

            budgetPool1 = new GlObjectCodeBudgetPool(umbrellaAccount1);
            budgetPool2 = new GlObjectCodeBudgetPool(umbrellaAccount2);
            glObjectCode = new GlObjectCode(objectId, nonPooledAccount1, GlClass.Expense);
        }

        [TestCleanup]
        public void Cleanup()
        {
            nonPooledAccount1 = null;
            nonPooledAccount2 = null;
            pooleeAccount1 = null;
            pooleeAccount2 = null;
            pooleeAccount3 = null;
            budgetPool1 = null;
            budgetPool2 = null;
            glObjectCode = null;
        }
        #endregion

        #region Constructor #1
        [TestMethod]
        public void Constructor_NonPooledAccount()
        {
            var glObjectCode = new GlObjectCode(objectId, nonPooledAccount1, GlClass.Expense);

            Assert.AreEqual(objectId, glObjectCode.Id);
            Assert.AreEqual(GlClass.Expense, glObjectCode.GlClass);
            Assert.AreEqual(1, glObjectCode.GlAccounts.Count);
            Assert.AreEqual(nonPooledAccount1.GlAccountNumber, glObjectCode.GlAccounts.First().GlAccountNumber);
            Assert.AreEqual(0, glObjectCode.Pools.Count);
        }

        [TestMethod]
        public void Constructor_UmbrellaAccount()
        {
            var glObjectCode = new GlObjectCode(objectId, umbrellaAccount1, GlClass.Revenue);

            Assert.AreEqual(objectId, glObjectCode.Id);
            Assert.AreEqual(GlClass.Revenue, glObjectCode.GlClass);
            Assert.AreEqual(0, glObjectCode.GlAccounts.Count);
            Assert.AreEqual(1, glObjectCode.Pools.Count);
            Assert.AreEqual(umbrellaAccount1.GlAccountNumber, glObjectCode.Pools.First().Umbrella.GlAccountNumber);
        }

        [TestMethod]
        public void Constructor_NullId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                new GlObjectCode(null, nonPooledAccount1, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor_EmptyId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                new GlObjectCode("", nonPooledAccount1, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor_NullGlAccount()
        {
            var expectedParamName = "glaccount";
            var actualParamName = "";
            GlObjectCodeGlAccount glAccount = null;
            try
            {
                new GlObjectCode(objectId, glAccount, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor_PooleeAccount()
        {
            var expectedMessage = "Only umbrella accounts can be used to create a Gl Object Code.";
            var actualMessage = "";
            try
            {
                new GlObjectCode("53", pooleeAccount1, GlClass.Expense);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region Constructor #2
        [TestMethod]
        public void Constructor2_Success()
        {
            budgetPool1.AddPoolee(pooleeAccount1);
            budgetPool1.AddPoolee(pooleeAccount2);
            budgetPool1.AddPoolee(pooleeAccount3);
            var glObjectCode = new GlObjectCode(objectId, budgetPool1, GlClass.Expense);

            Assert.AreEqual(objectId, glObjectCode.Id);
            Assert.AreEqual(0, glObjectCode.GlAccounts.Count);
            Assert.AreEqual(1, glObjectCode.Pools.Count);
            Assert.AreEqual(budgetPool1.Umbrella.GlAccountNumber, glObjectCode.Pools.First().Umbrella.GlAccountNumber);
            Assert.AreEqual(budgetPool1.Poolees.Count, glObjectCode.Pools.First().Poolees.Count);
        }

        [TestMethod]
        public void Constructor2_NullId()
        {
            string expectedParamName = "id";
            string actualParamName = "";
            
            try
            {
                new GlObjectCode(null, budgetPool1, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor2_EmptyId()
        {
            string expectedParamName = "id";
            string actualParamName = "";
            try
            {
                new GlObjectCode("", budgetPool1, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor2_NullBudgetPool()
        {
            string expectedParamName = "budgetpool";
            string actualParamName = "";
            GlObjectCodeBudgetPool budgetPool = null;
            try
            {
                new GlObjectCode(objectId, budgetPool, GlClass.Expense);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region AddGlAccount
        [TestMethod]
        public void AddGlAccount_Success()
        {
            glObjectCode.AddGlAccount(nonPooledAccount1);
            Assert.IsTrue(glObjectCode.GlAccounts.Count == 1);

            glObjectCode.AddGlAccount(nonPooledAccount2);
            Assert.IsTrue(glObjectCode.GlAccounts.Count == 2);
        }

        [TestMethod]
        public void AddGlAccount_DuplicateGlAccount()
        {
            glObjectCode.AddGlAccount(nonPooledAccount1);
            Assert.IsTrue(glObjectCode.GlAccounts.Count == 1);

            glObjectCode.AddGlAccount(nonPooledAccount1);
            Assert.IsTrue(glObjectCode.GlAccounts.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddGlAccount_NullGlAccount()
        {
            glObjectCode.AddGlAccount(null);
        }

        [TestMethod]
        public void AddGlAccount_UmbrellaAccount()
        {
            var expectedMessage = "Only non-pooled accounts can be added to the GlAccounts list.";
            var actualMessage = "";

            try
            {
                glObjectCode.AddGlAccount(umbrellaAccount2);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void AddGlAccount_PooleeAccount()
        {
            var expectedMessage = "Only non-pooled accounts can be added to the GlAccounts list.";
            var actualMessage = "";

            try
            {
                glObjectCode.AddGlAccount(pooleeAccount1);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region AddBudgetPool
        [TestMethod]
        public void AddBudgetPool_Success()
        {
            Assert.AreEqual(0, glObjectCode.Pools.Count);
            glObjectCode.AddBudgetPool(budgetPool1);

            Assert.AreEqual(1, glObjectCode.Pools.Count);
        }

        [TestMethod]
        public void AddBudgetPool_NullPool()
        {
            var expectedParamName = "pool";
            var actualParamName = "";
            try
            {
                glObjectCode.AddBudgetPool(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region Totals amounts
        [TestMethod]
        public void Totals_AfterConstructor_NonPooledAccount()
        {
            Assert.AreEqual(nonPooledAccount1.BudgetAmount, glObjectCode.TotalBudget);
            Assert.AreEqual(nonPooledAccount1.ActualAmount, glObjectCode.TotalActuals);
            Assert.AreEqual(nonPooledAccount1.EncumbranceAmount, glObjectCode.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_AfterConstructor_UmbrellaAccount_UmbrellaIsNotVisible()
        {
            glObjectCode = new GlObjectCode(objectId, budgetPool1, GlClass.Expense);
            glObjectCode.Pools.First().IsUmbrellaVisible = false;
            Assert.AreEqual(umbrellaAccount1.BudgetAmount, glObjectCode.TotalBudget);
            Assert.AreEqual(umbrellaAccount1.ActualAmount, glObjectCode.TotalActuals);
            Assert.AreEqual(umbrellaAccount1.EncumbranceAmount, glObjectCode.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreVisible_UmbrellasHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            glObjectCode.AddGlAccount(nonPooledAccount2);

            // Create the pools
            budgetPool1.IsUmbrellaVisible = true;
            budgetPool2.IsUmbrellaVisible = true;

            // Add the umbrella poolees to each pool
            budgetPool1.AddPoolee(pooleeAccount1);
            budgetPool2.AddPoolee(pooleeAccount2);

            // Add the pools to the subtotal
            glObjectCode.AddBudgetPool(budgetPool1);
            glObjectCode.AddBudgetPool(budgetPool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + budgetPool1.Umbrella.BudgetAmount + budgetPool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + budgetPool1.Umbrella.ActualAmount + budgetPool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + budgetPool1.Umbrella.EncumbranceAmount + budgetPool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, glObjectCode.TotalBudget);
            Assert.AreEqual(expectedActuals, glObjectCode.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, glObjectCode.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreVisible_UmbrellasDoNotHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            glObjectCode.AddGlAccount(nonPooledAccount2);

            budgetPool1.IsUmbrellaVisible = true;
            budgetPool2.IsUmbrellaVisible = true;

            // Add the pools to the subtotal
            glObjectCode.AddBudgetPool(budgetPool1);
            glObjectCode.AddBudgetPool(budgetPool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + budgetPool1.Umbrella.BudgetAmount + budgetPool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + budgetPool1.Umbrella.ActualAmount + budgetPool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + budgetPool1.Umbrella.EncumbranceAmount + budgetPool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, glObjectCode.TotalBudget);
            Assert.AreEqual(expectedActuals, glObjectCode.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, glObjectCode.TotalEncumbrances);
        }
        #endregion
    }
}