//Copyright 2014 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AverageAwardPackageTests
    {
        public int? averageGrantAmount;
        public int? averageLoanAmount;
        public int? averageScholarshipAmount;
        public string awardYearCode;

        public AverageAwardPackage averageAwardPackage;

        public void BaseInitialize()
        {
            averageGrantAmount = 12345;
            averageLoanAmount = 2982;
            averageScholarshipAmount = 9999;
            awardYearCode = "2014";

            averageAwardPackage = new AverageAwardPackage(averageGrantAmount, averageLoanAmount, averageScholarshipAmount, awardYearCode);
        }

        [TestClass]
        public class AverageAwardPackageConstructorTests : AverageAwardPackageTests
        {

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void AttributesAreEqualTest()
            {
                Assert.AreEqual(averageGrantAmount.Value, averageAwardPackage.AverageGrantAmount);
                Assert.AreEqual(averageLoanAmount.Value, averageAwardPackage.AverageLoanAmount);
                Assert.AreEqual(averageScholarshipAmount.Value, averageAwardPackage.AverageScholarshipAmount);
                Assert.AreEqual(awardYearCode, averageAwardPackage.AwardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public void AllAmountArgumentsNullThrowsExceptionTest()
            {
                new AverageAwardPackage(null, null, null, awardYearCode);
            }

            [TestMethod]
            public void ZeroGrantAmountWhenNullTest()
            {
                averageAwardPackage = new AverageAwardPackage(null, averageLoanAmount, averageScholarshipAmount, awardYearCode);
                Assert.AreEqual(0, averageAwardPackage.AverageGrantAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NegativeGrantAmountThrowsExceptionTest()
            {
                new AverageAwardPackage(-1, averageLoanAmount, averageScholarshipAmount, awardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NegativeLoanAmountThrowsExceptionTest()
            {
                new AverageAwardPackage(averageGrantAmount, -1, averageScholarshipAmount, awardYearCode);
            }

            [TestMethod]
            public void ZeroLoanAmountWhenNullTest()
            {
                averageAwardPackage = new AverageAwardPackage(averageGrantAmount, null, averageScholarshipAmount, awardYearCode);
                Assert.AreEqual(0, averageAwardPackage.AverageLoanAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NegativeScholarshipAmountThrowsExceptionTest()
            {
                new AverageAwardPackage(averageGrantAmount, averageLoanAmount, -1, awardYearCode);
            }
            [TestMethod]
            public void ZeroScholarshipAmountWhenNullTest()
            {
                averageAwardPackage = new AverageAwardPackage(averageGrantAmount, averageLoanAmount, null, awardYearCode);
                Assert.AreEqual(0, averageAwardPackage.AverageScholarshipAmount);
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public void NullAwardYearCodeThrowsExceptionTest()
            {
                new AverageAwardPackage(averageGrantAmount, averageLoanAmount, averageScholarshipAmount, null);
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public void EmptyStringAwardYearThrowsExceptionTest()
            {
                new AverageAwardPackage(averageGrantAmount, averageLoanAmount, averageScholarshipAmount, "");
            }
        }
    }
}
