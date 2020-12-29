/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidConfigurationTests
    {
        [TestClass]
        public class ConfigurationConstructorTests
        {
            private List<string> activeProfileAwardYears;
            private List<string> hubAvailableYears;

            private string awardYear;
            private string awardYearDescription;

            private bool IsSelfServiceActive;
            private bool IsAwardingActive;
            private bool AreAwardChangesAllowed;
            private bool AllowAnnualAwardChangesOnly;
            private bool IsProfileActive;
            private bool IsShoppingSheetActive;
            private bool IsSatisfactoryAcademicProgressActive;

            private List<string> ExcludeAwardPeriods;
            private List<AwardStatusCategory> ExcludeAwardStatusCategoriesView;
            private List<string> ExcludeAwardCategoriesView;
            private List<string> ExcludeAwardsView;

            private List<AwardStatusCategory> ExcludeAwardStatusCategoriesFromChange;
            private List<AwardStatusCategory> ExcludeActionCategoriesFromAwardLetterAndShoppingSheet;
            private List<string> ExcludeAwardsFromAwardLetterAndShoppingSheet;
            private List<string> ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet;
            private List<string> ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet;
            private ShoppingSheetConfiguration ShoppingSheetConfiguration;
            private List<string> ExcludeAwardCategoriesFromChange;
            private List<string> ExcludeAwardsFromChange;
            private List<string> ExcludeAwardStatusesFromChange;
            private List<string> ChecklistItemCodes;
            private List<string> ChecklistItemControlStatuses;
            private List<string> ChecklistItemDefaultFlags;
            private List<string> IgnoreAwardStatusesFromEval;
            private List<string> IgnoreAwardCategoriesFromEval;
            private List<string> IgnoreAwardsFromEval;
            private List<string> IgnoreAwardStatusesOnChecklist;
            private List<string> IgnoreAwardCategoriesOnChecklist;
            private List<string> IgnoreAwardsOnChecklist;

            private string AcceptedAwardStatusCode;
            private string AcceptedAwardCommunicationCode;
            private string AcceptedAwardCommunicationStatus;
            private string RejectedAwardStatusCode;
            private string RejectedAwardCommunicationCode;
            private string RejectedAwardCommunicationStatus;

            private bool AllowUnmetNeedBorrowing;
            private bool AllowLoanChanges;
            private bool AllowLoanChangeIfAccepted;
            private bool SuppressInstanceData;

            private string NewLoanCommunicationCode;
            private string NewLoanCommunicationStatus;
            private string LoanChangeCommunicationCode;
            private string LoanChangeCommunicationsStatus;
            private string PaperCopyOptionText;

            private bool IsLoanAmountChangeRequestRequired;
            private bool IsDeclinedStatusChangeRequestRequired;
            private bool IsAwardLetterHistoryActive;
            private bool SuppressMaximumLoanLimits;
            private bool UseDocumentStatusDescription;
            private bool SuppressAccountSummaryDisplay;
            private bool SuppressAverageAwardPackageDisplay;
            private bool SuppressAwardLetterAcceptance;
            private bool SuppressDisbursementInfoDisplay;

            private string CounselorPhoneType;


            private FinancialAid.Entities.FinancialAidConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                activeProfileAwardYears = new List<string>() { "2012", "2013" };
                hubAvailableYears = new List<string>() { "2013" };

                awardYear = "2015";
                awardYearDescription = "2014-15 Academic Year";

                IsSelfServiceActive = true;
                IsAwardingActive = true;
                AreAwardChangesAllowed = true;
                AllowAnnualAwardChangesOnly = true;
                IsProfileActive = true;
                IsShoppingSheetActive = true;


                ExcludeAwardPeriods = new List<string>() { "14/FA", "13/FA" };
                ExcludeAwardCategoriesView = new List<string>() { "GSL" };
                ExcludeAwardsView = new List<string>() { "ACG1", "ACG2" };
                ExcludeAwardStatusCategoriesView = new List<AwardStatusCategory>() { AwardStatusCategory.Rejected, AwardStatusCategory.Denied };

                ExcludeAwardsFromChange = new List<string>() { "SUB", "PELL" };
                ExcludeAwardStatusCategoriesFromChange = new List<AwardStatusCategory>() { AwardStatusCategory.Estimated };
                ExcludeAwardCategoriesFromChange = new List<string>() { "GSL", "PELL" };
                ExcludeAwardStatusesFromChange = new List<string>() { "Z", "Y" };
                ChecklistItemCodes = new List<string>() { "Awards", "Award Letter", "PROFILE", "FAFSA" };
                ChecklistItemControlStatuses = new List<string>() { "Q", "F", "G" };
                ChecklistItemDefaultFlags = new List<string>() { "y", "n", "Y", "J" };
                IgnoreAwardStatusesFromEval = new List<string>() { "Z", "M", "U", "O"};
                IgnoreAwardCategoriesFromEval = new List<string>() { "GSL" };
                IgnoreAwardsFromEval = new List<string>() { "ACG1" };

                IgnoreAwardStatusesOnChecklist = new List<string>() { "Z", "M", "U", "O" };
                IgnoreAwardCategoriesOnChecklist = new List<string>() { "GSL" };
                IgnoreAwardsOnChecklist = new List<string>() { "ACG1" };

                ExcludeActionCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>() { AwardStatusCategory.Rejected, AwardStatusCategory.Denied };
                ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = new List<string>() { "PELL"};
                ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = new List<string>() { "14/FA", "13/FA"};
                ExcludeAwardsFromAwardLetterAndShoppingSheet = new List<string>() { "SUB", "ACG1" };
                ShoppingSheetConfiguration = new ShoppingSheetConfiguration() {EfcOption = ShoppingSheetEfcOption.IsirEfc};

                AcceptedAwardStatusCode = "A";
                AcceptedAwardCommunicationCode = "WVU";
                AcceptedAwardCommunicationStatus = "W";
                RejectedAwardStatusCode = "R";
                RejectedAwardCommunicationCode = "BYU";
                RejectedAwardCommunicationStatus = "R";

                AllowUnmetNeedBorrowing = true;
                AllowLoanChanges = true;
                AllowLoanChangeIfAccepted = true;
                SuppressInstanceData = true;

                NewLoanCommunicationCode = "WVU";
                NewLoanCommunicationStatus = "W";
                LoanChangeCommunicationCode = "BYU";
                LoanChangeCommunicationsStatus = "W";
                PaperCopyOptionText = "By selecting this option, I am explicitly choosing to receive paper...";
                
                IsLoanAmountChangeRequestRequired = true;
                IsDeclinedStatusChangeRequestRequired = true;
                IsAwardLetterHistoryActive = true;
                IsSatisfactoryAcademicProgressActive = true;
                SuppressMaximumLoanLimits = true;
                UseDocumentStatusDescription = true;
                SuppressAccountSummaryDisplay = true;
                SuppressAverageAwardPackageDisplay = true;
                SuppressAwardLetterAcceptance = false;
                SuppressDisbursementInfoDisplay = true;

                CounselorPhoneType = "BU";

                configuration = new FinancialAidConfiguration("office", awardYear);
            }

            [TestMethod]
            public void AwardYearTest()
            {
                Assert.AreEqual(awardYear, configuration.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new FinancialAidConfiguration("office", "");
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public void OfficeIdRequiredTest()
            {
                new FinancialAidConfiguration(null, configuration.AwardYear);
            }

            [TestMethod]
            public void AwardYearDescriptionGetSetTest()
            {
                configuration.AwardYearDescription = awardYearDescription;
                Assert.AreEqual(awardYearDescription, configuration.AwardYearDescription);
            }

            [TestMethod]
            public void CounselorPhoneTypeSetTest()
            {
                configuration.CounselorPhoneType = CounselorPhoneType;                
                Assert.AreEqual(CounselorPhoneType, configuration.CounselorPhoneType);
            }

            [TestMethod]
            public void CounselorPhoneTypeInitializedToNullTest()
            {
                Assert.AreEqual(null, configuration.CounselorPhoneType);
            }


            [TestMethod]
            public void IsSelfServiceActiveInitFalseTest()
            {
                Assert.AreEqual(false, configuration.IsSelfServiceActive);
            }

            [TestMethod]
            public void IsSelfServiceActiveGetSetTest()
            {
                configuration.IsSelfServiceActive = IsSelfServiceActive;
                Assert.AreEqual(IsSelfServiceActive, configuration.IsSelfServiceActive);
            }

            [TestMethod]
            public void IsAwardingActiveInitFalseTest()
            {
                Assert.AreEqual(false, configuration.IsAwardingActive);
            }

            [TestMethod]
            public void IsAwardingActiveGetSetTest()
            {
                configuration.IsAwardingActive = IsAwardingActive;
                Assert.AreEqual(IsAwardingActive, configuration.IsAwardingActive);
            }

            [TestMethod]
            public void AreAwardChangesAllowedInitFalseTest()
            {
                Assert.AreEqual(false, configuration.AreAwardChangesAllowed);
            }

            [TestMethod]
            public void AreAwardChangesAllowedGetSetTest()
            {
                configuration.AreAwardChangesAllowed = AreAwardChangesAllowed;
                Assert.AreEqual(AreAwardChangesAllowed, configuration.AreAwardChangesAllowed);
            }

            [TestMethod]
            public void AllowAnnualAwardUpdatesOnlyInitFalseTest()
            {
                Assert.AreEqual(false, configuration.AllowAnnualAwardUpdatesOnly);
            }

            [TestMethod]
            public void AllowAnnualAwardUpdatesInlyGetSetTest()
            {
                configuration.AllowAnnualAwardUpdatesOnly = AllowAnnualAwardChangesOnly;
                Assert.AreEqual(AllowAnnualAwardChangesOnly, configuration.AllowAnnualAwardUpdatesOnly);
            }

            [TestMethod]
            public void IsProfileActiveInitFalseTest()
            {
                Assert.AreEqual(false, configuration.IsProfileActive);
            }

            [TestMethod]
            public void IsProfileActiveGetSetTest()
            {
                configuration.IsProfileActive = IsProfileActive;
                Assert.AreEqual(IsProfileActive, configuration.IsProfileActive);
            }

            [TestMethod]
            public void IsShoppingSheetActiveInitFalseTest()
            {
                Assert.AreEqual(false, configuration.IsShoppingSheetActive);
            }

            [TestMethod]
            public void IsShoppingSheetActiveGetSetTest()
            {
                configuration.IsShoppingSheetActive = IsShoppingSheetActive;
                Assert.AreEqual(IsShoppingSheetActive, configuration.IsShoppingSheetActive);
            }

            [TestMethod]
            public void ExcludeAwardPeriodsInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardPeriods);
                Assert.AreEqual(0, configuration.ExcludeAwardPeriods.Count());
            }

            [TestMethod]
            public void ExcludeAwardPeriodsGetSetTest()
            {
                configuration.ExcludeAwardPeriods = ExcludeAwardPeriods;
                Assert.AreEqual(ExcludeAwardPeriods, configuration.ExcludeAwardPeriods);
            }

            [TestMethod]
            public void ExcludeAwardStatusCategoriesViewInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardStatusCategoriesView);
                Assert.AreEqual(0, configuration.ExcludeAwardStatusCategoriesView.Count());
            }

            [TestMethod]
            public void ExcludeAwardStatusCategoriesViewGetSetTest()
            {
                configuration.ExcludeAwardStatusCategoriesView = ExcludeAwardStatusCategoriesView;
                Assert.AreEqual(ExcludeAwardStatusCategoriesView, configuration.ExcludeAwardStatusCategoriesView);
            }

            [TestMethod]
            public void ExcludeAwardCategoriesViewInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardCategoriesView);
                Assert.AreEqual(0, configuration.ExcludeAwardCategoriesView.Count());
            }

            [TestMethod]
            public void ExcludeAwardCategoriesViewGetSetTest()
            {
                configuration.ExcludeAwardCategoriesView = ExcludeAwardCategoriesView;
                Assert.AreEqual(ExcludeAwardCategoriesView, configuration.ExcludeAwardCategoriesView);
            }

            [TestMethod]
            public void ExcludeAwardsViewInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardsView);
                Assert.AreEqual(0, configuration.ExcludeAwardsView.Count());
            }

            [TestMethod]
            public void ExcludeAwardsViewGetSetTest()
            {
                configuration.ExcludeAwardsView = ExcludeAwardsView;
                Assert.AreEqual(ExcludeAwardsView, configuration.ExcludeAwardsView);
            }

            [TestMethod]
            public void ExcludeAwardsFromChangeInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardsFromChange);
                Assert.AreEqual(0, configuration.ExcludeAwardsFromChange.Count());
            }

            [TestMethod]
            public void ExcludeAwardsFromChangeGetSetTest()
            {
                configuration.ExcludeAwardsFromChange = ExcludeAwardsFromChange;
                Assert.AreEqual(ExcludeAwardsFromChange, configuration.ExcludeAwardsFromChange);
            }

            [TestMethod]
            public void ExcludeAwardCategoriesFromChangeInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardCategoriesFromChange);
                Assert.AreEqual(0, configuration.ExcludeAwardCategoriesFromChange.Count());
            }

            [TestMethod]
            public void ExcludeAwardCategoriesFromChangeGetSetTest()
            {
                configuration.ExcludeAwardCategoriesFromChange = ExcludeAwardCategoriesFromChange;
                Assert.AreEqual(ExcludeAwardCategoriesFromChange, configuration.ExcludeAwardCategoriesFromChange);
            }

            [TestMethod]
            public void ExcludeAwardStatusCategoriesFromChangeInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardStatusCategoriesFromChange);
                Assert.AreEqual(0, configuration.ExcludeAwardStatusCategoriesFromChange.Count());
            }

            [TestMethod]
            public void ExcludeAwardStatusCategoriesFromChangeGetSetTest()
            {
                configuration.ExcludeAwardStatusCategoriesFromChange = ExcludeAwardStatusCategoriesFromChange;
                Assert.AreEqual(ExcludeAwardStatusCategoriesFromChange, configuration.ExcludeAwardStatusCategoriesFromChange);
            }

            [TestMethod]
            public void ExcludeAwardStatusesFromChangeInitTest()
            {
                Assert.IsNotNull(configuration.ExcludeAwardStatusesFromChange);
                Assert.AreEqual(0, configuration.ExcludeAwardStatusesFromChange.Count());
            }

            [TestMethod]
            public void ExcludeAwardStatusesFromChangeGetSetTest()
            {
                configuration.ExcludeAwardStatusesFromChange = ExcludeAwardStatusesFromChange;
                Assert.AreEqual(ExcludeAwardStatusesFromChange, configuration.ExcludeAwardStatusesFromChange);
            }

            [TestMethod]
            public void AcceptedAwardActionGetSetTest()
            {
                configuration.AcceptedAwardStatusCode = AcceptedAwardStatusCode;
                Assert.AreEqual(AcceptedAwardStatusCode, configuration.AcceptedAwardStatusCode);
            }

            [TestMethod]
            public void AcceptedAwardCommunicationCodeGetSetTest()
            {
                configuration.AcceptedAwardCommunicationCode = AcceptedAwardCommunicationCode;
                Assert.AreEqual(AcceptedAwardCommunicationCode, configuration.AcceptedAwardCommunicationCode);
            }

            [TestMethod]
            public void AcceptedAwardCommunicationStatusGetSetTest()
            {
                configuration.AcceptedAwardCommunicationStatus = AcceptedAwardCommunicationStatus;
                Assert.AreEqual(AcceptedAwardCommunicationStatus, configuration.AcceptedAwardCommunicationStatus);
            }

            [TestMethod]
            public void RejectedAwardActionGetSetTest()
            {
                configuration.RejectedAwardStatusCode = RejectedAwardStatusCode;
                Assert.AreEqual(RejectedAwardStatusCode, configuration.RejectedAwardStatusCode);
            }

            [TestMethod]
            public void RejectedAwardCommunicationCodeGetSetTest()
            {
                configuration.RejectedAwardCommunicationCode = RejectedAwardCommunicationCode;
                Assert.AreEqual(RejectedAwardCommunicationCode, configuration.RejectedAwardCommunicationCode);
            }

            [TestMethod]
            public void RejectedAwardCommunicationStatusGetSetTest()
            {
                configuration.RejectedAwardCommunicationStatus = RejectedAwardCommunicationStatus;
                Assert.AreEqual(RejectedAwardCommunicationStatus, configuration.RejectedAwardCommunicationStatus);
            }

            [TestMethod]
            public void AllowUnmetNeedBorrowingInitFalseTest()
            {
                Assert.AreEqual(false, configuration.AllowNegativeUnmetNeedBorrowing);
            }

            [TestMethod]
            public void AllowUnmetNeedBorrowingGetSetTest()
            {
                configuration.AllowNegativeUnmetNeedBorrowing = AllowUnmetNeedBorrowing;
                Assert.AreEqual(AllowUnmetNeedBorrowing, configuration.AllowNegativeUnmetNeedBorrowing);
            }

            [TestMethod]
            public void AllowLoanChangesInitFalseTest()
            {
                Assert.AreEqual(false, configuration.AllowLoanChanges);
            }

            [TestMethod]
            public void AllowLoanChangesGetSetTest()
            {
                configuration.AllowLoanChanges = AllowLoanChanges;
                Assert.AreEqual(AllowLoanChanges, configuration.AllowLoanChanges);
            }

            [TestMethod]
            public void AllowLoanChangeIfAcceptedInitFalseTest()
            {
                Assert.AreEqual(false, configuration.AllowLoanChangeIfAccepted);
            }

            [TestMethod]
            public void AllowLoanChangeIfAcceptedGetSetTest()
            {
                configuration.AllowLoanChangeIfAccepted = AllowLoanChangeIfAccepted;
                Assert.AreEqual(AllowLoanChangeIfAccepted, configuration.AllowLoanChangeIfAccepted);
            }

            [TestMethod]
            public void NewLoanCommunicationCodeGetSetTest()
            {
                configuration.NewLoanCommunicationCode = NewLoanCommunicationCode;
                Assert.AreEqual(NewLoanCommunicationCode, configuration.NewLoanCommunicationCode);
            }

            [TestMethod]
            public void NewLoanCommunicationStatusGetSetTest()
            {
                configuration.NewLoanCommunicationStatus = NewLoanCommunicationStatus;
                Assert.AreEqual(NewLoanCommunicationStatus, configuration.NewLoanCommunicationStatus);
            }

            [TestMethod]
            public void LoanChangeCommunicationCodeGetSetTest()
            {
                configuration.LoanChangeCommunicationCode = LoanChangeCommunicationCode;
                Assert.AreEqual(LoanChangeCommunicationCode, configuration.LoanChangeCommunicationCode);
            }

            [TestMethod]
            public void LoanChangeCommunicationStatusGetSetTest()
            {
                configuration.LoanChangeCommunicationStatus = LoanChangeCommunicationsStatus;
                Assert.AreEqual(LoanChangeCommunicationsStatus, configuration.LoanChangeCommunicationStatus);
            }

            /// <summary>
            /// Test if SuppressInstanceData boolean is intialized with a false value
            /// </summary>
            [TestMethod]
            public void SuppressInstanceDataInitFalseTest()
            {
                Assert.AreEqual(false, configuration.SuppressInstanceData);
            }

            /// <summary>
            /// Test if SuppressInstanceData gets set correctly
            /// </summary>
            [TestMethod]
            public void SuppressInstanceDataGetSetTest()
            {
                configuration.SuppressInstanceData = SuppressInstanceData;
                Assert.AreEqual(SuppressInstanceData, configuration.SuppressInstanceData);
            }

            /// <summary>
            /// Test if paperCopyText is initualized as null
            /// </summary>
            [TestMethod]
            public void PaperCopyOptionTextInitNullTest()
            {
                Assert.AreEqual(null, configuration.PaperCopyOptionText);
            }

            /// <summary>
            /// Test if PaperCopyOptionText can be set and retrieved
            /// </summary>
            [TestMethod]
            public void PaperCopyOptionTextGetSetTest()
            {
                configuration.PaperCopyOptionText = PaperCopyOptionText;
                Assert.AreEqual(PaperCopyOptionText, configuration.PaperCopyOptionText);
            }

            [TestMethod]
            public void IsLoanAmountChangeRequestRequiredTest()
            {
                configuration.IsLoanAmountChangeRequestRequired = IsLoanAmountChangeRequestRequired;
                Assert.AreEqual(IsLoanAmountChangeRequestRequired, configuration.IsLoanAmountChangeRequestRequired);
            }

            [TestMethod]
            public void IsDeclinedStatusChangeRequestRequiredTest()
            {
                configuration.IsDeclinedStatusChangeRequestRequired = IsDeclinedStatusChangeRequestRequired;
                Assert.AreEqual(IsDeclinedStatusChangeRequestRequired, configuration.IsDeclinedStatusChangeRequestRequired);
            }

            [TestMethod]
            public void ExcludeActionCategoriesFromAwardLetterAndShoppingSheetTest()
            {
                configuration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = ExcludeActionCategoriesFromAwardLetterAndShoppingSheet;
                CollectionAssert.AreEqual(ExcludeActionCategoriesFromAwardLetterAndShoppingSheet, configuration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void ExcludeAwardsFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                Assert.IsFalse(configuration.ExcludeAwardsFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public void ExcludeAwardsFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                configuration.ExcludeAwardsFromAwardLetterAndShoppingSheet = ExcludeAwardsFromAwardLetterAndShoppingSheet;
                CollectionAssert.AreEqual(ExcludeAwardsFromAwardLetterAndShoppingSheet, configuration.ExcludeAwardsFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                Assert.IsFalse(configuration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public void ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                configuration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet;
                CollectionAssert.AreEqual(ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet, configuration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                Assert.IsFalse(configuration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public void ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                configuration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet;
                CollectionAssert.AreEqual(ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet, configuration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public void IsAwardLetterHistoryActiveInitFalseTest()
            {
                Assert.AreEqual(false, configuration.IsAwardLetterHistoryActive);
            }

            [TestMethod]
            public void IsAwardLetterHistoryActiveGetSetTest()
            {
                configuration.IsAwardLetterHistoryActive = IsAwardLetterHistoryActive;
                Assert.AreEqual(IsAwardLetterHistoryActive, configuration.IsAwardLetterHistoryActive);
            }

            [TestMethod]
            public void ShoppingSheetConfigurationTest()
            {
                configuration.ShoppingSheetConfiguration = ShoppingSheetConfiguration;
                Assert.AreEqual(ShoppingSheetConfiguration, configuration.ShoppingSheetConfiguration);
            }

            [TestMethod]
            public void FAConfiguration_EqualsTest()
            {
                Assert.IsTrue(configuration.Equals(new FinancialAidConfiguration(configuration.OfficeId, configuration.AwardYear)));
            }

            [TestMethod]
            public void FAConfigurationDiffOfficeId_NotEqualsTest()
            {
                Assert.IsFalse(configuration.Equals(new FinancialAidConfiguration("office1", configuration.AwardYear)));
            }

            [TestMethod]
            public void FAConfigurationDiffAwardYear_NotEqualsTest()
            {
                Assert.IsFalse(configuration.Equals(new FinancialAidConfiguration("office", "diffYear")));
            }

            [TestMethod]
            public void FAConfiguration_NotEqualsNullObjectTest()
            {
                Assert.IsFalse(configuration.Equals(null));
            }

            [TestMethod]
            public void HashCode_EqualsTest()
            {
                Assert.AreEqual(configuration.GetHashCode(), (new FinancialAidConfiguration(configuration.OfficeId, configuration.AwardYear)).GetHashCode());
            }

            [TestMethod]
            public void HashCodeDiffOfficeId_NotEqualsTest()
            {
                Assert.AreNotEqual(configuration.GetHashCode(), (new FinancialAidConfiguration("diffOfficeId", configuration.AwardYear)).GetHashCode());
            }

            [TestMethod]
            public void HashCodeDiffAwardYear_NotEqualsTest()
            {
                Assert.AreNotEqual(configuration.GetHashCode(), (new FinancialAidConfiguration(configuration.OfficeId, "diffYear")).GetHashCode());
            }

            [TestMethod]
            public void FAConfiguration_ToStringTest()
            {
                var configString = "office*2015";
                Assert.AreEqual(configString, (new FinancialAidConfiguration("office", "2015")).ToString());
            }   
         
            [TestMethod]
            public void SuppressMaximumLoanLimitsGetSetTest()
            {
                configuration.SuppressMaximumLoanLimits = SuppressMaximumLoanLimits;
                Assert.AreEqual(SuppressMaximumLoanLimits, configuration.SuppressMaximumLoanLimits);
            }

            [TestMethod]
            public void UseDocumentStatusDescription_GetSetTest()
            {
                configuration.UseDocumentStatusDescription = UseDocumentStatusDescription;
                Assert.AreEqual(UseDocumentStatusDescription, configuration.UseDocumentStatusDescription);
            }

            [TestMethod]
            public void GetUpdatedStatusCode_ReturnsAcceptedAwardStatusCodeTest()
            {
                configuration.AcceptedAwardStatusCode = "A";
                string actualCode = configuration.GetUpdatedAwardStatusCode(new AwardStatus("Acc", "Desc", AwardStatusCategory.Accepted));
                Assert.AreEqual(configuration.AcceptedAwardStatusCode, actualCode);
            }

            [TestMethod]
            public void GetUpdatedStatusCode_ReturnsRejectedAwardStatusCodeTest1()
            {
                configuration.RejectedAwardStatusCode = "R";
                string actualCode = configuration.GetUpdatedAwardStatusCode(new AwardStatus("D", "Desc", AwardStatusCategory.Denied));
                Assert.AreEqual(configuration.RejectedAwardStatusCode, actualCode);
            }

            [TestMethod]
            public void GetUpdatedStatusCode_ReturnsRejectedAwardStatusCodeTest2()
            {
                configuration.RejectedAwardStatusCode = "R";
                string actualCode = configuration.GetUpdatedAwardStatusCode(new AwardStatus("F", "Desc", AwardStatusCategory.Rejected));
                Assert.AreEqual(configuration.RejectedAwardStatusCode, actualCode);
            }

            [TestMethod]
            public void GetUpdatedStatusCode_ReturnsAnEmptyTest()
            {
                configuration.RejectedAwardStatusCode = "R";
                string actualCode = configuration.GetUpdatedAwardStatusCode(new AwardStatus("D", "Desc", AwardStatusCategory.Estimated));
                Assert.AreEqual(string.Empty, actualCode);
            }

            [TestMethod]
            public void ChecklistItemCodes_InitializedToAnEmptyListTest()
            {
                Assert.IsFalse(configuration.ChecklistItemCodes.Any());
            }

            [TestMethod]
            public void ChecklistItemCodes_GetSetTest()
            {
                configuration.ChecklistItemCodes = ChecklistItemCodes;
                CollectionAssert.AreEqual(ChecklistItemCodes, configuration.ChecklistItemCodes);
            }

            [TestMethod]
            public void ChecklistItemControlStatuses_InitializedToAnEmptyListTest()
            {
                Assert.IsFalse(configuration.ChecklistItemControlStatuses.Any());
            }

            [TestMethod]
            public void ChecklistItemControlStatuses_GetSetTest()
            {
                configuration.ChecklistItemControlStatuses = ChecklistItemControlStatuses;
                CollectionAssert.AreEqual(ChecklistItemControlStatuses, configuration.ChecklistItemControlStatuses);
            }

            [TestMethod]
            public void ChecklistItemDefaultFlags_InitializedToAnEmptyListTest()
            {
                Assert.IsFalse(configuration.ChecklistItemDefaultFlags.Any());
            }

            [TestMethod]
            public void ChecklistItemDefaultFlags_GetSetTest()
            {
                configuration.ChecklistItemDefaultFlags = ChecklistItemDefaultFlags;
                CollectionAssert.AreEqual(ChecklistItemDefaultFlags, configuration.ChecklistItemDefaultFlags);
            }

            [TestMethod]
            public void IgnoreAwardStatusesFromEval_InitializedToAnEmptyListTest()
            {
                Assert.IsFalse(configuration.IgnoreAwardStatusesFromEval.Any());
            }

            [TestMethod]
            public void IgnoreAwardStatusesFromEval_GetSetTest()
            {
                configuration.IgnoreAwardStatusesFromEval = IgnoreAwardStatusesFromEval;
                CollectionAssert.AreEqual(IgnoreAwardStatusesFromEval, configuration.IgnoreAwardStatusesFromEval);
            }
            
            [TestMethod]
            public void IgnoreAwardsFromEval_InitializedToAnEmptyListTest()
            {
                Assert.IsFalse(configuration.IgnoreAwardsFromEval.Any());
            }

            [TestMethod]
            public void IgnoreAwardsFromEval_GetSetTest()
            {
                configuration.IgnoreAwardsFromEval = IgnoreAwardsFromEval;
                CollectionAssert.AreEqual(IgnoreAwardsFromEval, configuration.IgnoreAwardsFromEval);
            }

            [TestMethod]
            public void IgnoreAwardCategoriesFromEvalToAnEmptyList()
            {
                Assert.IsFalse(configuration.IgnoreAwardCategoriesFromEval.Any());
            }

            [TestMethod]
            public void IgnoreAwardCategoriesFromEval_GetSetTest()
            {
                configuration.IgnoreAwardCategoriesFromEval = IgnoreAwardCategoriesFromEval;
                CollectionAssert.AreEqual(IgnoreAwardCategoriesFromEval, configuration.IgnoreAwardCategoriesFromEval);
            }

            [TestMethod]
            public void SuppressAverageAwardPackageDisplayGetSetTest()
            {
                configuration.SuppressAverageAwardPackageDisplay = SuppressAverageAwardPackageDisplay;
                Assert.AreEqual(SuppressAverageAwardPackageDisplay, configuration.SuppressAverageAwardPackageDisplay);

            }

            [TestMethod]
            public void SuppressAccountSummaryDisplayGetSetTest()
            {
                configuration.SuppressAccountSummaryDisplay = SuppressAccountSummaryDisplay;
                Assert.AreEqual(SuppressAccountSummaryDisplay, configuration.SuppressAccountSummaryDisplay);
            }

            [TestMethod]
            public void SuppressAwardLetterAcceptanceGetSetTest()
            {
                configuration.SuppressAwardLetterAcceptance = SuppressAwardLetterAcceptance;
                Assert.AreEqual(SuppressAwardLetterAcceptance, configuration.SuppressAwardLetterAcceptance);
            }

            [TestMethod]
            public void SuppressDisbursementInfoDisplayInitTest()
            {
                Assert.IsFalse(configuration.SuppressDisbursementInfoDisplay);
            }

            [TestMethod]
            public void SuppressDisbursementInfoDisplayGetSetTest()
            {
                configuration.SuppressDisbursementInfoDisplay = SuppressDisbursementInfoDisplay;
                Assert.AreEqual(SuppressDisbursementInfoDisplay, configuration.SuppressDisbursementInfoDisplay);
            }

            [TestMethod]
            public void IgnoreAwardStatusesOnChecklist_InitializedToEmptyListTest()
            {
                Assert.IsFalse(configuration.IgnoreAwardStatusesOnChecklist.Any());
            }

            [TestMethod]
            public void IgnoreAwardStatusesOnChecklist_GetSetTest()
            {
                configuration.IgnoreAwardStatusesOnChecklist = IgnoreAwardStatusesOnChecklist;
                CollectionAssert.AreEqual(IgnoreAwardStatusesOnChecklist, configuration.IgnoreAwardStatusesOnChecklist);
            }

            [TestMethod]
            public void IgnoreAwardsOnChecklist_InitializedToEmptyListTest()
            {
                Assert.IsFalse(configuration.IgnoreAwardsOnChecklist.Any());
            }

            [TestMethod]
            public void IgnoreAwardsOnChecklist_GetSetTest()
            {
                configuration.IgnoreAwardsOnChecklist = IgnoreAwardsOnChecklist;
                CollectionAssert.AreEqual(IgnoreAwardsOnChecklist, configuration.IgnoreAwardsOnChecklist);
            }

            [TestMethod]
            public void IgnoreAwardCategoriesOnChecklist_InitializedToEmptyListTest()
            {
                Assert.IsFalse(configuration.IgnoreAwardCategoriesOnChecklist.Any());
            }

            [TestMethod]
            public void IgnoreAwardCategoriesOnChecklist_GetSetTest()
            {
                configuration.IgnoreAwardCategoriesOnChecklist = IgnoreAwardCategoriesOnChecklist;
                CollectionAssert.AreEqual(IgnoreAwardCategoriesOnChecklist, configuration.IgnoreAwardCategoriesOnChecklist);
            }

            [TestMethod]
            public void ShowBudgetDetailsOnAwardLetter_InitializedFalseTest()
            {
                Assert.IsFalse(configuration.ShowBudgetDetailsOnAwardLetter);
            }

            [TestMethod]
            public void ShowBudgetDetailsOnAwardLetterGetSetTest()
            {
                configuration.ShowBudgetDetailsOnAwardLetter = true;
                Assert.IsTrue(configuration.ShowBudgetDetailsOnAwardLetter);
            }

            [TestMethod]
            public void StudentAwardLetterBudgetDetailsDescription_InitializedNullTest()
            {
                Assert.IsNull(configuration.StudentAwardLetterBudgetDetailsDescription);
            }

            [TestMethod]
            public void StudentAwardLetterBudgetDetailsDescription_GetSetTest()
            {
                configuration.StudentAwardLetterBudgetDetailsDescription = "Description";
                Assert.AreEqual("Description", configuration.StudentAwardLetterBudgetDetailsDescription);
            }
        }
    }
}
