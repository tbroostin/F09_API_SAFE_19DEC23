// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of the BlanketPurchaseOrderGlDistribution domain entity
    /// </summary>
    [TestClass]
    public class BlanketPurchaseOrderGlDistributionTests
    {
        #region Initialize and Cleanup

        private string glAccount;
        private decimal encumberedAmount;

        [TestInitialize]
        public void Initialize()
        {
            glAccount = "10_00_01_02_20601_53013";
            encumberedAmount = 100.56m;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Nothing to do
        }
        #endregion

        #region Constructor tests

        [TestMethod]
        public void BlanketPurchaseOrderGlDistribution_Base()
        {
            var glDistribution = new BlanketPurchaseOrderGlDistribution(glAccount, encumberedAmount);

            Assert.AreEqual(glAccount, glDistribution.GlAccountNumber, "GL account should be initialized.");
            Assert.AreEqual(encumberedAmount, glDistribution.EncumberedAmount, "Encumbered amount should be initialized.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlanketPurchaseOrderGlDistribution_NullGlAccount()
        {
            var glDistribution = new BlanketPurchaseOrderGlDistribution(null, encumberedAmount);
        }
        #endregion
    }
}
