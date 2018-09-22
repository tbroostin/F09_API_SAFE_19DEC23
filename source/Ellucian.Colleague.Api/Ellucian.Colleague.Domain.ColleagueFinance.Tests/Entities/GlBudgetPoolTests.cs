// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GlBudgetPoolTests
    {
        #region Initialize and Cleanup
        private GlBudgetPoolBuilder budgetPoolBuilder;
        private CostCenterGlAccountBuilder glAccountBuilder;

        [TestInitialize]
        public void Initialize()
        {
            budgetPoolBuilder = new GlBudgetPoolBuilder();
            glAccountBuilder = new CostCenterGlAccountBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            budgetPoolBuilder = null;
            glAccountBuilder = null;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Constructor_Success()
        {
            var umbrella = glAccountBuilder.WithPoolType(GlBudgetPoolType.Umbrella)
                .WithBudgetAmount(1000m)
                .WithActualAmount(500m)
                .WithEncumbranceAmount(250m).Build();
            var actualBudgetPool = budgetPoolBuilder
                .WithUmbrella(umbrella).Build();

            // Confirm that the umbrella is set correctly and that the amounts have been saved.
            Assert.AreEqual(budgetPoolBuilder.GlBudgetPoolEntity.Umbrella.GlAccountNumber, actualBudgetPool.Umbrella.GlAccountNumber);
            Assert.AreEqual(GlBudgetPoolType.Umbrella, actualBudgetPool.Umbrella.PoolType);
            Assert.AreEqual(umbrella.BudgetAmount, actualBudgetPool.Umbrella.BudgetAmount);
            Assert.AreEqual(umbrella.ActualAmount, actualBudgetPool.Umbrella.ActualAmount);
            Assert.AreEqual(umbrella.EncumbranceAmount, actualBudgetPool.Umbrella.EncumbranceAmount);

            // Confirm that the rest of the properties are initialized correctly.
            Assert.IsTrue(actualBudgetPool.Poolees is List<CostCenterGlAccount>);
            Assert.AreEqual(0, actualBudgetPool.Poolees.Count);
            Assert.IsFalse(actualBudgetPool.IsUmbrellaVisible);
        }

        [TestMethod]
        public void Constructor_NullUmbrella()
        {
            string expectedParamName = "umbrellaglaccount";
            string actualParamName = "";

            try
            {
                budgetPoolBuilder.WithUmbrella(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region AddPoolee
        [TestMethod]
        public void AddPoolee_FirstPoolee()
        {
            var budgetPool = budgetPoolBuilder.Build();
            var poolee = glAccountBuilder.WithPoolType(GlBudgetPoolType.Poolee).Build();
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(poolee);
            Assert.AreEqual(1, budgetPool.Poolees.Count);
            Assert.AreEqual(poolee.GlAccountNumber, budgetPool.Poolees.First().GlAccountNumber);
        }

        [TestMethod]
        public void AddPoolee_AddTwo()
        {
            var budgetPool = budgetPoolBuilder.Build();
            var poolee1 = glAccountBuilder.WithPoolType(GlBudgetPoolType.Poolee).Build();
            var poolee2 = glAccountBuilder.WithGlAccountNumber("11_13").WithPoolType(GlBudgetPoolType.Poolee).Build();
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(poolee1);
            Assert.AreEqual(1, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(poolee2);
            Assert.AreEqual(2, budgetPool.Poolees.Count);
            
        }

        [TestMethod]
        public void AddPoolee_AddDuplicatePoolee()
        {
            var budgetPool = budgetPoolBuilder.Build();
            var poolee = glAccountBuilder.WithPoolType(GlBudgetPoolType.Poolee).Build();
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(poolee);
            Assert.AreEqual(1, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(poolee);
            Assert.AreEqual(1, budgetPool.Poolees.Count);
        }

        [TestMethod]
        public void AddPoolee_NullPoolee()
        {
            var expectedParamName = "pooleeglaccount";
            var actualParamName = "";

            try
            {
                var budgetPool = budgetPoolBuilder.Build();
                budgetPool.AddPoolee(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void AddPoolee_NonPooledAccount()
        {
            var expectedMessage = "Only poolee accounts can be added to the poolees list.";
            var actualMessage = "";

            try
            {
                var budgetPool = budgetPoolBuilder.Build();
                budgetPool.AddPoolee(glAccountBuilder.WithPoolType(GlBudgetPoolType.None).Build());
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void AddPoolee_UmbrellaAccount()
        {
            var expectedMessage = "Only poolee accounts can be added to the poolees list.";
            var actualMessage = "";

            try
            {
                var budgetPool = budgetPoolBuilder.Build();
                budgetPool.AddPoolee(glAccountBuilder.WithPoolType(GlBudgetPoolType.Umbrella).Build());
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }

            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion
    }
}