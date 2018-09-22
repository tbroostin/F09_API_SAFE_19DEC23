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
    /// This class tests the valid and invalid conditions of the LineItemGlDistribution
    /// domain entity. The LineItemGlDistribution domain entity requires a GL account, 
    /// quantity, and an amount.
    /// </summary>
    [TestClass]
    public class LineItemGlDistributionTests
    {
        #region Initialize and Cleanup
        private string glAccount;
        private decimal quantity;
        private decimal amount;

        [TestInitialize]
        public void Initialize()
        {
            glAccount = "10_00_01_02_20601_53013";
            quantity = 55m;
            amount = 100.56m;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Nothing to do.
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void LineItemGlDistribution_Base()
        {
            var glDistribution = new LineItemGlDistribution(glAccount, quantity, amount);

            Assert.AreEqual(glAccount, glDistribution.GlAccountNumber, "GL number should be initialized.");
            Assert.AreEqual(quantity, glDistribution.Quantity, "Quantity should be initialized.");
            Assert.AreEqual(amount, glDistribution.Amount, "Amount should be initialized.");
            Assert.IsFalse(glDistribution.Masked, "Should default to false.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LineItemGlDistribution_NullGlAccount()
        {
            var glDistribution = new LineItemGlDistribution(null, quantity, amount);
        }
        #endregion

        #region GetFormattedMaskedGlAccount tests
        [TestMethod]
        public void GetFormattedMaskedGlAccount_LongGLNumber()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "01_02_00_01_3333_33333";
            string expectedGlAccountNumber = glAccountNumber.Replace("_", "-");
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(new List<string>()));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_LongGLNumber_Masked()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "01_02_00_01_3333_33333";
            string expectedGlAccountNumber = "##-##-##-##-####-#####";
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            glAccountObject.Masked = true;
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(new List<string>()));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_GlNumberLengthExactly15()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "010203040506077";
            string expectedGlAccountNumber = "01-02-0304050-6077";
            var startPositions = new List<string>() {"1", "3", "5", "12" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_GlNumberLengthExactly15_Masked()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "010203040506077";
            string expectedGlAccountNumber = "##-##-#######-####";
            var startPositions = new List<string>() {"1", "3", "5", "12" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            glAccountObject.Masked = true;
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_ShortGLNumber()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "0102030405";
            string expectedGlAccountNumber = "01-0203-0405";
            var startPositions = new List<string>() {"1", "3", "7" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_ShortGLNumber_Masked()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "0102030405";
            string expectedGlAccountNumber = "##-####-####";
            var startPositions = new List<string>() {"1", "3", "7" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            glAccountObject.Masked = true;
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_TypicalGlNumber()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "1000006503001";
            string expectedGlAccountNumber = "10-0000-65030-01";
            var startPositions = new List<string>() { "1", "3", "7", "12" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedMaskedGlAccount_InvalidStartPosition()
        {
            // Initialize the GL Account object.
            string glAccountNumber = "0102030405";
            string expectedGlAccountNumber = "010203-0405";
            var startPositions = new List<string>() { "0", "7" };
            var glAccountObject = new LineItemGlDistribution(glAccountNumber, quantity, amount);
            Assert.AreEqual(expectedGlAccountNumber, glAccountObject.GetFormattedMaskedGlAccount(startPositions));
        }
        #endregion
    }
}
