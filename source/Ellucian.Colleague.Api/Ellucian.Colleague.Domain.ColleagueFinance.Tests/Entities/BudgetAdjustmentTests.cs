// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class BudgetAdjustmentTests
    {
        private string id;
        private DateTime transactionDate;
        private string reason;
        private string personId;
        private List<AdjustmentLine> adjustmentLines;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            this.id = "B000001";
            this.transactionDate = DateTime.Now;
            this.reason = "more money";
            this.personId = "0000001";
            this.adjustmentLines = new List<AdjustmentLine>()
            {
                new AdjustmentLine("10_00_01_00_20601_51000", 100m, 0m),
                new AdjustmentLine("10_00_01_00_20601_51001", 0m, 100m)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.id = null;
            this.transactionDate = DateTime.Now;
            this.reason = null;
            this.personId = null;
            this.adjustmentLines = null;
        }
        #endregion

        #region Constructor without ID
        [TestMethod]
        public void BudgetAdjustmentWithoutId_Success()
        {
            var budgetAdjustment = new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
            Assert.AreEqual(this.transactionDate, budgetAdjustment.TransactionDate);
            Assert.AreEqual(this.reason, budgetAdjustment.Reason);
            Assert.IsNotNull(budgetAdjustment.ErrorMessages);
            Assert.AreEqual(0, budgetAdjustment.ErrorMessages.Count);

            Assert.AreEqual(adjustmentLines.Count, budgetAdjustment.AdjustmentLines.Count);
            foreach (var adjustmentLine in this.adjustmentLines)
            {
                var selectedLine = budgetAdjustment.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(selectedLine);
            }
        }

        #region Reason errors
        [TestMethod]
        public void BudgetAdjustment_NullReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = null;
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustment_EmptyReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = "";
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Person ID errors
        [TestMethod]
        public void BudgetAdjustment_NullPersonId()
        {
            var expectedParam = "personId";
            var actualParam = "";
            try
            {
                this.personId = null;
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustment_EmptyPersonId()
        {
            var expectedParam = "personId";
            var actualParam = "";
            try
            {
                this.personId = string.Empty;
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Adjustment line errors

        [TestMethod]
        public void BudgetAdjustment_AdjustmentLinesIsNull()
        {
            var expectedParam = "adjustmentLines";
            var actualParam = "";
            try
            {
                this.adjustmentLines = null;
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustment_ZeroAdjustmentLines()
        {
            var expectedParam = "adjustmentLines";
            var actualParam = "";
            try
            {
                this.adjustmentLines = new List<AdjustmentLine>();
                BuildBudgetAdjustmentWithoutId();
            }
            catch (ArgumentException aex)
            {
                actualParam = aex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustment_OneAdjustmentLine()
        {
            this.adjustmentLines.RemoveAt(0);
            BuildBudgetAdjustmentWithoutId();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustment_NotBalanced()
        {
            this.adjustmentLines.Add(new AdjustmentLine("1", 100m, 0m));
            BuildBudgetAdjustmentWithoutId();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BudgetAdjustment_DuplicateGlNumbers()
        {
            this.adjustmentLines.Add(new AdjustmentLine("1", 100m, 0m));
            this.adjustmentLines.Add(new AdjustmentLine("1", 0m, 100m));
            BuildBudgetAdjustmentWithoutId();
        }

        #endregion
        #endregion

        #region Constructor with ID
        [TestMethod]
        public void BudgetAdjustmentWithId_Success()
        {
            var budgetAdjustment = BuildBudgetAdjustmentWithId();
            Assert.AreEqual(this.id, budgetAdjustment.Id);
            Assert.AreEqual(this.transactionDate, budgetAdjustment.TransactionDate);
            Assert.AreEqual(this.reason, budgetAdjustment.Reason);
            Assert.AreEqual(this.personId, budgetAdjustment.PersonId);

            Assert.AreEqual(adjustmentLines.Count, budgetAdjustment.AdjustmentLines.Count);
            foreach (var adjustmentLine in this.adjustmentLines)
            {
                var selectedLine = budgetAdjustment.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(selectedLine);
            }
        }

        [TestMethod]
        public void BudgetAdjustment_NullId()
        {
            var expectedParam = "id";
            var actualParam = "";
            try
            {
                this.id = null;
                BuildBudgetAdjustmentWithId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustment_EmptyId()
        {
            var expectedParam = "id";
            var actualParam = "";
            try
            {
                this.id = "";
                BuildBudgetAdjustmentWithId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Constructor with no adjustment lines as parameter

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_Success()
        {
            var budgetAdjustment = new BudgetAdjustment(this.id, this.reason, this.transactionDate, this.personId);
            Assert.AreEqual(this.id, budgetAdjustment.Id);
            Assert.AreEqual(this.transactionDate, budgetAdjustment.TransactionDate);
            Assert.AreEqual(this.reason, budgetAdjustment.Reason);
            Assert.AreEqual(this.personId, budgetAdjustment.PersonId);

            foreach (var adjustmentLine in this.adjustmentLines)
            {
                budgetAdjustment.AddAdjustmentLine(adjustmentLine);
            }

            Assert.AreEqual(adjustmentLines.Count, budgetAdjustment.AdjustmentLines.Count);

            foreach (var adjustmentLine in this.adjustmentLines)
            {
                var selectedLine = budgetAdjustment.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(selectedLine);
            }
        }

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_NullId()
        {
            var expectedParam = "id";
            var actualParam = "";
            try
            {
                this.id = null;
                BuildBudgetAdjustmentWithId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_EmptyId()
        {
            var expectedParam = "id";
            var actualParam = "";
            try
            {
                this.id = "";
                var budgetAdjustment = new BudgetAdjustment(this.id, this.reason, this.transactionDate, this.personId);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_NullReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = null;
                BuildBudgetAdjustmentWithId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_EmptyReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = "";
                var budgetAdjustment = new BudgetAdjustment(this.id, this.reason, this.transactionDate, this.personId);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_NullPersonId()
        {
            var expectedParam = "personId";
            var actualParam = "";
            try
            {
                this.personId = null;
                BuildBudgetAdjustmentWithId();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustmentNoAdjustmentLines_EmptyPersonId()
        {
            var expectedParam = "personId";
            var actualParam = "";
            try
            {
                this.personId = "";
                var budgetAdjustment = new BudgetAdjustment(this.id, this.reason, this.transactionDate, this.personId);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        #endregion

        private BudgetAdjustment BuildBudgetAdjustmentWithoutId()
        {
            return new BudgetAdjustment(transactionDate, reason, personId, adjustmentLines);
        }

        private BudgetAdjustment BuildBudgetAdjustmentWithId()
        {
            return new BudgetAdjustment(this.id, this.transactionDate, this.reason, this.personId, this.adjustmentLines);
        }
    }
}