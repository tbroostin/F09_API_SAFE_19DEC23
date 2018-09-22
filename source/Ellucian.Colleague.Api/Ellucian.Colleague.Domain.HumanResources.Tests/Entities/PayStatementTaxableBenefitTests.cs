using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementTaxableBenefitTests
    {
        public string expectedTaxableBenefitId = "FOO";
        public string expectedTaxableBenefitDescription = "foo foo";
        public decimal? expectedTaxableBenefitAmt = 55.9m;
        public decimal? expectedTaxableBenefitYtdAmt = 100.8m;

        [TestMethod]
        public void ConstructorTest()
        {
            var expectedPayStatementTaxableBenefit = new PayStatementTaxableBenefit(expectedTaxableBenefitId, expectedTaxableBenefitDescription, expectedTaxableBenefitAmt, expectedTaxableBenefitYtdAmt);
            Assert.AreEqual(expectedTaxableBenefitId, expectedPayStatementTaxableBenefit.TaxableBenefitId);
            Assert.AreEqual(expectedTaxableBenefitDescription, expectedPayStatementTaxableBenefit.TaxableBenefitDescription);
            Assert.AreEqual(expectedTaxableBenefitAmt, expectedPayStatementTaxableBenefit.TaxableBenefitAmt);
            Assert.AreEqual(expectedTaxableBenefitYtdAmt, expectedPayStatementTaxableBenefit.TaxableBenefitYtdAmt);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullBenefitIdExceptionTest()
        {
            var expectedPayStatementTaxableBenefit = new PayStatementTaxableBenefit(null, expectedTaxableBenefitDescription, expectedTaxableBenefitAmt, expectedTaxableBenefitYtdAmt);
        }
    }
}
