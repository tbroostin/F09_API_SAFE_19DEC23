// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Utilities
{
    [TestClass]
    public class FilterUtilityTests
    {
        #region Initialize and Cleanup
        CostCenterQueryCriteria criteria = null;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the individual component filter values.
            var componentCriteria = new CostCenterComponentQueryCriteria("OBJECT");
            componentCriteria.IndividualComponentValues = new List<string>() { "51000" };

            // Initialize the range component filter values.
            var rangeCriteria = new CostCenterComponentRangeQueryCriteria("52000", "52999");
            componentCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>() { rangeCriteria };

            // Add the criteria component to the parent object.
            criteria = new CostCenterQueryCriteria(new List<CostCenterComponentQueryCriteria>() { componentCriteria });

            // Exlcude accounts with no activity.
            criteria.IncludeActiveAccountsWithNoActivity = false;
        }

        [TestCleanup]
        public void Cleanup()
        {
            criteria = null;
        }
        #endregion

        #region IsFilterWideOpen
        #region Standard tests
        [TestMethod]
        public void IsFilterWideOpen_IncludeAccountsWithNoActivity()
        {
            criteria.IncludeActiveAccountsWithNoActivity = true;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.IndividualComponentValues = new List<string>();
            }
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
            }
            Assert.IsTrue(FilterUtility.IsFilterWideOpen(criteria));
        }

        [TestMethod]
        public void IsFilterWideOpen_ExcludeAccountsWithNoActivity()
        {
            criteria.IncludeActiveAccountsWithNoActivity = false;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.IndividualComponentValues = new List<string>();
            }
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
            }
            Assert.IsTrue(FilterUtility.IsFilterWideOpen(criteria));
        }

        [TestMethod]
        public void IsFilterWideOpen_LimitWithIndividualComponentValues()
        {
            criteria.IncludeActiveAccountsWithNoActivity = true;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
            }
            Assert.IsFalse(FilterUtility.IsFilterWideOpen(criteria));
        }

        [TestMethod]
        public void IsFilterWideOpen_LimitWithRangeComponentValues()
        {
            criteria.IncludeActiveAccountsWithNoActivity = true;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.IndividualComponentValues = new List<string>();
            }
            Assert.IsFalse(FilterUtility.IsFilterWideOpen(criteria));
        }

        [TestMethod]
        public void IsFilterWideOpen_FullyPopulatedFilter()
        {
            Assert.IsFalse(FilterUtility.IsFilterWideOpen(criteria));
        }

        [TestMethod]
        public void IsFilterWideOpen_EmptyFilter()
        {
            criteria.IncludeActiveAccountsWithNoActivity = true;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.IndividualComponentValues = new List<string>();
            }
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
            }
            Assert.IsTrue(FilterUtility.IsFilterWideOpen(criteria));
        }
        #endregion

        #region Error checking
        [TestMethod]
        public void IsFilterWideOpen_NullValues()
        {
            criteria.IncludeActiveAccountsWithNoActivity = true;
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.IndividualComponentValues = null;
            }
            foreach (var componentCriteria in criteria.ComponentCriteria)
            {
                componentCriteria.RangeComponentValues = null;
            }
            Assert.IsTrue(FilterUtility.IsFilterWideOpen(criteria));
        }
        #endregion
        #endregion
    }
}