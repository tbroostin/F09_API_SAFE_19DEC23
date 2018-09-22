/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// This test class contains test sub classes for the StudentAward and StudentAwardPeriod domain
    /// objects
    /// </summary>
    [TestClass]
    public class StudentAwardTests
    {

        public StudentAwardYear studentAwardYear;
        public string studentId;
        public Award award;
        public AwardCategory category;
        public List<StudentAwardPeriod> studentAwardPeriods;
        public bool isEligible;

        public StudentAward studentAward;

        public void BaseInitialize()
        {

            studentId = "0003914";
            studentAwardYear = new StudentAwardYear(studentId, "2013", new FinancialAidOffice("office"));
            studentAwardYear.CurrentOffice.AddConfiguration(new FinancialAid.Entities.FinancialAidConfiguration("office", "2013") { AllowLoanChanges = true, AreAwardChangesAllowed = true });
            category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
            award = new Award("WOOFY", "desc", category);
            studentAwardPeriods = new List<StudentAwardPeriod>();
            isEligible = true;

            studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
        }

        /// <summary>
        /// This class tests the StudentAward constructor - that all attributes are set and 
        /// initialized as expected
        /// </summary>
        [TestClass]
        public class StudentAwardConstructorTests : StudentAwardTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            /// <summary>
            /// Test that the constructor set the AwardYear correctly
            /// </summary>
            [TestMethod]
            public void AwardYearEqualTest()
            {
                Assert.AreEqual(studentAwardYear, studentAward.StudentAwardYear);
            }

            /// <summary>
            /// Test that the constructor set the studentId correctly
            /// </summary>
            [TestMethod]
            public void StudentIdEqualTest()
            {
                Assert.AreEqual(studentId, studentAward.StudentId);
            }

            /// <summary>
            /// Test that the constructor set the awardId correctly
            /// </summary>
            [TestMethod]
            public void AwardIdEqualTest()
            {
                Assert.AreEqual(award, studentAward.Award);
            }

            /// <summary>
            /// Test that the constructor initializes the StudentAwardPeriods list.
            /// </summary>
            [TestMethod]
            public void StudentAwardPeriodListInitializedTest()
            {
                Assert.IsTrue(studentAward.StudentAwardPeriods != null);
                Assert.AreEqual(0, studentAward.StudentAwardPeriods.Count);
            }

            [TestMethod]
            public void IsEligibleEqualTest()
            {
                Assert.AreEqual(isEligible, studentAward.IsEligible);
            }

            /// <summary>
            /// Test that the award year argument is required
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullAwardYearExceptionTest()
            {
                new StudentAward(null, studentId, award, isEligible);
            }

            /// <summary>
            /// Test that the studentId is required
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullStudentIdExceptionTest()
            {
                new StudentAward(studentAwardYear, "", award, isEligible);
            }

            /// <summary>
            /// Test that the Award is required.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullAwardExceptionTest()
            {
                new StudentAward(studentAwardYear, studentId, null, isEligible);
            }

            [TestMethod]
            public void PendingChangeRequestId_InitializedToEmptyTest()
            {
                Assert.AreEqual(string.Empty, studentAward.PendingChangeRequestId);
            }
        }

        [TestClass]
        public class StudentAwardModifiableTests : StudentAwardTests
        {
            private FinancialAid.Entities.FinancialAidConfiguration configuration;
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                configuration = studentAwardYear.CurrentConfiguration;
                configuration.IsAwardingActive = true;
                configuration.AllowLoanChanges = true;
                configuration.AreAwardChangesAllowed = true;
                configuration.ExcludeAwardCategoriesFromChange = new List<string>();
                configuration.ExcludeAwardsFromChange = new List<string>();
                award.IsFederalDirectLoan = true;
                var awardStatus = new AwardStatus("f", "d", AwardStatusCategory.Pending);
                new StudentAwardPeriod(studentAward, "foo", awardStatus, false, false) { HasLoanDisbursement = true };
                new StudentAwardPeriod(studentAward, "bar", awardStatus, false, false) { HasLoanDisbursement = true };

            }

            [TestMethod]
            public void IsModifiableTest()
            {
                Assert.IsTrue(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void IsModifiableIfAtLeastOneAwardPeriodIsModifiable()
            {
                studentAward.StudentAwardPeriods[0].HasLoanDisbursement = false;
                Assert.IsFalse(studentAward.StudentAwardPeriods[0].IsAmountModifiable);
                Assert.IsTrue(studentAward.StudentAwardPeriods[1].IsAmountModifiable);
                Assert.IsTrue(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void IsNotModifiableIfNoAwardPeriodsAreModifiable()
            {
                award.IsFederalDirectLoan = false;
                Assert.IsFalse(studentAward.StudentAwardPeriods[0].IsAmountModifiable);
                Assert.IsFalse(studentAward.StudentAwardPeriods[1].IsAmountModifiable);
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfNotLoan()
            {
                category = new AwardCategory("WOOFY", "bar", AwardCategoryType.Work);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfOtherLoan()
            {
                category = new AwardCategory("foo", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfNotFederalDirectLoan()
            {
                award.IsFederalDirectLoan = false;
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }
        }

        [TestClass]
        public class StudentAwardIsViewableTests : StudentAwardTests
        {
            private FinancialAid.Entities.FinancialAidConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                configuration = studentAwardYear.CurrentConfiguration;
                configuration.IsAwardingActive = true;
                var awardStatus = new AwardStatus("f", "d", AwardStatusCategory.Pending);
                new StudentAwardPeriod(studentAward, "foo", awardStatus, false, false);
                new StudentAwardPeriod(studentAward, "bar", awardStatus, false, false);

            }

            [TestMethod]
            public void IsViewableIfAtLeastOneStudentAwardPeriodIsViewable()
            {
                configuration.ExcludeAwardPeriods = new List<string>() { "foo" };
                Assert.IsFalse(studentAward.StudentAwardPeriods[0].IsViewable);
                Assert.IsTrue(studentAward.StudentAwardPeriods[1].IsViewable);
                Assert.IsTrue(studentAward.IsViewable);
            }

            [TestMethod]
            public void IsNotViewableIfNoStudentAwardPeriodsAreViewable()
            {
                configuration.IsAwardingActive = false;
                Assert.IsFalse(studentAward.StudentAwardPeriods[0].IsViewable);
                Assert.IsFalse(studentAward.StudentAwardPeriods[1].IsViewable);
                Assert.IsFalse(studentAward.IsViewable);
            }



        }

        /// <summary>
        /// This class tests the override Equals method of the StudentAward.
        /// Two StudentAwards are equal when they have the same year, studentid and awardid.
        /// </summary>
        [TestClass]
        public class StudentAwardEqualsTests
        {
            private StudentAwardYear studentAwardYear1;
            private StudentAwardYear studentAwardYear2;

            private FinancialAidOffice currentOffice;

            private string studentId1;
            private string studentId2;

            private Award award1;
            private Award award2;

            private AwardCategory category;

            private StudentAward studentAward1;
            private StudentAward studentAward2;
            private StudentAward studentAward3;
            private StudentAward studentAward4;
            private StudentAward studentAward5;

            [TestInitialize]
            public void Initialize()
            {
                studentId1 = "0003914";
                studentId2 = "0003915";

                currentOffice = new FinancialAidOffice("office");

                studentAwardYear1 = new StudentAwardYear(studentId1, "2013", currentOffice);
                studentAwardYear2 = new StudentAwardYear(studentId1, "2014", currentOffice);

                category = new AwardCategory("foo", "bar", null);

                award1 = new Award("WOOFY", "Desc", category);
                award2 = new Award("SUBDL", "Desc", category);

                studentAward1 = new StudentAward(studentAwardYear1, studentId1, award1, true);
                studentAward2 = new StudentAward(studentAwardYear2, studentId1, award1, true);
                studentAward3 = new StudentAward(studentAwardYear1, studentId2, award1, true);
                studentAward4 = new StudentAward(studentAwardYear1, studentId1, award2, true);
                studentAward5 = new StudentAward(studentAwardYear1, studentId1, award1, true);
            }

            /// <summary>
            /// Two StudentAwards with different award years are not equal
            /// </summary>
            [TestMethod]
            public void DiffAwardYearNotEqualTest()
            {
                Assert.IsFalse(studentAward1.Equals(studentAward2));
            }

            /// <summary>
            /// Two StudentAwards with different student ids are not equal
            /// </summary>
            [TestMethod]
            public void DiffStudentIdNotEqualTest()
            {
                Assert.IsFalse(studentAward1.Equals(studentAward3));
            }

            /// <summary>
            /// Two StudentAwards with different award ids are not equal
            /// </summary>
            [TestMethod]
            public void DiffAwardIdNotEqualTest()
            {
                Assert.IsFalse(studentAward1.Equals(studentAward4));
            }

            /// <summary>
            /// Two StudentAwards with the same year, awardid and studentid are equal.
            /// </summary>
            [TestMethod]
            public void SameAttributesIsEqualTest()
            {
                Assert.IsTrue(studentAward1.Equals(studentAward5));
            }

            /// <summary>
            /// An object not of type StudentAward is not equal to a StudentAward object.
            /// </summary>
            [TestMethod]
            public void OtherObjectTypeNotEqualTest()
            {
                var obj = new Object();
                Assert.IsFalse(studentAward1.Equals(obj));
            }

        }

        /// <summary>
        /// This class tests the override GetHashCode method of StudentAwards.
        /// Two StudentAwards have the same HashCode if they are equal
        /// </summary>
        [TestClass]
        public class StudentAwardGetHashCodeTests
        {
            private StudentAwardYear studentAwardYear1;
            private StudentAwardYear studentAwardYear2;

            private string studentId;
            private AwardCategory category;
            private Award award;

            private FinancialAidOffice currentOffice;

            private StudentAward StudentAward1;
            private StudentAward StudentAward2;
            private StudentAward StudentAward3;

            [TestInitialize]
            public void Initialize()
            {

                studentId = "0003914";
                currentOffice = new FinancialAidOffice("Office");
                studentAwardYear1 = new StudentAwardYear(studentId, "2013", currentOffice);
                studentAwardYear2 = new StudentAwardYear(studentId, "2014", currentOffice);
                category = new AwardCategory("foo", "bar", null);
                award = new Award("WOOFY", "Desc", category);
                StudentAward1 = new StudentAward(studentAwardYear1, studentId, award, true);
                StudentAward2 = new StudentAward(studentAwardYear2, studentId, award, true);
                StudentAward3 = new StudentAward(studentAwardYear1, studentId, award, true);
            }

            /// <summary>
            /// Unequal StudentAwards yield unequal HashCodes
            /// </summary>
            [TestMethod]
            public void UnequalObjectsUnequalHashCodesTest()
            {
                Assert.AreNotEqual(StudentAward1.GetHashCode(), StudentAward2.GetHashCode());
            }

            /// <summary>
            /// Equal StudentAwards yield equal HashCodes
            /// </summary>
            [TestMethod]
            public void EqualObjectsEqualHashCodesTest()
            {
                Assert.AreEqual(StudentAward1.GetHashCode(), StudentAward3.GetHashCode());
            }
        }
    }
}

