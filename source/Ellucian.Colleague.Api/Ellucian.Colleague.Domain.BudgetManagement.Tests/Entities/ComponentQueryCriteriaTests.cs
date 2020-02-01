// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests.Entities
{
    [TestClass]
    public class ComponentQueryCriteriaTests
    {
        #region Initialize and Cleanup

        public ComponentQueryCriteria ComponentQueryCriteriaEntity;
        public List<string> IndividualComponentValues;
        public List<ComponentRangeQueryCriteria> RangeComponentValues;

        [TestInitialize]
        public void Initialize()
        {
            ComponentQueryCriteriaEntity = new ComponentQueryCriteria("FUND")
            {
                IndividualComponentValues = new List<string>(),
                RangeComponentValues = new List<ComponentRangeQueryCriteria>()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            ComponentQueryCriteriaEntity = null;
        }
        #endregion

        #region Constructor tests

        [TestMethod]
        public void ComponentQueryCriteriaConstructor_Success()
        {
            var componentQueryCriteria = new ComponentQueryCriteria("FUND");

            Assert.AreEqual(componentQueryCriteria.ComponentName, ComponentQueryCriteriaEntity.ComponentName);
            Assert.IsTrue(componentQueryCriteria.IndividualComponentValues.Count == 0);
            Assert.IsTrue(componentQueryCriteria.RangeComponentValues.Count == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComponentQueryCriteriaConstructor_NullComponentName()
        {
            var componentRangeQueryCriteria = new ComponentQueryCriteria(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComponentQueryCriteriaConstructor_EmptyComponentName()
        {
            var componentRangeQueryCriteria = new ComponentQueryCriteria("");
        }
        #endregion

    }
}