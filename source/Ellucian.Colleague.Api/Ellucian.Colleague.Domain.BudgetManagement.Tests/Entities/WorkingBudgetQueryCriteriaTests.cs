// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests.Entities
{
    [TestClass]
    public class WorkingBudgetQueryCriteriaTests
    {
        #region Initialize and Cleanup
        public List<ComponentQueryCriteria> componentCriteria;

        [TestInitialize]
        public void Initialize()
        {
            componentCriteria = new List<ComponentQueryCriteria>()
            {
                new ComponentQueryCriteria("FUND"),
                new ComponentQueryCriteria("PROGRAM"),
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            componentCriteria = null;
        }
        #endregion

        [TestMethod]
        public void WorkingBudgetQueryCriteriaConstructor_Success()
        {
            var WorkingBudgetQueryCriteria = new WorkingBudgetQueryCriteria(componentCriteria);

            Assert.IsTrue(WorkingBudgetQueryCriteria.ComponentCriteria.ToList().Count == 2);
            Assert.IsTrue(WorkingBudgetQueryCriteria.ComponentCriteria is IEnumerable<ComponentQueryCriteria>);

            foreach (var criteria in componentCriteria)
            {
                var selectedComponentCriterias = WorkingBudgetQueryCriteria.ComponentCriteria.Where(x => x.ComponentName == criteria.ComponentName).ToList();
                Assert.AreEqual(1, selectedComponentCriterias.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkingBudgetQueryCriteriaConstructor_NullComponentQueryCriteria()
        {
            var WorkingBudgetQueryCriteria = new WorkingBudgetQueryCriteria(null);
        }
    }
}