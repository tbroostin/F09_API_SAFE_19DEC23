// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// Test the valid and invalid conditions of an budget adjustment summary domain entity
    /// </summary>
    [TestClass]
    public class BudgetAdjustmentSummaryTests
    {
        private string reason;
        private string personId;

        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {
            this.reason = "This is the reason";
            this.personId = "0000001";
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.reason = null;
        }
        #endregion

        [TestMethod]
        public void BudgetAdjustmentSummary_Success()
        {
            var budgetAdjustmentSummary = new BudgetAdjustmentSummary(reason, personId);
            Assert.AreEqual(this.reason, budgetAdjustmentSummary.Reason);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BudgetAdjustmentSummary_NullReason()
        {
            this.reason = null;
            var budgetAdjustmentSummary = new BudgetAdjustmentSummary(reason, personId);
        }
    }
}