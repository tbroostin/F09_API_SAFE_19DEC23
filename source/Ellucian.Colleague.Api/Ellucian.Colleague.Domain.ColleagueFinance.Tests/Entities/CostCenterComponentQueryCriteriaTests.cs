// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class CostCenterComponentQueryCriteriaTests
    {
        #region Initialize and Cleanup
        private CostCenterComponentQueryCriteriaBuilder CostCenterComponentQueryCriteriaBuilderObject;

        [TestInitialize]
        public void Initialize()
        {
            CostCenterComponentQueryCriteriaBuilderObject = new CostCenterComponentQueryCriteriaBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CostCenterComponentQueryCriteriaBuilderObject = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void CostCenterComponentQueryCriteriaConstructor_Success()
        {
            var costCenterComponentQueryCriteria = CostCenterComponentQueryCriteriaBuilderObject.WithComponentName("Fund").Build();

            Assert.AreEqual(CostCenterComponentQueryCriteriaBuilderObject.ComponentName, costCenterComponentQueryCriteria.ComponentName);
            Assert.IsTrue(costCenterComponentQueryCriteria.IndividualComponentValues.Count == 0);
            Assert.IsTrue(costCenterComponentQueryCriteria.RangeComponentValues.Count == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentQueryCriteriaConstructor_NullComponentName()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentQueryCriteriaBuilderObject.WithComponentName(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentQueryCriteriaConstructor_EmptyComponentName()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentQueryCriteriaBuilderObject.WithComponentName("").Build();
        }
        #endregion

    }
}