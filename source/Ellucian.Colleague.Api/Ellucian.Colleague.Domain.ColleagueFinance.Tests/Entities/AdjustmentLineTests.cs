// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class AdjustmentLineTests
    {
        private string glNumber;
        private decimal from;
        private decimal to;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            this.glNumber = "10_00_00_01_20601_51000";
            this.from = 100m;
            this.to = 0m;
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.glNumber = "10_00_00_01_20601_51000";
            this.from = 100m;
            this.to = 0m;
        }
        #endregion

        [TestMethod]
        public void BudgetAdjustmentLine_Success()
        {
            var adjustmentLine = BuildBudgetAdjustmentLine();
            Assert.AreEqual(this.glNumber, adjustmentLine.GlNumber);
            Assert.AreEqual(this.from, adjustmentLine.FromAmount);
            Assert.AreEqual(this.to, adjustmentLine.ToAmount);
        }

        #region GL number errors
        [TestMethod]
        public void BudgetAdjustmentLine_NullGLNumber()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                this.glNumber = null;
                BuildBudgetAdjustmentLine();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustmentLine_EmptyGLNumber()
        {
            var expectedParam = "glAccount";
            var actualParam = "";
            try
            {
                this.glNumber = null;
                BuildBudgetAdjustmentLine();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Amount errors
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustmentLine_HasNeitherAmount()
        {
            this.from = 0m;
            this.to = 0m;
            BuildBudgetAdjustmentLine();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustmentLine_HasBothAmounts()
        {
            this.from = 1m;
            this.to = 1m;
            BuildBudgetAdjustmentLine();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustmentLine_NegativeFrom()
        {
            this.from = -100m;
            this.to = 0m;
            BuildBudgetAdjustmentLine();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustmentLine_NegativeTo()
        {
            this.from = 0m;
            this.to = -50m;
            BuildBudgetAdjustmentLine();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustmentLine_BothNumbersNegative()
        {
            this.from = -100m;
            this.to = -50m;
            var adjustmentLine = BuildBudgetAdjustmentLine();
        }
        #endregion

        private AdjustmentLine BuildBudgetAdjustmentLine()
        {
            return new AdjustmentLine(this.glNumber, this.from, this.to);
        }
    }
}