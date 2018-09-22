// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class CostCenterSubtotalTests
    {
        #region Initialize and Cleanup
        private CostCenterSubtotalBuilder subtotalBuilder;
        private GlBudgetPoolBuilder budgetPoolBuilder;
        private CostCenterGlAccountBuilder glAccountBuilder;
        private GeneralLedgerComponentDescriptionBuilder glComponentDescriptionBuilder;
        private TestCostCenterRepository testCostCenterRepository;
        private CostCenterGlAccount nonPooledAccount1;
        private CostCenterGlAccount nonPooledAccount2;
        private CostCenterGlAccount umbrellaAccount1;
        private CostCenterGlAccount umbrellaAccount2;
        private CostCenterGlAccount pooleeAccount;

        [TestInitialize]
        public void Initialize()
        {
            subtotalBuilder = new CostCenterSubtotalBuilder();
            budgetPoolBuilder = new GlBudgetPoolBuilder();
            glAccountBuilder = new CostCenterGlAccountBuilder();
            glComponentDescriptionBuilder = new GeneralLedgerComponentDescriptionBuilder();
            testCostCenterRepository = new TestCostCenterRepository();

            // Initialize new GlAccount objects
            nonPooledAccount1 = glAccountBuilder
                .WithGlAccountNumber("1000005308001")
                .WithBudgetAmount(5839m)
                .WithActualAmount(123m)
                .WithEncumbranceAmount(50m).Build();
            nonPooledAccount2 = glAccountBuilder
                .WithGlAccountNumber("1065305308001")
                .WithPoolType(GlBudgetPoolType.None)
                .WithBudgetAmount(94815m)
                .WithActualAmount(658m)
                .WithEncumbranceAmount(5120m).Build();
            umbrellaAccount1 = glAccountBuilder
                .WithGlAccountNumber("1065305308002")
                .WithPoolType(GlBudgetPoolType.Umbrella)
                .WithBudgetAmount(8524m)
                .WithActualAmount(430m)
                .WithEncumbranceAmount(150m).Build();
            umbrellaAccount2 = glAccountBuilder
                .WithGlAccountNumber("1065305308003")
                .WithPoolType(GlBudgetPoolType.Umbrella)
                .WithBudgetAmount(5433m)
                .WithActualAmount(104m)
                .WithEncumbranceAmount(451m).Build();
            pooleeAccount = glAccountBuilder
                .WithGlAccountNumber("1065305308004")
                .WithPoolType(GlBudgetPoolType.Poolee)
                .WithBudgetAmount(1209m)
                .WithActualAmount(98m)
                .WithEncumbranceAmount(188m).Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            subtotalBuilder = null;
            budgetPoolBuilder = null;
            glAccountBuilder = null;
            glComponentDescriptionBuilder = null;
            testCostCenterRepository = null;
            nonPooledAccount1 = null;
            nonPooledAccount2 = null;
            pooleeAccount = null;
        }
        #endregion

        #region Constructor #1
        [TestMethod]
        public void CostCenterSubtotalConstructor_NonPooledAccount()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlAccount(nonPooledAccount1).WithGlClass(GlClass.Expense).BuildWithGlAccount();

            Assert.AreEqual("53", costCenterSubtotal.Id);
            Assert.AreEqual(GlClass.Expense, costCenterSubtotal.GlClass);
            Assert.AreEqual(1, costCenterSubtotal.GlAccounts.Count);
            Assert.AreEqual(nonPooledAccount1.GlAccountNumber, costCenterSubtotal.GlAccounts.First().GlAccountNumber);
            Assert.AreEqual(0, costCenterSubtotal.Pools.Count);
        }

        [TestMethod]
        public void CostCenterSubtotalConstructor_UmbrellaAccount()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlAccount(umbrellaAccount1).WithGlClass(GlClass.Revenue).BuildWithGlAccount();

            Assert.AreEqual("53", costCenterSubtotal.Id);
            Assert.AreEqual(GlClass.Revenue, costCenterSubtotal.GlClass);
            Assert.AreEqual(0, costCenterSubtotal.GlAccounts.Count);
            Assert.AreEqual(1, costCenterSubtotal.Pools.Count);
            Assert.AreEqual(umbrellaAccount1.GlAccountNumber, costCenterSubtotal.Pools.First().Umbrella.GlAccountNumber);
        }

        [TestMethod]
        public void CostCenterSubtotalConstructor_NullId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                subtotalBuilder.WithId(null).BuildWithGlAccount();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void CostCenterSubtotalConstructor_EmptyId()
        {
            var expectedParamName = "id";
            var actualParamName = "";
            try
            {
                subtotalBuilder.WithId("").BuildWithGlAccount();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void CostCenterSubtotalConstructor_NullGlAccount()
        {
            var expectedParamName = "glaccount";
            var actualParamName = "";
            try
            {
                subtotalBuilder.WithId("53").WithGlAccount(null).BuildWithGlAccount();
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
            var umbrellaAccount = glAccountBuilder.WithPoolType(GlBudgetPoolType.Poolee).Build();

            var expectedMessage = "Only umbrella accounts can be used to create a subtotal.";
            var actualMessage = "";
            try
            {
                subtotalBuilder.WithId("53").WithGlAccount(umbrellaAccount).BuildWithGlAccount();
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
            var umbrella = glAccountBuilder.WithGlAccountNumber("10_00").WithPoolType(GlBudgetPoolType.Umbrella).Build();
            var poolee1 = glAccountBuilder.WithGlAccountNumber("10_01").WithPoolType(GlBudgetPoolType.Poolee).Build();
            var poolee2 = glAccountBuilder.WithGlAccountNumber("10_02").WithPoolType(GlBudgetPoolType.Poolee).Build();
            var poolee3 = glAccountBuilder.WithGlAccountNumber("10_03").WithPoolType(GlBudgetPoolType.Poolee).Build();
            var budgetPool = budgetPoolBuilder.WithUmbrella(umbrella).Build();
            budgetPool.AddPoolee(poolee1);
            budgetPool.AddPoolee(poolee2);
            budgetPool.AddPoolee(poolee3);
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithBudgetPool(budgetPool).BuildWithBudgetPool();

            Assert.AreEqual(costCenterSubtotal.Id, "53");
            Assert.AreEqual(0, costCenterSubtotal.GlAccounts.Count);
            Assert.AreEqual(1, costCenterSubtotal.Pools.Count);
            Assert.AreEqual(budgetPool.Umbrella.GlAccountNumber, costCenterSubtotal.Pools.First().Umbrella.GlAccountNumber);
            Assert.AreEqual(budgetPool.Poolees.Count, costCenterSubtotal.Pools.First().Poolees.Count);
        }

        [TestMethod]
        public void Constructor2_NullId()
        {
            string expectedParamName = "id";
            string actualParamName = "";
            try
            {
                subtotalBuilder.WithId(null).BuildWithBudgetPool();
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
                subtotalBuilder.WithId("").BuildWithBudgetPool();
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
            try
            {
                subtotalBuilder.WithId("53").WithBudgetPool(null).BuildWithBudgetPool();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region Id test
        [TestMethod]
        public void Id()
        {
            var subtotalId = "44444";
            var costCenterSubtotalEntity = subtotalBuilder.WithId(subtotalId).WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();

            Assert.AreEqual(subtotalId, costCenterSubtotalEntity.Id);
        }

        #endregion

        #region AddGlAccount
        [TestMethod]
        public void AddGlAccount_Success()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            Assert.IsTrue(costCenterSubtotal.GlAccounts.Count == 1);

            costCenterSubtotal.AddGlAccount(nonPooledAccount2);
            Assert.IsTrue(costCenterSubtotal.GlAccounts.Count == 2);
        }

        [TestMethod]
        public void AddGlAccount_DuplicateGlAccount()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            Assert.IsTrue(costCenterSubtotal.GlAccounts.Count == 1);

            costCenterSubtotal.AddGlAccount(nonPooledAccount1);
            Assert.IsTrue(costCenterSubtotal.GlAccounts.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddGlAccount_NullGlAccount()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).BuildWithGlAccount();
            costCenterSubtotal.AddGlAccount(null);
        }

        [TestMethod]
        public void AddGlAccount_UmbrellaAccount()
        {
            var expectedMessage = "Only non-pooled accounts can be added to the GlAccounts list.";
            var actualMessage = "";

            try
            {
                var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(umbrellaAccount1).BuildWithGlAccount();
                costCenterSubtotal.AddGlAccount(umbrellaAccount2);
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
                var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(umbrellaAccount1).BuildWithGlAccount();
                costCenterSubtotal.AddGlAccount(pooleeAccount);
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
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            Assert.AreEqual(0, costCenterSubtotal.Pools.Count);
            costCenterSubtotal.AddBudgetPool(testCostCenterRepository.CostCenters.SelectMany(x => x.CostCenterSubtotals).SelectMany(x => x.Pools).First());

            Assert.AreEqual(1, costCenterSubtotal.Pools.Count);
        }

        [TestMethod]
        public void AddBudgetPool_NullPool()
        {
            var expectedParamName = "pool";
            var actualParamName = "";
            try
            {
                var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
                costCenterSubtotal.AddBudgetPool(null);
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
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            Assert.AreEqual(nonPooledAccount1.BudgetAmount, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(nonPooledAccount1.ActualAmount, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(nonPooledAccount1.EncumbranceAmount, costCenterSubtotal.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_AfterConstructor_UmbrellaAccount_UmbrellaIsNotVisible()
        {
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(umbrellaAccount1).BuildWithGlAccount();
            Assert.AreEqual(umbrellaAccount1.BudgetAmount, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(umbrellaAccount1.ActualAmount, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(umbrellaAccount1.EncumbranceAmount, costCenterSubtotal.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_AfterConstructor_UmbrellaAccount_UmbrellaIsVisible()
        {

            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(umbrellaAccount1).BuildWithGlAccount();
            costCenterSubtotal.Pools.First().IsUmbrellaVisible = true;
            Assert.AreEqual(umbrellaAccount1.BudgetAmount, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(umbrellaAccount1.ActualAmount, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(umbrellaAccount1.EncumbranceAmount, costCenterSubtotal.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreVisible_UmbrellasHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            costCenterSubtotal.AddGlAccount(nonPooledAccount2);

            // Create the pools
            var pool1 = budgetPoolBuilder.WithUmbrella(umbrellaAccount1).Build();
            var pool2 = budgetPoolBuilder.WithUmbrella(umbrellaAccount2).Build();
            pool1.IsUmbrellaVisible = true;
            pool2.IsUmbrellaVisible = true;

            // Create a poolee for each umbrella to represent direct expenses to the umbrella
            var umbrellaPoolee1 = glAccountBuilder.WithGlAccountNumber(umbrellaAccount1.GlAccountNumber).WithPoolType(GlBudgetPoolType.Poolee).Build();
            var umbrellaPoolee2 = glAccountBuilder.WithGlAccountNumber(umbrellaAccount2.GlAccountNumber).WithPoolType(GlBudgetPoolType.Poolee).Build();

            // Add the umbrella poolees to each pool
            pool1.AddPoolee(umbrellaPoolee1);
            pool2.AddPoolee(umbrellaPoolee2);

            // Add the pools to the subtotal
            costCenterSubtotal.AddBudgetPool(pool1);
            costCenterSubtotal.AddBudgetPool(pool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + pool1.Umbrella.BudgetAmount + pool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + pool1.Umbrella.ActualAmount + pool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + pool1.Umbrella.EncumbranceAmount + pool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(expectedActuals, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, costCenterSubtotal.TotalEncumbrances);
        }


        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreVisible_UmbrellasDoNotHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            costCenterSubtotal.AddGlAccount(nonPooledAccount2);

            // Create the pools
            var pool1 = budgetPoolBuilder.WithUmbrella(umbrellaAccount1).Build();
            var pool2 = budgetPoolBuilder.WithUmbrella(umbrellaAccount2).Build();
            pool1.IsUmbrellaVisible = true;
            pool2.IsUmbrellaVisible = true;

            // Add the pools to the subtotal
            costCenterSubtotal.AddBudgetPool(pool1);
            costCenterSubtotal.AddBudgetPool(pool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + pool1.Umbrella.BudgetAmount + pool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + pool1.Umbrella.ActualAmount + pool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + pool1.Umbrella.EncumbranceAmount + pool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(expectedActuals, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, costCenterSubtotal.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreNotVisible_UmbrellasHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            costCenterSubtotal.AddGlAccount(nonPooledAccount2);

            // Create the pools
            var pool1 = budgetPoolBuilder.WithUmbrella(umbrellaAccount1).Build();
            var pool2 = budgetPoolBuilder.WithUmbrella(umbrellaAccount2).Build();
            pool1.IsUmbrellaVisible = false;
            pool2.IsUmbrellaVisible = false;

            // Create a poolee for each umbrella to represent direct expenses to the umbrella
            var umbrellaPoolee1 = glAccountBuilder.WithGlAccountNumber(umbrellaAccount1.GlAccountNumber).WithPoolType(GlBudgetPoolType.Poolee).Build();
            var umbrellaPoolee2 = glAccountBuilder.WithGlAccountNumber(umbrellaAccount2.GlAccountNumber).WithPoolType(GlBudgetPoolType.Poolee).Build();

            // Add the umbrella poolees to each pool
            pool1.AddPoolee(umbrellaPoolee1);
            pool2.AddPoolee(umbrellaPoolee2);

            // Add the pools to the subtotal
            costCenterSubtotal.AddBudgetPool(pool1);
            costCenterSubtotal.AddBudgetPool(pool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + pool1.Umbrella.BudgetAmount + pool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + pool1.Umbrella.ActualAmount + pool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + pool1.Umbrella.EncumbranceAmount + pool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(expectedActuals, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, costCenterSubtotal.TotalEncumbrances);
        }

        [TestMethod]
        public void Totals_MultipleGlAccounts_MultiplePools_UmbrellasAreNotVisible_UmbrellasDoNotHaveDirectExpenses()
        {
            // Create the cost center subtotal object and add non-pooled accounts.
            var costCenterSubtotal = subtotalBuilder.WithId("53").WithGlClass(GlClass.Expense).WithGlAccount(nonPooledAccount1).BuildWithGlAccount();
            costCenterSubtotal.AddGlAccount(nonPooledAccount2);

            // Create the pools
            var pool1 = budgetPoolBuilder.WithUmbrella(umbrellaAccount1).Build();
            var pool2 = budgetPoolBuilder.WithUmbrella(umbrellaAccount2).Build();
            pool1.IsUmbrellaVisible = false;
            pool2.IsUmbrellaVisible = false;

            // Add the pools to the subtotal
            costCenterSubtotal.AddBudgetPool(pool1);
            costCenterSubtotal.AddBudgetPool(pool2);

            var expectedBudget = nonPooledAccount1.BudgetAmount + nonPooledAccount2.BudgetAmount
                + pool1.Umbrella.BudgetAmount + pool2.Umbrella.BudgetAmount;
            var expectedActuals = nonPooledAccount1.ActualAmount + nonPooledAccount2.ActualAmount
                + pool1.Umbrella.ActualAmount + pool2.Umbrella.ActualAmount;
            var expectedEncumbrances = nonPooledAccount1.EncumbranceAmount + nonPooledAccount2.EncumbranceAmount
                + pool1.Umbrella.EncumbranceAmount + pool2.Umbrella.EncumbranceAmount;
            Assert.AreEqual(expectedBudget, costCenterSubtotal.TotalBudget);
            Assert.AreEqual(expectedActuals, costCenterSubtotal.TotalActuals);
            Assert.AreEqual(expectedEncumbrances, costCenterSubtotal.TotalEncumbrances);
        }
        #endregion
    }
}