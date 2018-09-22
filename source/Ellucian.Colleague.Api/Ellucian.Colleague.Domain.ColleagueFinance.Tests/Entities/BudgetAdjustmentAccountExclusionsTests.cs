// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class BudgetAdjustmentAccountExclusionsTests
    {
        [TestMethod]
        public void BudgetAdjustmentAccountExclusions()
        {
            var exclusions = BuildExclusions();

            Assert.IsTrue(exclusions.ExcludedElements is List<BudgetAdjustmentExcludedElement>);
            Assert.AreEqual(0, exclusions.ExcludedElements.Count);
        }

        public BudgetAdjustmentAccountExclusions BuildExclusions()
        {
            return new BudgetAdjustmentAccountExclusions();
        }
    }
}