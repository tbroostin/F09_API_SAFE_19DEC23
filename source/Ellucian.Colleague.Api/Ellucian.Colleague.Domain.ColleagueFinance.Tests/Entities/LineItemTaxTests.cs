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
    /// This class tests the valid and invalid conditions of the LineItemTax domain
    /// entity. The LineItemTax domain entity requires a tax code.
    /// </summary>
    [TestClass]
    public class LineItemTaxTests
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

        #region Constructor tests
        [TestMethod]
        public void LineItemTax_Base()
        {
            string taxCode = "VA";
            decimal taxAmount = 105.99m;
            var tax = new LineItemTax(taxCode, taxAmount);

            Assert.AreEqual(taxCode, tax.TaxCode);
            Assert.AreEqual(taxAmount, tax.TaxAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LineItemTax_NullTaxCode()
        {
            var tax = new LineItemTax(null, 105.99m);
        }
        #endregion
    }
}
