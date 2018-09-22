// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class DraftBudgetAdjustmentTests
    {
        private string reason;
        private List<DraftAdjustmentLine> adjustmentLines;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            this.reason = "lots more money";
            this.adjustmentLines = new List<DraftAdjustmentLine>()
            {
                new DraftAdjustmentLine() { GlNumber= "10_00_01_00_20601_51000",  FromAmount= 100m,  ToAmount= 0m } ,
                new DraftAdjustmentLine() { GlNumber= "10_00_01_00_20601_51004",  FromAmount= 0m,  ToAmount= 100m }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.reason = null;
            this.adjustmentLines = null;
        }
        #endregion

        [TestMethod]
        public void DraftBudgetAdjustment_Success()
        {
            var draftBudgetAdjustment = BuildDraftBudgetAdjustment();
            Assert.AreEqual(this.reason, draftBudgetAdjustment.Reason);
            Assert.IsNotNull(draftBudgetAdjustment.ErrorMessages);
            Assert.AreEqual(0, draftBudgetAdjustment.ErrorMessages.Count);

            Assert.AreEqual(adjustmentLines.Count, draftBudgetAdjustment.AdjustmentLines.Count);
            foreach (var adjustmentLine in this.adjustmentLines)
            {
                var selectedLine = draftBudgetAdjustment.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(selectedLine);
            }

            Assert.AreEqual(0, draftBudgetAdjustment.NextApprovers.Count);
        }

        [TestMethod]
        public void DraftBudgetAdjustmentUpdate_Success()
        {
            var draftBudgetAdjustment = BuildDraftBudgetAdjustment();
            draftBudgetAdjustment.Id = "1";
            draftBudgetAdjustment.Initiator = "Frank N. Stein";
            draftBudgetAdjustment.Comments = "need more budget";
            draftBudgetAdjustment.TransactionDate = new DateTime(2018, 7, 14);
            draftBudgetAdjustment.NextApprovers = new List<NextApprover> { new NextApprover("TGL") };
            Assert.AreEqual(this.reason, draftBudgetAdjustment.Reason);
            Assert.IsNotNull(draftBudgetAdjustment.ErrorMessages);
            Assert.AreEqual(0, draftBudgetAdjustment.ErrorMessages.Count);
            Assert.AreEqual("1", draftBudgetAdjustment.Id);
            Assert.AreEqual("Frank N. Stein", draftBudgetAdjustment.Initiator);
            Assert.AreEqual("need more budget", draftBudgetAdjustment.Comments);
            Assert.AreEqual(new DateTime(2018, 7, 14), draftBudgetAdjustment.TransactionDate);

            Assert.AreEqual(adjustmentLines.Count, draftBudgetAdjustment.AdjustmentLines.Count);
            foreach (var adjustmentLine in this.adjustmentLines)
            {
                var selectedLine = draftBudgetAdjustment.AdjustmentLines.FirstOrDefault(x =>
                    x.GlNumber == adjustmentLine.GlNumber
                    && x.FromAmount == adjustmentLine.FromAmount
                    && x.ToAmount == adjustmentLine.ToAmount);
                Assert.IsNotNull(selectedLine);
            }
        }

        #region Reason errors
        [TestMethod]
        public void DraftBudgetAdjustment_NullReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = null;
                BuildDraftBudgetAdjustment();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void DraftBudgetAdjustment_EmptyReason()
        {
            var expectedParam = "reason";
            var actualParam = "";
            try
            {
                this.reason = "";
                BuildDraftBudgetAdjustment();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Adjustment line tests
        [TestMethod]
        public void BudgetAdjustment_AdjustmentLinesIsNull()
        {
            var expectedParam = "";
            var actualParam = "";
            try
            {
                this.adjustmentLines = null;
                BuildDraftBudgetAdjustment();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void BudgetAdjustment_ZeroAdjustmentLinesAllowed()
        {
            this.adjustmentLines = new List<DraftAdjustmentLine>();
            BuildDraftBudgetAdjustment();
        }

        [TestMethod]
        public void BudgetAdjustment_OneAdjustmentLine()
        {
            this.adjustmentLines.RemoveAt(0);
            BuildDraftBudgetAdjustment();
        }

        [TestMethod]
        public void BudgetAdjustment_NotBalanced()
        {
            this.adjustmentLines.Add(new DraftAdjustmentLine() { GlNumber = "10_00_01_00_20601_51000", FromAmount = 100m, ToAmount = 0m });
            BuildDraftBudgetAdjustment();
        }

        [TestMethod]
        public void BudgetAdjustment_DuplicateGlNumbersAllowed()
        {
            this.adjustmentLines.Add(new DraftAdjustmentLine() { GlNumber = "10_00_01_00_20601_51000", FromAmount = 100m, ToAmount = 0m });
            this.adjustmentLines.Add(new DraftAdjustmentLine() { GlNumber = "10_00_01_00_20601_51000", FromAmount = 0m, ToAmount = 100m });
            BuildDraftBudgetAdjustment();
        }
        #endregion


        private DraftBudgetAdjustment BuildDraftBudgetAdjustment()
        {
            var adjustment = new DraftBudgetAdjustment(reason);
            if (adjustmentLines != null)
            {
                foreach (var line in adjustmentLines)
                {
                    if (line != null)
                    {
                        adjustment.AdjustmentLines.Add(line);
                    }
                }
            }
            return adjustment;
        }
    }
}