// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests.Entities
{
    [TestClass]
    public class SortSubtotalComponentQueryCriteriaTests
    {
        #region Initialize and Cleanup

        public SortSubtotalComponentQueryCriteria SortSubtotalComponentQueryCriteriaEntity;

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion

        #region Constructor tests

        [TestMethod]
        public void ComponentQueryCriteriaConstructor_Success()
        {
            var sortSubtotalComponentQueryCriteria = new SortSubtotalComponentQueryCriteria();

            Assert.AreEqual(sortSubtotalComponentQueryCriteria.IsDisplaySubTotal, false);
            Assert.IsNull(sortSubtotalComponentQueryCriteria.SubtotalName);
            Assert.IsNull(sortSubtotalComponentQueryCriteria.SubtotalType);
            Assert.AreEqual(sortSubtotalComponentQueryCriteria.Order, 0);
        }

        #endregion

    }
}