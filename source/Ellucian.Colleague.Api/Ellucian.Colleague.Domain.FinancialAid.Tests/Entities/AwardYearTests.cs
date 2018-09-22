using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Tests the AwardYear domain object.
    /// </summary>
    [TestClass]
    public class AwardYearTests
    {
        /// <summary>
        /// Tests the constructor of the AwardYear object. The constructor
        /// sets the attributes equal to the value of the constructor parameters, except when
        /// the description parameter is null or empty. When that is the case, the constructor
        /// sets the description equal to the value of the code.
        /// </summary>
        [TestClass]
        public class AwardYearConstructor
        {
            /// <summary>
            /// AwardYear code
            /// </summary>
            private string code;

            /// <summary>
            /// AwardYear description
            /// </summary>
            private string description;

            /// <summary>
            /// AwardYear object for testing
            /// </summary>
            private AwardYear awardYear;

            [TestInitialize]
            public void Initialize()
            {
                code = "2013";
                description = "2013/2014 AwardYear";
                awardYear = new AwardYear(code, description);
            }

            /// <summary>
            /// Tests that the constructor set the code correctly.
            /// </summary>
            [TestMethod]
            public void AwardYearCode()
            {
                Assert.AreEqual(code, awardYear.Code);
            }

            /// <summary>
            /// Tests that the constructor set the description correctly
            /// </summary>
            [TestMethod]
            public void AwardYearDescription()
            {
                Assert.AreEqual(description, awardYear.Description);
            }

            /// <summary>
            /// Tests that a NullException is thrown when a null code is passed in.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearCodeNullException()
            {
                new AwardYear(null, description);
            }

            /// <summary>
            /// Test that a NullException is thrown when a null description is passed in
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearDescriptionNotRequired()
            {
                var newYear = new AwardYear(code, null);
            }

            [TestMethod]
            public void AwardYearAsNumberTest()
            {
                int expectedNumber;
                Assert.IsTrue(int.TryParse(code, out expectedNumber));

                Assert.AreEqual(expectedNumber, awardYear.YearAsNumber);
            }

            [TestMethod]
            public void BadAwardYearReturnsNegatvieOneTest()
            {
                awardYear = new AwardYear("foobar", "test");
                Assert.AreEqual(-1, awardYear.YearAsNumber);
            }

        }

        /// <summary>
        /// Tests the equals method of the AwardYear class. Two AwardYear objects are equal
        /// when their codes are equal, regardless of the object's description attribute.
        /// </summary>
        [TestClass]
        public class AwardYearEquals
        {
            private string code1;
            private string desc1;

            private string code2;
            private string desc2;

            /// <summary>
            /// Test AwardYear 1
            /// </summary>
            private AwardYear awardYear1;

            /// <summary>
            /// Test AwardYear 2 has the same code as AwardYear 1 but different description.
            /// </summary>
            private AwardYear awardYear2;

            /// <summary>
            /// Test AwardYear 3 has a different code than AwardYear 1 but the same description.
            /// </summary>
            private AwardYear awardYear3;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "2013";
                desc1 = "2013/2014 Award Year";

                code2 = "2014";
                desc2 = "2014/2015 Award Year";

                awardYear1 = new AwardYear(code1, desc1);
                awardYear2 = new AwardYear(code1, desc2);
                awardYear3 = new AwardYear(code2, desc1);
            }

            /// <summary>
            /// Test that the Equals method returns true when comparing two AwardYears with the same
            /// codes but different descriptions
            /// </summary>
            [TestMethod]
            public void AwardYearCodesEqual()
            {
                Assert.IsTrue(awardYear1.Equals(awardYear2));
            }

            /// <summary>
            /// Test that the Equals method returns false when comparing two AwardYears with different
            /// codes but same descriptions.
            /// </summary>
            [TestMethod]
            public void AwardYearCodesNotEqual()
            {
                Assert.IsFalse(awardYear1.Equals(awardYear3));
            }
        }
    }
}
