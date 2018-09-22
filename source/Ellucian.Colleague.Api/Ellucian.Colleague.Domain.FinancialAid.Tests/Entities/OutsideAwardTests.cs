/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class OutsideAwardTests
    {
        [TestClass]
        public class OutsideAwardControllerWithIdTests
        {
            private OutsideAward outsideAward;

            private string studentId;
            private string id;
            private string awardName;
            private string awardType;
            private string fundingSource;
            private decimal awardAmount;
            private string awardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0004791";
                id = "1";
                awardName = "Awesome award";
                awardType = "grant";
                fundingSource = "Gym member";
                awardAmount = 234.67m;
                awardYear = "2016";

                outsideAward = new OutsideAward(id, studentId, awardYear, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            public void OutsideAwardIsNotNullTest()
            {
                Assert.IsNotNull(outsideAward);
            }

            [TestMethod]
            public void StudentId_EqualsExpectedTest()
            {
                Assert.AreEqual(studentId, outsideAward.StudentId);
            }

            [TestMethod]
            public void Id_EqualsExpectedTest()
            {
                Assert.AreEqual(id, outsideAward.Id);
            }

            [TestMethod]
            public void AwardYearCode_EqualsExpectedTest()
            {
                Assert.AreEqual(awardYear, outsideAward.AwardYearCode);
            }

            [TestMethod]
            public void AwardName_EqualsExpectedTest()
            {
                Assert.AreEqual(awardName, outsideAward.AwardName);
            }

            [TestMethod]
            public void AwardType_EqualsExpectedTest()
            {
                Assert.AreEqual(awardType, outsideAward.AwardType);
            }

            [TestMethod]
            public void AwardAmount_EqualsExpectedTest()
            {
                Assert.AreEqual(awardAmount, outsideAward.AwardAmount);
            }

            [TestMethod]
            public void AwardFundingSource_EqualsExpectedTest()
            {
                Assert.AreEqual(fundingSource, outsideAward.AwardFundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(null, studentId, awardYear, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(id, null, awardYear, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(id, studentId, null, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardNameIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(id, studentId, awardYear, null, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardTypeIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(id, studentId, awardYear, awardName, null, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AwardAmountIsLessThanOrEqualToZero_ArgumentExceptionThrownTest()
            {
                new OutsideAward(id, studentId, awardYear, awardName, awardType, -4, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardFundingSourceIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(id, studentId, awardYear, awardName, awardType, awardAmount, null);
            }
        }

        [TestClass]
        public class OutsideAwardControllerWithoutIdTests{

            private OutsideAward outsideAward;

            private string studentId;
            private string awardName;
            private string awardType;
            private string fundingSource;
            private decimal awardAmount;
            private string awardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0004791";
                awardName = "Awesome award";
                awardType = "grant";
                fundingSource = "Gym member";
                awardAmount = 234.67m;
                awardYear = "2016";

                outsideAward = new OutsideAward(studentId, awardYear, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            public void OutsideAwardIsNotNullTest()
            {
                Assert.IsNotNull(outsideAward);
            }

            [TestMethod]
            public void StudentId_EqualsExpectedTest()
            {
                Assert.AreEqual(studentId, outsideAward.StudentId);
            }

            [TestMethod]
            public void AwardYearCode_EqualsExpectedTest()
            {
                Assert.AreEqual(awardYear, outsideAward.AwardYearCode);
            }

            [TestMethod]
            public void AwardName_EqualsExpectedTest()
            {
                Assert.AreEqual(awardName, outsideAward.AwardName);
            }

            [TestMethod]
            public void AwardType_EqualsExpectedTest()
            {
                Assert.AreEqual(awardType, outsideAward.AwardType);
            }

            [TestMethod]
            public void AwardAmount_EqualsExpectedTest()
            {
                Assert.AreEqual(awardAmount, outsideAward.AwardAmount);
            }

            [TestMethod]
            public void AwardFundingSource_EqualsExpectedTest()
            {
                Assert.AreEqual(fundingSource, outsideAward.AwardFundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(null, awardYear, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(studentId, null, awardName, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardNameIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(studentId, awardYear, null, awardType, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardTypeIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(studentId, awardYear, awardName, null, awardAmount, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AwardAmountIsLessThanOrEqualToZero_ArgumentExceptionThrownTest()
            {
                new OutsideAward(studentId, awardYear, awardName, awardType, -4, fundingSource);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardFundingSourceIsNull_ArgumentNullExceptionThrownTest()
            {
                new OutsideAward(studentId, awardYear, awardName, awardType, awardAmount, null);
            }
        }
    }
}
