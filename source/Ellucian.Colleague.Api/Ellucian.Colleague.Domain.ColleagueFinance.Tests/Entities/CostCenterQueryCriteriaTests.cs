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
    public class CostCenterQueryCriteriaTests
    {
        #region Initialize and Cleanup
        private CostCenterQueryCriteriaBuilder CostCenterQueryCriteriaBuilderObject;
        private CostCenterComponentQueryCriteriaBuilder CostCenterComponentQueryCriteriaBuilderObject;
        private List<CostCenterComponentQueryCriteria> componentCriteria;

        [TestInitialize]
        public void Initialize()
        {
            CostCenterQueryCriteriaBuilderObject = new CostCenterQueryCriteriaBuilder();
            CostCenterComponentQueryCriteriaBuilderObject = new CostCenterComponentQueryCriteriaBuilder();
            componentCriteria = new List<CostCenterComponentQueryCriteria>();
            var criteria1 = CostCenterComponentQueryCriteriaBuilderObject.WithComponentName("FUND").Build();
            componentCriteria.Add(criteria1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            CostCenterQueryCriteriaBuilderObject = null;
            CostCenterComponentQueryCriteriaBuilderObject = null;
            componentCriteria = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void CostCenterQueryCriteriaConstructor_Success()
        {
            var componentCriterias = new List<CostCenterComponentQueryCriteria>()
            {
                new CostCenterComponentQueryCriteria("FUND"),
                new CostCenterComponentQueryCriteria("PROGRAM"),
            };
            var costCenterQueryCriteria = CostCenterQueryCriteriaBuilderObject.WithComponentQueryCriteria(componentCriterias).Build();

            Assert.IsTrue(costCenterQueryCriteria.ComponentCriteria.ToList().Count == 2);
            Assert.IsTrue(costCenterQueryCriteria.ComponentCriteria is IEnumerable<CostCenterComponentQueryCriteria>);

            Assert.AreEqual(componentCriterias.Count, costCenterQueryCriteria.ComponentCriteria.ToList().Count);
            foreach (var criteria in componentCriterias)
            {
                var selectedComponentCriterias = costCenterQueryCriteria.ComponentCriteria.Where(x => x.ComponentName == criteria.ComponentName).ToList();
                Assert.AreEqual(1, selectedComponentCriterias.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterQueryCriteriaConstructor_NullComponentQueryCriteria()
        {
            var costCenterQueryCriteria = CostCenterQueryCriteriaBuilderObject.WithComponentQueryCriteria(null).Build();
        }
        #endregion
 
    }
}