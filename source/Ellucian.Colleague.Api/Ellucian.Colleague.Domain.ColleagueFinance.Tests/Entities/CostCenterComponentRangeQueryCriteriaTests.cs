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
    public class CostCenterComponentRangeQueryCriteriaTests
    {
        #region Initialize and Cleanup
        private CostCenterComponentRangeQueryCriteriaBuilder CostCenterComponentRangeQueryCriteriaBuilderObject;

        [TestInitialize]
        public void Initialize()
        {
            CostCenterComponentRangeQueryCriteriaBuilderObject = new CostCenterComponentRangeQueryCriteriaBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CostCenterComponentRangeQueryCriteriaBuilderObject = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void CostCenterComponentRangeQueryCriteriaConstructor_Success()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentRangeQueryCriteriaBuilderObject.WithStartAndEndValue("10", "25").Build();

            Assert.AreEqual(CostCenterComponentRangeQueryCriteriaBuilderObject.StartValue, costCenterComponentRangeQueryCriteria.StartValue);
            Assert.AreEqual(CostCenterComponentRangeQueryCriteriaBuilderObject.EndValue, costCenterComponentRangeQueryCriteria.EndValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentRangeQueryCriteriaConstructor_NullStartValue()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentRangeQueryCriteriaBuilderObject.WithStartAndEndValue(null, "25").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentRangeQueryCriteriaConstructor_EmptyStartValue()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentRangeQueryCriteriaBuilderObject.WithStartAndEndValue("", "25").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentRangeQueryCriteriaConstructor_NullEndValue()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentRangeQueryCriteriaBuilderObject.WithStartAndEndValue("10", null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterComponentRangeQueryCriteriaConstructor_EmptyEndValue()
        {
            var costCenterComponentRangeQueryCriteria = CostCenterComponentRangeQueryCriteriaBuilderObject.WithStartAndEndValue("10", "").Build();
        }
        #endregion

    }
}