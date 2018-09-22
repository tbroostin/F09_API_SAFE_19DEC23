/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentAwardPeriodTests
    {
        public StudentAwardYear studentAwardYear;
        public string studentId;
        public Award award;
        public AwardCategory category;
        public string awardPeriodId;
        public decimal? awardAmount;
        public AwardStatus awardStatus;
        public bool isEligible;
        public bool isFrozen;
        public bool isTransmitted;
        public bool hasLoanDisbursements;

        public StudentAward studentAward;
        public StudentAwardPeriod studentAwardPeriod;

        public void BaseInitialize()
        {
            studentId = "0003914";
            studentAwardYear = new StudentAwardYear(studentId, "2013", new FinancialAidOffice("office"));
            studentAwardYear.CurrentOffice.AddConfiguration(new FinancialAid.Entities.FinancialAidConfiguration("office", "2013") { AllowLoanChanges = true, AllowLoanChangeIfAccepted = true});           
            category = new AwardCategory("foo", "bar", null);
            award = new Award("WOOFY", "desc", category);

            awardPeriodId = "13/FA";
            awardAmount = (decimal)420.90;
            awardStatus = new AwardStatus("S", "desc", AwardStatusCategory.Pending);
            isFrozen = false;
            isTransmitted = false;
            hasLoanDisbursements = true;

            studentAward = new StudentAward(studentAwardYear, studentId, award, false);
            studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
        }

        /// <summary>
        /// This test class tests the StudentAwardPeriod Constructor - that all the attributes are initialized
        /// and set as expected
        /// </summary>
        [TestClass]
        public class StudentAwardPeriodConstructor : StudentAwardPeriodTests
        {

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void StudentAwardEqualsTest()
            {
                Assert.AreEqual(studentAward, studentAwardPeriod.StudentAward);
            }

            [TestMethod]
            public void StudentAwardContainsPeriodTest()
            {
                Assert.IsTrue(studentAward.StudentAwardPeriods.Contains(studentAwardPeriod));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAwardRequiredTest()
            {
                new StudentAwardPeriod(null, awardPeriodId, awardStatus, false, true);
            }

            /// <summary>
            /// Test that the constructor set the awardPeriod correctly
            /// </summary>
            [TestMethod]
            public void AwardPeriodIdEqualsTest()
            {
                Assert.AreEqual(awardPeriodId, studentAwardPeriod.AwardPeriodId);
            }

            /// <summary>
            /// Test that the award period is required
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardPeriodIdRequiredTest()
            {
                new StudentAwardPeriod(studentAward, null, awardStatus, false, true);
            }

            /// <summary>
            /// Test that the constructor set the AwardStatus correctly
            /// </summary>
            [TestMethod]
            public void AwardStatusEqualsTest()
            {
                Assert.AreEqual(awardStatus, studentAwardPeriod.AwardStatus);
            }

            /// <summary>
            /// Test that the award status is not required
            /// </summary>
            [TestMethod]
            public void AwardStatusNotRequiredTest()
            {
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, null, false, true);
                Assert.IsNull(studentAwardPeriod.AwardStatus);
            }

            [TestMethod]
            public void IsFrozenEqualsTest()
            {
                Assert.AreEqual(isFrozen, studentAwardPeriod.IsFrozen);
            }

            [TestMethod]
            public void IsTransmittedEqualsTest()
            {
                Assert.AreEqual(isTransmitted, studentAwardPeriod.IsTransmitted);
            }

            [TestMethod]
            public void GetSetAwardAmount_GreaterThanZeroTest()
            {
                decimal amount = (decimal)0.1;
                studentAwardPeriod.AwardAmount = amount;
                Assert.IsTrue(studentAwardPeriod.AwardAmount.HasValue);
                Assert.AreEqual(amount, studentAwardPeriod.AwardAmount.Value);
            }

            [TestMethod]
            public void GetSetAwardAmount_ZeroTest()
            {
                decimal amount = 0;
                studentAwardPeriod.AwardAmount = amount;
                Assert.IsTrue(studentAwardPeriod.AwardAmount.HasValue);
                Assert.AreEqual(amount, studentAwardPeriod.AwardAmount.Value);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void GetSetAwardAmount_LessThanZeroThrowsExceptionTest()
            {
                decimal amount = (decimal)-0.1;
                studentAwardPeriod.AwardAmount = amount;
            }

            [TestMethod]
            public void GetSetAwardAmount_NullTest()
            {
                decimal? amount = null;
                studentAwardPeriod.AwardAmount = amount;
                Assert.IsFalse(studentAwardPeriod.AwardAmount.HasValue);
            }

            [TestMethod]
            public void GetStudentAwardYearTest()
            {
                Assert.AreEqual(studentAwardYear, studentAwardPeriod.StudentAwardYear);
            }

            [TestMethod]
            public void GetSetHasLoanDisbursementsTest()
            {
                studentAwardPeriod.HasLoanDisbursement = hasLoanDisbursements;
                Assert.AreEqual(hasLoanDisbursements, studentAwardPeriod.HasLoanDisbursement);
            }
        }


        [TestClass]
        public class StudentAwardPeriodIsIgnoredOnAwardLetterTests : StudentAwardPeriodTests 
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                awardStatus = new AwardStatus("S", "desc", AwardStatusCategory.Pending);

                award.IsFederalDirectLoan = true;
                isEligible = true;
                hasLoanDisbursements = true;

                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                studentAwardPeriod.HasLoanDisbursement = hasLoanDisbursements;

                //set the configuration so that the StudentAwardPeriod IsAmountModifiable=true by default
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = true;

                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesFromEval = new List<string>();
                studentAwardYear.CurrentConfiguration.IgnoreAwardsFromEval = new List<string>();
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesFromEval = new List<string>();
            }

            [TestMethod]
            public void IsIgnoredOnAwardLetterTests_LettersOnTest()
            {
                //Award letters are on and no ignore fields; Nothing ignored 
                Assert.IsFalse(studentAwardPeriod.IsIgnoredOnAwardLetter);
            }

            [TestMethod]
            public void IsIgnoredOnAwardLetterTests_LettersOffTest()
            {
                //Award letters are off and no ignore fields; All ignored 

                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);

                //set the configuration so that the StudentAwardPeriod IsAmountModifiable=true by default
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = false;
              
                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnAwardLetter);
            }

            [TestMethod]
            public void IsIgnoredOnAwardLetterTests_IgnoreCategoryTest()
            {
                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);

                //set the configuration so that letters are on and ignore a category
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = true;
                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesFromEval.Add("GSL");

                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnAwardLetter);
            }

            [TestMethod]
            public void IsIgnoredOnAwardLetterTests_IgnoreAwardTest()
            {
                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);

                //set the configuration so that latters are on and ignore an award
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = true;
                studentAwardYear.CurrentConfiguration.IgnoreAwardsFromEval.Add("WOOFY");

                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnAwardLetter);
            }

            [TestMethod]
            public void IsIgnoredOnAwardLetterTests_IgnoreActionStatusTest()
            {
                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);

                //set the configuration so that letters are on and ignore an action status
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = true;
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesFromEval.Add("S");

                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnAwardLetter);
            }

        }

        [TestClass]
        public class StudentAwardPeriodIsIgnoredOnChecklistTests : StudentAwardPeriodTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                awardStatus = new AwardStatus("S", "desc", AwardStatusCategory.Pending);

                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);                

                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist = new List<string>();
                studentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist = new List<string>();
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist = new List<string>();
            }

            [TestMethod]
            public void IgnoredAwardCategoriesOnChecklist_IsIgnoredOnChecklistTrueTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist.Add(category.Code);
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void IgnoredAwardsOnChecklist_IsIgnoredOnChecklistTrueTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist.Add(award.Code);
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void IgnoredAwardStatusesOnChecklist_IsIgnoredOnChecklistTrueTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist.Add(awardStatus.Code);
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsTrue(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void NothingIgnoredOnChecklist_IsIgnoredOnChecklistFalseTest()
            {
                Assert.IsFalse(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void NullIgnoredAwardCategoriesList_IsIgnoredOnChecklistFalseTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist.Add("foo");
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist.Add("X");
                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist = null;
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void NullIgnoredAwardsList_IsIgnoredOnChecklistFalseTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist = null;
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist.Add("X");
                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist.Add("BAR");
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsIgnoredOnChecklist);
            }

            [TestMethod]
            public void NullIgnoredAwardStatusesList_IsIgnoredOnChecklistFalseTest()
            {
                studentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist.Add("fooBar");
                studentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist = null;
                studentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist.Add("BAR");
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsIgnoredOnChecklist);
            }
        }


        [TestClass]
        public class StudentAwardPeriodAmountModifiableTests : StudentAwardPeriodTests
        {
            [TestInitialize]
            public void Initialize()
            {

                BaseInitialize();

                category = new AwardCategory("GSL", "bar", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                award.IsFederalDirectLoan = true;
                isEligible = true;
                hasLoanDisbursements = true;

                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                studentAwardPeriod.HasLoanDisbursement = hasLoanDisbursements;

                //set the configuration so that the StudentAwardPeriod IsAmountModifiable=true by default
                studentAwardYear.CurrentConfiguration.IsAwardingActive = true;
                studentAwardYear.CurrentConfiguration.AreAwardChangesAllowed = true;
                studentAwardYear.CurrentConfiguration.AllowLoanChangeIfAccepted = true;
                studentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromChange = new List<string>();
                studentAwardYear.CurrentConfiguration.ExcludeAwardsFromChange = new List<string>();


            }

            [TestMethod]
            public void ModifiableTest()
            {
                Assert.IsTrue(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfNullAwardLoanType()
            {
                category = new AwardCategory("FOO", "BAR", AwardCategoryType.Grant);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                studentAwardPeriod.HasLoanDisbursement = hasLoanDisbursements;
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfOtherAwardLoanType()
            {
                category = new AwardCategory("FOO", "BAR", AwardCategoryType.Loan);
                award = new Award("WOOFY", "desc", category);
                studentAward = new StudentAward(studentAwardYear, studentId, award, isEligible);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                studentAwardPeriod.HasLoanDisbursement = hasLoanDisbursements;
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfNotFederalLoan()
            {
                award.IsFederalDirectLoan = false;
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfLoanChangesDisallowedByConfiguration()
            {
                studentAward.StudentAwardYear.CurrentConfiguration.AllowLoanChanges = false;
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfAllAwardChangesAreDisallowedByConfiguration()
            {
                studentAward.StudentAwardYear.CurrentConfiguration.AreAwardChangesAllowed = false;
                Assert.IsFalse(studentAward.IsAmountModifiable);
            }

            [TestMethod]
            public void StudentAwardPeriodIsModifiable_PendingAwardStatusCategoryTest()
            {
                Assert.IsTrue(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void StudentAwardPeriodIsModifiable_EstimatedAwardStatusCategoryTest()
            {
                studentAwardPeriod.AwardStatus = new AwardStatus("E", "E", AwardStatusCategory.Estimated);
                Assert.IsTrue(studentAwardPeriod.IsAmountModifiable);
            }

            /// <summary>
            /// Clients no longer wanted to eligiblity to be a criterion for modifying award amounts.
            /// So this test checks that if a student award is not eligible, the award periods can
            /// still be modifiable.
            /// </summary>
            [TestMethod]
            public void StudentAwardPeriodIsModifiable_IfStudentAwardNotEligible()
            {
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                studentAwardPeriod.HasLoanDisbursement = true;

                Assert.IsTrue(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfNotHasLoanDisbursementsTest()
            {
                studentAwardPeriod.HasLoanDisbursement = false;
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfAwardStatusIsRejected()
            {
                studentAwardPeriod.AwardStatus = new AwardStatus("R", "R", AwardStatusCategory.Rejected);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfAwardStatusIsDenied()
            {
                studentAwardPeriod.AwardStatus = new AwardStatus("D", "D", AwardStatusCategory.Denied);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfAwardStatusIsAcceptedAndLoanChangesNotAllowedIfAcceptedTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.AllowLoanChangeIfAccepted = false;
                studentAwardPeriod.AwardStatus = new AwardStatus("A", "A", AwardStatusCategory.Accepted);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void IsModifiableIfAwardStatusIsAcceptedAndLoanChangesAreAllowedIfAcceptedTest()
            {
                studentAwardPeriod.AwardStatus = new AwardStatus("A", "A", AwardStatusCategory.Accepted);
                Assert.IsTrue(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void IsStatusModifiableIfDeclineZeroOfAcceptedLoansTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.AllowDeclineZeroOfAcceptedLoans = false;
                studentAwardPeriod.AwardStatus = new AwardStatus("A", "A", AwardStatusCategory.Accepted);
                Assert.IsFalse(studentAwardPeriod.IsStatusModifiable);
            }

            [TestMethod]
            public void StatusNotModifableIfLoanTransmittedTest()
            {
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, true);
                Assert.IsFalse(studentAwardPeriod.IsStatusModifiable);
            }

            [TestMethod]
            public void AmountModifiableIsFalseIfTransmittedTest()
            {
                studentAward = new StudentAward(studentAwardYear, studentId, award, true);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, true);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }


            [TestMethod]
            public void NotModifiableIfFrozen()
            {
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, true, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

            [TestMethod]
            public void NotModifiableIfTransmitted()
            {
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, true);
                Assert.IsFalse(studentAwardPeriod.IsAmountModifiable);
            }

        }

        [TestClass]
        public class StudentAwardPeriodIsViewableTests : StudentAwardPeriodTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                //Initialize so IsViewable == true 
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsSelfServiceActive = true;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsAwardingActive = true;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesView = new List<string>();
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardsView = new List<string>();
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesView = new List<AwardStatusCategory>();
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriods = new List<string>();
            }

            //[TestMethod]
            //public void IsViewable_NotViewableWhenConfigurationIsNullTest()
            //{
            //    studentAwardPeriod.StudentAwardYear.CurrentOffice.Configurations = null;
            //    Assert.IsFalse(studentAwardPeriod.IsViewable);

            //}

            [TestMethod]
            public void IsViewable_ConfigurationIsBlankTest()
            {
                Assert.IsTrue(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_IsAwardingActiveIsFalseIsNotViewableTest()
            {
                studentAward.StudentAwardYear.CurrentConfiguration.IsAwardingActive = false;
                Assert.IsFalse(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_ViewableWhenExclusionListsAreNullTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesView = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardsView = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesView = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriods = null;
                Assert.IsTrue(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_NotViewableWhenAwardCategoryExcludedTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesView.Add(studentAward.Award.AwardCategory.Code);
                Assert.IsFalse(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_NotViewableWhenAwardExcludedTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardsView.Add(studentAward.Award.Code);
                Assert.IsFalse(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_NotViewableWhenExclusionListContainsAwardStatusCategoryTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesView.Add(studentAwardPeriod.AwardStatus.Category);
                Assert.IsFalse(studentAwardPeriod.IsViewable);
            }

            [TestMethod]
            public void IsViewable_NotViewableWhenExclusionListContainsAwardPeriodTest()
            {
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriods.Add(studentAwardPeriod.AwardPeriodId);
                Assert.IsFalse(studentAwardPeriod.IsViewable);
            }
        }

        [TestClass]
        public class StudentAwardPeriodIsViewableOnAwardLetterAndShoppingSheetTests : StudentAwardPeriodTests
        {

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsAwardLetterActive = true;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.IsShoppingSheetActive = true;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = null;
                studentAwardPeriod.StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = null;
            }

            [TestMethod]
            public void IsViewableOnAwardLetterAndShoppingSheet_ReturnsTrueTest()
            {
                Assert.IsTrue(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void IsAwardLetterActive_IsViewableOnAwardLetterAndShoppingSheet_ReturnsTrueTest()
            {
                studentAwardYear.CurrentConfiguration.IsShoppingSheetActive = false;
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsTrue(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void IsShoppingSheetActive_IsViewableOnAwardLetterAndShoppingSheet_ReturnsTrueTest()
            {
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = false;
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsTrue(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void AwardLetterShoppingSheetInactive_IsViewableOnAwardLetterAndShoppingSheet_ReturnsFalseTest()
            {
                studentAwardYear.CurrentConfiguration.IsShoppingSheetActive = false;
                studentAwardYear.CurrentConfiguration.IsAwardLetterActive = false;
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void NoConfiguration_IsViewableOnAwardLetterAndShoppingSheet_ReturnsFalseTest()
            {
                studentAwardYear = new StudentAwardYear(studentId, "2013", new FinancialAidOffice("office"));
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            /// <summary>
            /// Tests if the boolean is set to false if the student award period is contained in the
            /// list of award status categories to be excluded in the current year's configuration
            /// </summary>
            [TestMethod]
            public void ExcludeAwardStatusCategory_IsViewableOnAwardLetterAndShoppingSheetReturnsFalseTest()
            {
                var awardStatusCategoryToExclude = awardStatus.Category;
                studentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>()
                {
                   awardStatusCategoryToExclude
                };
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            /// <summary>
            /// Tests if the boolean is set to false if the student award period is contained in the
            /// list of award codes to be excluded in the current year's configuration
            /// </summary>
            [TestMethod]
            public void ExcludeAwards_IsViewableOnAwardLetterAndShoppingSheetReturnsFalseTest()
            {
                var awardCodeToExclude = award.Code;
                studentAwardYear.CurrentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = new List<string>()
                {
                   awardCodeToExclude
                };
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            /// <summary>
            /// Tests if the boolean is set to false if the student award period is contained in the
            /// list of award categories to be excluded in the current year's configuration
            /// </summary>
            [TestMethod]
            public void ExcludeAwardCategories_IsViewableOnAwardLetterAndShoppingSheetReturnsFalseTest()
            {
                var awardCategoryToExclude = award.AwardCategory.Code;
                studentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = new List<string>()
                {
                   awardCategoryToExclude
                };
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }

            /// <summary>
            /// Tests if the boolean is set to false if the student award period is contained in the
            /// list of award periods to be excluded in the current year's configuration
            /// </summary>
            [TestMethod]
            public void ExcludeAwardPeriods_IsViewableOnAwardLetterAndShoppingSheetReturnsFalseTest()
            {
                var awardPeriodToExclude = studentAwardPeriod.AwardPeriodId;
                studentAwardYear.CurrentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = new List<string>()
                {
                   awardPeriodToExclude
                };
                studentAward = new StudentAward(studentAwardYear, studentId, award, false);
                studentAwardPeriod = new StudentAwardPeriod(studentAward, awardPeriodId, awardStatus, isFrozen, isTransmitted);
                Assert.IsFalse(studentAwardPeriod.IsViewableOnAwardLetterAndShoppingSheet);
            }
        }

        [TestClass]
        public class StudentAwardPeriodEqualsTests
        {

            private StudentAward studentAward1;
            private StudentAward studentAward2;

            private StudentAwardYear studentAwardYear;
            private string studentId;
            private AwardCategory category;
            private Award award;

            private string awardPeriodId;
            private AwardStatus awardStatus;

            private StudentAwardPeriod studentAwardPeriod1;
            private StudentAwardPeriod studentAwardPeriod2;

            [TestInitialize]
            public void Initialize()
            {

                studentId = "0003914";
                studentAwardYear = new StudentAwardYear(studentId, "2013", new FinancialAidOffice("office"));
                category = new AwardCategory("foo", "bar", null);
                award = new Award("WOOFY", "desc", null);
                awardPeriodId = "12/FA";
                awardStatus = new AwardStatus("A", "desc", AwardStatusCategory.Accepted);

                studentAward1 = new StudentAward(studentAwardYear, studentId, award, true);
                studentAward2 = new StudentAward(studentAwardYear, studentId, award, false);

                studentAwardPeriod1 = new StudentAwardPeriod(studentAward1, awardPeriodId, awardStatus, false, false);
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, awardPeriodId, awardStatus, false, true);

            }

            [TestMethod]
            public void StudentAwardPeriodsAreEqual()
            {
                Assert.AreEqual(studentAwardPeriod1, studentAwardPeriod2);
            }

            [TestMethod]
            public void StudentAwardPeriodsNotEqual_StudentAwardsNotEqual()
            {
                studentAward2 = new StudentAward(studentAwardYear, "foobar", award, false);
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, awardPeriodId, awardStatus, true, false);

                Assert.AreNotEqual(studentAwardPeriod1, studentAwardPeriod2);
            }

            [TestMethod]
            public void StudentAwardPeriodsNotEqual_AwardPeriodsNotEqual()
            {
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, "foobar", awardStatus, true, true);
                Assert.AreNotEqual(studentAwardPeriod1, studentAwardPeriod2);
            }

            [TestMethod]
            public void StudentAwardPeriodsNotEqual_NullStudentAwardPeriod()
            {
                studentAwardPeriod2 = null;
                Assert.AreNotEqual(studentAwardPeriod1, studentAwardPeriod2);
            }

            [TestMethod]
            public void StudentAwardPeriodsNotEqual_NotStudentAwardPeriodObject()
            {
                Assert.AreNotEqual(studentAwardPeriod1, new Object());
            }
        }

        [TestClass]
        public class StudentAwardPeriodGetHashCodeTests
        {
            private StudentAward studentAward1;
            private StudentAward studentAward2;

            private StudentAwardYear studentAwardYear;
            private string studentId;
            private AwardCategory category;
            private Award award;

            private string awardPeriodId;
            private AwardStatus awardStatus;

            private StudentAwardPeriod studentAwardPeriod1;
            private StudentAwardPeriod studentAwardPeriod2;

            [TestInitialize]
            public void Initialize()
            {

                studentId = "0003914";
                studentAwardYear = new StudentAwardYear(studentId, "2013", new FinancialAidOffice("office"));
                category = new AwardCategory("foo", "Bar", null);
                award = new Award("WOOFY", "desc", category);
                awardPeriodId = "12/FA";
                awardStatus = new AwardStatus("A", "desc", AwardStatusCategory.Accepted);

                studentAward1 = new StudentAward(studentAwardYear, studentId, award, true);
                studentAward2 = new StudentAward(studentAwardYear, studentId, award, false);

                studentAwardPeriod1 = new StudentAwardPeriod(studentAward1, awardPeriodId, awardStatus, false, false);
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, awardPeriodId, awardStatus, false, true);
            }

            [TestMethod]
            public void EqualObjectsEqualHashCodesTest()
            {
                Assert.AreEqual(studentAwardPeriod1.GetHashCode(), studentAwardPeriod2.GetHashCode());
            }

            [TestMethod]
            public void UnequalStudentAwardUnequalHashCodesTest()
            {
                studentAward2 = new StudentAward(new StudentAwardYear(studentId, "foobar", new FinancialAidOffice("office")), studentId, award, true);
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, awardPeriodId, awardStatus, false, true);

                Assert.AreNotEqual(studentAwardPeriod1.GetHashCode(), studentAwardPeriod2.GetHashCode());
            }

            [TestMethod]
            public void UnequalStudentAwardPeriodsUnequalHashCodesTest()
            {
                studentAwardPeriod2 = new StudentAwardPeriod(studentAward2, "foobar", awardStatus, false, true);
                Assert.AreNotEqual(studentAwardPeriod1.GetHashCode(), studentAwardPeriod2.GetHashCode());
            }
        }

    }
}
