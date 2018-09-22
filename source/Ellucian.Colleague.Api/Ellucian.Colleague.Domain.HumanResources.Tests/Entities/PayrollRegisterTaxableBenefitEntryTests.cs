/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterTaxableBenefitEntryTests
    {
        public string expectedTaxableBenefitId = "LIFE";
        public string expectedTaxableBenefitTaxCode = "FWHT";
        public decimal? expectedTaxableBenefitAmt = 77.99m;
        public PayrollRegisterTaxableBenefitEntry expectedPayrollRegisterTaxableBenefitEntry;

        [TestMethod]
        public void PropertiesAreSetTest()
        {
            expectedPayrollRegisterTaxableBenefitEntry = new PayrollRegisterTaxableBenefitEntry(expectedTaxableBenefitId, expectedTaxableBenefitTaxCode, expectedTaxableBenefitAmt);
            Assert.AreEqual(expectedTaxableBenefitId, expectedPayrollRegisterTaxableBenefitEntry.TaxableBenefitId);
            Assert.AreEqual(expectedTaxableBenefitTaxCode, expectedPayrollRegisterTaxableBenefitEntry.TaxableBenefitTaxCode);
            Assert.AreEqual(expectedTaxableBenefitAmt, expectedPayrollRegisterTaxableBenefitEntry.TaxableBenefitAmt);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullBenefitIdArgumentException()
        {
            expectedPayrollRegisterTaxableBenefitEntry = new PayrollRegisterTaxableBenefitEntry(null, expectedTaxableBenefitTaxCode, expectedTaxableBenefitAmt);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxCodeArgumentException()
        {
            expectedPayrollRegisterTaxableBenefitEntry = new PayrollRegisterTaxableBenefitEntry(expectedTaxableBenefitId, null, expectedTaxableBenefitAmt);
        }
    }
}
