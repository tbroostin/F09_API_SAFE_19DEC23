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
    public class GlObjectCodeBudgetPoolTests
    {
        #region Initialize and Cleanup
        private string umbrellaId = "10_00_00_01_20601_51000";
        private string pooleeId1 = "10_00_00_01_20601_51001";
        private string pooleeId2 = "10_00_00_01_20601_51002";
        private GlObjectCodeGlAccount umbrellaGlAccount;
        private GlObjectCodeGlAccount pooleeGlAccount1;
        private GlObjectCodeGlAccount pooleeGlAccount2;
        private GlObjectCodeBudgetPool budgetPool;

        [TestInitialize]
        public void Initialize()
        {
            umbrellaGlAccount = new GlObjectCodeGlAccount(umbrellaId, GlBudgetPoolType.Umbrella);
            umbrellaGlAccount.BudgetAmount = 1000m;
            umbrellaGlAccount.ActualAmount = 250m;
            umbrellaGlAccount.EncumbranceAmount = 100m;
            budgetPool = new GlObjectCodeBudgetPool(umbrellaGlAccount);
            pooleeGlAccount1 = new GlObjectCodeGlAccount(pooleeId1, GlBudgetPoolType.Poolee);
            pooleeGlAccount2 = new GlObjectCodeGlAccount(pooleeId2, GlBudgetPoolType.Poolee);

        }

        [TestCleanup]
        public void Cleanup()
        {
            umbrellaGlAccount = null;
            budgetPool = null;
            pooleeId1 = null;
            pooleeId2 = null;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Constructor_Success()
        {
            // Confirm that the umbrella is set correctly and that the amounts have been saved.
            Assert.AreEqual(umbrellaGlAccount.GlAccountNumber, budgetPool.Umbrella.GlAccountNumber);
            Assert.AreEqual(GlBudgetPoolType.Umbrella, budgetPool.Umbrella.PoolType);
            Assert.AreEqual(umbrellaGlAccount.BudgetAmount, budgetPool.Umbrella.BudgetAmount);
            Assert.AreEqual(umbrellaGlAccount.ActualAmount, budgetPool.Umbrella.ActualAmount);
            Assert.AreEqual(umbrellaGlAccount.EncumbranceAmount, budgetPool.Umbrella.EncumbranceAmount);

            // Confirm that the rest of the properties are initialized correctly.
            Assert.IsTrue(budgetPool.Poolees is List<GlObjectCodeGlAccount>);
            Assert.AreEqual(0, budgetPool.Poolees.Count);
            Assert.IsFalse(budgetPool.IsUmbrellaVisible);
        }

        [TestMethod]
        public void Constructor_NullUmbrella()
        {
            string expectedParamName = "umbrellaglaccount";
            string actualParamName = "";

            try
            {
                new GlObjectCodeBudgetPool(null);
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
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(pooleeGlAccount1);
            Assert.AreEqual(1, budgetPool.Poolees.Count);
            Assert.AreEqual(pooleeGlAccount1.GlAccountNumber, budgetPool.Poolees.First().GlAccountNumber);
        }

        [TestMethod]
        public void AddPoolee_AddTwo()
        {
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(pooleeGlAccount1);
            Assert.AreEqual(1, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(pooleeGlAccount2);
            Assert.AreEqual(2, budgetPool.Poolees.Count);

        }

        [TestMethod]
        public void AddPoolee_AddDuplicatePoolee()
        {
            Assert.AreEqual(0, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(pooleeGlAccount1);
            Assert.AreEqual(1, budgetPool.Poolees.Count);

            budgetPool.AddPoolee(pooleeGlAccount1);
            Assert.AreEqual(1, budgetPool.Poolees.Count);
        }

        [TestMethod]
        public void AddPoolee_NullPoolee()
        {
            var expectedParamName = "pooleeglaccount";
            var actualParamName = "";

            try
            {
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
                budgetPool.AddPoolee(new GlObjectCodeGlAccount("123", GlBudgetPoolType.None));
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
                budgetPool.AddPoolee(new GlObjectCodeGlAccount("123", GlBudgetPoolType.Umbrella));
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