// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of a 
    /// Budget Development configuration domain entity. 
    /// </summary>
    [TestClass]
    public class BudgetDevelopmentConfigurationTests
    {
        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion

        [TestMethod]
        public void BudgetDevelopmentConfiguration_Success()
        {
            string budgetId = "FY2021";
            var buDevConfig = new BudgetConfiguration(budgetId);

            Assert.AreEqual(budgetId, buDevConfig.BudgetId);
            Assert.IsNull(buDevConfig.BudgetTitle);
            Assert.IsNull(buDevConfig.BudgetYear);
            // The status will contain the first value in the enum as a default.
            Assert.AreEqual(buDevConfig.BudgetStatus, BudgetStatus.New);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BudgetDevelopmentConfiguration_NullBudgetId()
        {
            var buDevConfig = new BudgetConfiguration(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BudgetDevelopmentConfiguration_EmptyBudgetId()
        {
            var buDevConfig = new BudgetConfiguration("");
        }
    }
}
