// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class BudgetAdjustmentsEnabledTests
    {
        private bool enabled = true;

        [TestMethod]
        public void BudgetAdjustmentsEnabled()
        {
            var entity = BuildBudgetAdjustmentEnabled();
            Assert.AreEqual(this.enabled, entity.Enabled);
        }

        private BudgetAdjustmentsEnabled BuildBudgetAdjustmentEnabled()
        {
            return new BudgetAdjustmentsEnabled(this.enabled);
        }
    }
}