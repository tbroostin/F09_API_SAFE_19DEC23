// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// Test the valid and invalid conditions of a budget adjustment pending approval summary domain entity
    /// </summary>
    [TestClass]
    public class BudgetAdjustmentPendingApprovalSummaryTests
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

        [TestMethod]
        public void BudgetAdjustmentPendingApprovalSummary_Success()
        {
            var budgetAdjustmentSummary = new BudgetAdjustmentPendingApprovalSummary();
            Assert.IsNull(budgetAdjustmentSummary.Reason);
        }
    }
}