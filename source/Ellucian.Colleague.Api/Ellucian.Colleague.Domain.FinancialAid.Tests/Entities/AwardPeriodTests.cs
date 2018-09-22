using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Constructor for the AwardPeriod
    /// </summary>
    [TestClass]
    public class AwardPeriodConstructor
    {
        private string awdpdCode;
        private string awdpdDesc;
        private DateTime awdpdStartDate;

        private AwardPeriod awardPeriod;

        /// <summary>
        /// Create an AwardPeriod
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            awdpdCode = "14/FA";
            awdpdDesc = "2014/2015 Fall";
            awdpdStartDate = DateTime.Now;
            awardPeriod = new AwardPeriod(awdpdCode, awdpdDesc, awdpdStartDate);

        }

        /// <summary>
        /// Tests that the constructor set the code correctly.
        /// </summary>
        [TestMethod]
        public void AwardPeriodCodeTest()
        {
            Assert.AreEqual(awdpdCode, awardPeriod.Code);
        }

        /// <summary>
        /// Tests that the constructor set the description correctly
        /// </summary>
        [TestMethod]
        public void AwardPeriodDescription()
        {
            Assert.AreEqual(awdpdDesc, awardPeriod.Description);
        }

        

        /// <summary>
        /// Tests that a NullException is thrown when a null code is passed in.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardPeriodCodeNullException()
        {
            new AwardPeriod(null, awdpdDesc, awdpdStartDate);
        }

        /// <summary>
        /// Test that a NullException is thrown when a null description is passed in
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardPeriodDescriptionRequired()
        {
            var newAwdpd = new AwardPeriod(awdpdCode, null, awdpdStartDate);
        }
    }

    /// <summary>
    /// Tests the equals method of the AwardPeriod class. Two AwardPeriod objects are equal
    /// when their codes are equal, regardless of the object's description attribute.
    /// </summary>
    [TestClass]
    public class AwardYearEquals
    {
        private string awdpd1;
        private string desc1;
        private DateTime startDt1;

        private string awdpd2;
        private string desc2;
        private DateTime startDt2;

        /// <summary>
        /// Test AwardPeriod 1
        /// </summary>
        private AwardPeriod awardPeriod1;

        /// <summary>
        /// Test AwardPeriod 2 has the same code as AwardPeriod 1 but different description.
        /// </summary>
        private AwardPeriod awardPeriod2;

        /// <summary>
        /// Test AwardPeriod 3 has a different code than AwardPeriod 1 but the same description.
        /// </summary>
        private AwardPeriod awardPeriod3;

        [TestInitialize]
        public void Initialize()
        {
            awdpd1 = "14/FA";
            desc1 = "2014/2015 Fall";
            startDt1 = DateTime.Now;  

            awdpd2 = "15/WI";
            desc2 = "2014/2015 Winter";
            startDt2 = DateTime.Now;

            awardPeriod1 = new AwardPeriod(awdpd1, desc1, startDt1);
            awardPeriod2 = new AwardPeriod(awdpd1, desc2, startDt1);
            awardPeriod3 = new AwardPeriod(awdpd2, desc1, startDt2);
        }

        /// <summary>
        /// Test that the Equals method returns true when comparing two AwardPeriods with the same
        /// codes but different descriptions
        /// </summary>
        [TestMethod]
        public void AwardPeriodCodesEqual()
        {
            Assert.IsTrue(awardPeriod1.Equals(awardPeriod2));
        }

        /// <summary>
        /// Test that the Equals method returns false when comparing two AwardPeriods with different
        /// codes but same descriptions.
        /// </summary>
        [TestMethod]
        public void AwardPeriodCodesNotEqual()
        {
            Assert.IsFalse(awardPeriod1.Equals(awardPeriod3));
        }
    }
 
}
