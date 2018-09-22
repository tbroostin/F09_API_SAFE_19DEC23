// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class BudgetAdjustmentExcludedElementTests
    {
        [TestMethod]
        public void BudgetAdjustmentExcludedElement()
        {
            var excludedElement = BuildExcludedElement();

            Assert.IsNull(excludedElement.ExclusionComponent);
            Assert.IsNull(excludedElement.ExclusionRange);
        }

        public BudgetAdjustmentExcludedElement BuildExcludedElement()
        {
            return new BudgetAdjustmentExcludedElement();
        }
    }
}