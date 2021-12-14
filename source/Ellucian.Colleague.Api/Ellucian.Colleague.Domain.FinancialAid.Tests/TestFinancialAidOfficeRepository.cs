/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestFinancialAidOfficeRepository : IFinancialAidOfficeRepository
    {
        #region ShoppingSheet Parameters
        public class ShoppingSheetParameterRecord
        {
            public string AwardYear;
            public string OpeId;
            public string CustomMessageRuleTableId;
            public string OfficeType;
            public decimal? GraduationRate;
            public decimal? LoanDefaultRate;
            public decimal? NationalLoanDefaultRate;
            public int? MedianBorrowingAmount;
            public decimal? MedianMonthlyPaymentAmount;
            public string UseProfileImEfc;
            public string UseProfileImUntilIsirIsFederal;
            public decimal? LowToMediumBoundary;
            public decimal? MediumToHighBoundary;
            public decimal? InstitutionRepaymentRate;
            public decimal? NationalRepaymentRateAverage;
        }

        public List<ShoppingSheetParameterRecord> shoppingSheetParameterData = new List<ShoppingSheetParameterRecord>()
        {
            new ShoppingSheetParameterRecord()
            {
                AwardYear = "2015",
                OpeId = "00999888",
                OfficeType = "1",
                GraduationRate = (decimal)88.7,
                LoanDefaultRate = (decimal)4.2,
                NationalLoanDefaultRate = (decimal) 12.5,
                MedianBorrowingAmount = 50000,
                MedianMonthlyPaymentAmount = (decimal)500.55,
                UseProfileImEfc = "Y",
                UseProfileImUntilIsirIsFederal = "y",
                LowToMediumBoundary = (decimal)42.1,
                MediumToHighBoundary = (decimal)72.3,
                InstitutionRepaymentRate = 24.6m,
                NationalRepaymentRateAverage=34.2m
            },
            new ShoppingSheetParameterRecord()
            {
                AwardYear = "2014",
                OpeId = "00999888",
                OfficeType = "1",
                GraduationRate = (decimal)88.7,
                LoanDefaultRate = (decimal)4.2,
                NationalLoanDefaultRate = (decimal) 12.5,
                MedianBorrowingAmount = 50000,
                MedianMonthlyPaymentAmount = (decimal)500.55,
                UseProfileImEfc = "y",
                UseProfileImUntilIsirIsFederal = "y",
                LowToMediumBoundary = (decimal)42.1,
                MediumToHighBoundary = (decimal)72.3,
                InstitutionRepaymentRate = 75.0m,
                NationalRepaymentRateAverage=13.8m
            },
            new ShoppingSheetParameterRecord()
            {
                AwardYear = "2014",
                OpeId = "00111222",
                OfficeType = "2",
                GraduationRate = (decimal)90,
                LoanDefaultRate = (decimal)5,
                NationalLoanDefaultRate = (decimal) 10,
                MedianBorrowingAmount = 45000,
                MedianMonthlyPaymentAmount = 1000,
                UseProfileImEfc = "y",
                UseProfileImUntilIsirIsFederal = "",
                LowToMediumBoundary = (decimal)42.1,
                MediumToHighBoundary = (decimal)72.3,
                InstitutionRepaymentRate = 57.6m,
                NationalRepaymentRateAverage=34.2m
            },
            new ShoppingSheetParameterRecord()
            {
                AwardYear = "2013",
                OpeId = "00999888",
                OfficeType = "1",
                GraduationRate = (decimal)88.7,
                LoanDefaultRate = (decimal)4.2,
                NationalLoanDefaultRate = (decimal) 12.5,
                MedianBorrowingAmount = 50000,
                MedianMonthlyPaymentAmount = 500,
                UseProfileImEfc = null,
                UseProfileImUntilIsirIsFederal = "n",
                LowToMediumBoundary = (decimal)42.1,
                MediumToHighBoundary = (decimal)72.3,
                InstitutionRepaymentRate = 10.9m,
                NationalRepaymentRateAverage=77.8m
            }
        };
        #endregion

        #region Office Year-Based Parameters
        public class OfficeParameterRecord
        {
            public string OfficeCode;
            public string AwardYear;
            public string AwardYearDescription;
            public string IsSelfServiceActive;
            public string IsAwardingActive;
            public string IsProfileActive;
            public string IsAwardLetterActive;
            public string IsShoppingSheetActive;
            public string CanMakeAwardChanges;
            public string AnnualAccRejOnly;
            public string AreLoanRequestsAllowed;
            public List<string> AwardPeriodsToExcludeFromView;
            public List<string> AwardStatusCategoriesToExcludeFromView;
            public List<string> AwardCategoriesToExcludeFromView;
            public List<string> AwardsToExcludeFromView;
            public List<string> AwardStatusCategoriesToPreventChanges;
            public List<string> AwardCategoriesToPreventChanges;
            public List<string> AwardsToPreventChanges;
            public List<string> AwardStatusCategoriesToExcludeFromAwardLetter;
            public List<string> AwardsToExcludeFromAwardLetter;
            public List<string> AwardCategoriesToExcludeFromAwardLetter;
            public List<string> AwardPeriodsToExcludeFromAwardLetter;
            public List<string> ActionStatusesToPreventChanges;
            public List<string> ChecklistItemCodes;
            public List<string> ChecklistItemControlStatuses;
            public List<string> ChecklistItemDefaultFlags;
            public List<string> IgnoreAwardStatusesFromEval;
            public int? AverageGradGrantAmount;
            public int? AverageGradLoanAmount;
            public int? AverageGradScholarshipAmount;
            public int? AverageUndergradGrantAmount;
            public int? AverageUndergradLoanAmount;
            public int? AverageUndergradScholarshipAmount;
            public string AcceptedAwardAction;
            public string AcceptedAwardCommunicationCode;
            public string AcceptedAwardCommunicationStatus;
            public string RejectedAwardAction;
            public string RejectedAwardCommunicationCode;
            public string RejectedAwardCommunicationStatus;
            public string AllowLoanChanges;
            public string AllowLoanChangeIfAccepted;
            public string AllowDeclineZeroOutIfAccepted;
            public string AllowNegativeUnmetNeedBorrowing;
            public string SuppressInstanceData;
            public string IsAwardLetterHistoryActive;            
            public string NewLoanCommunicationCode;
            public string NewLoanCommunicationStatus;
            public string LoanChangeCommunicationCode;
            public string LoanChangeCommunicationStatus;
            public string PaperCopyOptionText;
            public string ReviewLoanChanges;
            public string ReviewDeclinedAwards;
            public string CounselorPhoneType;
            public string CreateChecklistItemsForNewStudent;
            public string UseDefaultContact;
            public string SuppressMaximumLoanLimits;
            public string UseDocumentStatusDescription;
            public string DisplayPellLifetimeEarningsUsed;
            public string SuppressAccountSummaryDisplay;
            public string SuppressAverageAwardPackageDisplay;
            public string SuppressAwardLetterAcceptance;
            public string SuppressDisbursementInfoDisplay;
            public List<string> IgnoreAwardsOnChecklist;
            public List<string> IgnoreActionStatusesOnChecklist;
            public List<string> IgnoreAwardCategoriesOnChecklist;
            public string ShowBudgetDetailsOnAwardLetter;
            public string StudentAwardLetterBudgetDetailsDescription;
            public string BlankStatusText;
            public string BlankDueDateText;
            public string ShowAslaInfo;
        }

        public List<OfficeParameterRecord> officeParameterRecordData = new List<OfficeParameterRecord>()
        {
            new OfficeParameterRecord() 
            { 
                OfficeCode = "MAIN",
                AwardYear = "2012", 
                AwardYearDescription = "Academic Year 2014-15 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsProfileActive = "Y",
                IsAwardLetterActive = "y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "y",
                AnnualAccRejOnly = "y",
                AreLoanRequestsAllowed = "y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(),
                ChecklistItemControlStatuses = new List<string>(),
                ChecklistItemDefaultFlags = new List<string>(),
                IgnoreAwardStatusesFromEval = new List<string>(){"H", "j"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "R",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "R",
                AllowNegativeUnmetNeedBorrowing = "y",
                SuppressInstanceData = "y",
                IsAwardLetterHistoryActive = "y",               
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "y",
                AllowDeclineZeroOutIfAccepted = "y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "R",
                PaperCopyOptionText = "Paper copy option text",
                ReviewDeclinedAwards = "N",
                ReviewLoanChanges = "N",
                CounselorPhoneType = "BU",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "Y",
                SuppressAverageAwardPackageDisplay = "N",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>() {"SUB" },
                IgnoreActionStatusesOnChecklist = new List<string>() {"E" },
                IgnoreAwardCategoriesOnChecklist = new List<string>() {"LOAN" },
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2014/2015 Budget breakdown",
                BlankStatusText = "For when the status is blank 1",
                BlankDueDateText = "For when the due date is blank 1",
                ShowAslaInfo = "Y"
            },
            new OfficeParameterRecord() 
            { 
                OfficeCode = "MAIN",
                AwardYear = "2013", 
                AwardYearDescription = "Academic Year 2014-15 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "Y", 
                IsProfileActive = "y",
                IsAwardLetterActive = "y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "y",
                AnnualAccRejOnly = "y",
                AreLoanRequestsAllowed = "y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(),
                ChecklistItemControlStatuses = new List<string>(),
                ChecklistItemDefaultFlags = new List<string>(),
                IgnoreAwardStatusesFromEval = new List<string>(){"M", "L", "r"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "W",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "I",
                AllowNegativeUnmetNeedBorrowing = "y",
                SuppressInstanceData = string.Empty,
                IsAwardLetterHistoryActive = null,
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "y",
                AllowDeclineZeroOutIfAccepted = "y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBCHANGE",
                LoanChangeCommunicationStatus = "W",
                PaperCopyOptionText = "Paper copy option text number 1",
                ReviewDeclinedAwards = "Y",
                ReviewLoanChanges = "Y",
                CounselorPhoneType = "BU",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "N",
                SuppressAverageAwardPackageDisplay = "Y",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>() {"SUB" },
                IgnoreActionStatusesOnChecklist = new List<string>() {"E" },
                IgnoreAwardCategoriesOnChecklist = new List<string>(),
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2013/2014 Budget details",
                BlankStatusText = "For when the status is blank 2",
                BlankDueDateText = "For when the due date is blank 2",
                ShowAslaInfo = "Y"

            },
            new OfficeParameterRecord() 
            { 
                OfficeCode = "MAIN",
                AwardYear = "2014", 
                AwardYearDescription = "Academic Year 2014-15 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsAwardLetterActive = "y",
                IsProfileActive = "Y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "Y",
                AnnualAccRejOnly = "Y",
                AreLoanRequestsAllowed = "Y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(),
                ChecklistItemControlStatuses = new List<string>(),
                ChecklistItemDefaultFlags = new List<string>(),
                IgnoreAwardStatusesFromEval = new List<string>(){"U", "P", "F"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "W",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "I",
                AllowNegativeUnmetNeedBorrowing = "Y",
                SuppressInstanceData = "Y",
                IsAwardLetterHistoryActive = "y",
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "Y",
                AllowDeclineZeroOutIfAccepted = "y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "W",
                PaperCopyOptionText = "Paper copy option text number 2",
                ReviewDeclinedAwards = "",
                ReviewLoanChanges = "Y",
                CounselorPhoneType = "BU",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "N",
                SuppressAverageAwardPackageDisplay = "N",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>() {"SUB" },
                IgnoreActionStatusesOnChecklist = new List<string>() {"E", "O" },
                IgnoreAwardCategoriesOnChecklist = new List<string>() {"LOAN" },
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2014/2015 Budget breakdown",
                BlankStatusText = "For when the status is blank 3",
                BlankDueDateText = "For when the due date is blank 3",
                ShowAslaInfo = "Y"
            },
            new OfficeParameterRecord() 
            { 
                OfficeCode = "LAW",
                AwardYear = "2014", 
                AwardYearDescription = "Academic Year 2014-15 LAW Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsAwardLetterActive = "y",
                IsProfileActive = "Y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "Y",
                AnnualAccRejOnly = "Y",
                AreLoanRequestsAllowed = "Y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(),
                ChecklistItemControlStatuses = new List<string>(),
                ChecklistItemDefaultFlags = new List<string>(),
                IgnoreAwardStatusesFromEval = new List<string>(){"H", "j", "F"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "W",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "I",
                AllowNegativeUnmetNeedBorrowing = "y",
                SuppressInstanceData = null,
                IsAwardLetterHistoryActive = "Y",
                AllowLoanChanges = "Y",
                AllowLoanChangeIfAccepted = "Y",
                AllowDeclineZeroOutIfAccepted = "Y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "W",
                PaperCopyOptionText = "Paper copy option text number 3",
                ReviewDeclinedAwards = "",
                ReviewLoanChanges = "",
                CounselorPhoneType = "BU",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "Y",
                SuppressAverageAwardPackageDisplay = "Y",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>() {"UNSDL" },
                IgnoreActionStatusesOnChecklist = new List<string>(),
                IgnoreAwardCategoriesOnChecklist = new List<string>(),
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2014/2015 Budget breakdown",
                BlankStatusText = "For when the status is blank 4",
                BlankDueDateText = "For when the due date is blank 4",
                ShowAslaInfo = "Y"
                },
            new OfficeParameterRecord()
            {
                OfficeCode = "MAIN",
                AwardYear = "2015", 
                AwardYearDescription = "Academic Year 2015-16 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsAwardLetterActive = "y",
                IsProfileActive = "Y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "Y",
                AnnualAccRejOnly = "Y",
                AreLoanRequestsAllowed = "Y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(){"FAFSA", "PROFILE", "ACCAWDPKG", "APPLRVW", "CMPLREQDOC"},
                ChecklistItemControlStatuses = new List<string>(){"Q", "R", "S", "Q", "S"},
                ChecklistItemDefaultFlags = new List<string>(){"", "", "N", "n", ""},
                IgnoreAwardStatusesFromEval = new List<string>(){"A", "B", "C"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "W",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "I",
                AllowNegativeUnmetNeedBorrowing = "Y",
                SuppressInstanceData = "Y",
                IsAwardLetterHistoryActive = "y",
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "y",
                AllowDeclineZeroOutIfAccepted = "Y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "W",
                PaperCopyOptionText = "Paper copy option text number 4",
                ReviewDeclinedAwards = "Y",
                ReviewLoanChanges = "",
                CounselorPhoneType = "CP",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "Y",
                SuppressAverageAwardPackageDisplay = "N",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>(),
                IgnoreActionStatusesOnChecklist = new List<string>(),
                IgnoreAwardCategoriesOnChecklist = new List<string>(),
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2015/2016 Budget breakdown",
                BlankStatusText = "For when the status is blank 5",
                BlankDueDateText = "For when the due date is blank 5",
                ShowAslaInfo = "N"
            },            
            new OfficeParameterRecord()
            {
                OfficeCode = "MAIN",
                AwardYear = "2016", 
                AwardYearDescription = "Academic Year 2016-17 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsAwardLetterActive = "y",
                IsProfileActive = "Y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "Y",
                AnnualAccRejOnly = "Y",
                AreLoanRequestsAllowed = "Y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(){"FAFSA", "PROFILE", "ACCAWDPKG", "APPLRVW", "CMPLREQDOC"},
                ChecklistItemControlStatuses = new List<string>(){"Q", "R", "S", "Q", "S"},
                ChecklistItemDefaultFlags = new List<string>(){"Y", "Y", "Y", "Y", "Y"},
                IgnoreAwardStatusesFromEval = new List<string>(){"o", "P", "Q", "R"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "W",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "I",
                AllowNegativeUnmetNeedBorrowing = "Y",
                SuppressInstanceData = "Y",
                IsAwardLetterHistoryActive = "y",
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "y",
                AllowDeclineZeroOutIfAccepted = "Y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "W",
                PaperCopyOptionText = "Paper copy option text number 4",
                ReviewDeclinedAwards = "Y",
                ReviewLoanChanges = "",
                CounselorPhoneType = "CP",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "N",
                SuppressAverageAwardPackageDisplay = "Y",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>() {"SUB", "UNSUB" },
                IgnoreActionStatusesOnChecklist = new List<string>(),
                IgnoreAwardCategoriesOnChecklist = new List<string>(),
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2016/2017 Budget breakdown",
                BlankStatusText = "For when the status is blank 6",
                BlankDueDateText = "For when the due date is blank 6",
                ShowAslaInfo = "N"
            },
            new OfficeParameterRecord() 
            { 
                OfficeCode = "MAIN",
                AwardYear = "2017", 
                AwardYearDescription = "Academic Year 2017-18 MAIN Office",
                IsSelfServiceActive = "y", 
                IsAwardingActive = "y", 
                IsProfileActive = "Y",
                IsAwardLetterActive = "y",
                IsShoppingSheetActive = "y",
                CanMakeAwardChanges = "y",
                AnnualAccRejOnly = "y",
                AreLoanRequestsAllowed = "y",
                AwardPeriodsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToExcludeFromView = new List<string>(),
                AwardCategoriesToExcludeFromView = new List<string>(),
                AwardsToExcludeFromView = new List<string>(),
                AwardStatusCategoriesToPreventChanges = new List<string>(),
                AwardCategoriesToPreventChanges = new List<string>(),
                ActionStatusesToPreventChanges = new List<string>(),
                AwardsToPreventChanges = new List<string>(),
                AwardStatusCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardCategoriesToExcludeFromAwardLetter = new List<string>(),
                AwardPeriodsToExcludeFromAwardLetter = new List<string>(),
                AwardsToExcludeFromAwardLetter = new List<string>(),
                ChecklistItemCodes = new List<string>(),
                ChecklistItemControlStatuses = new List<string>(),
                ChecklistItemDefaultFlags = new List<string>(),
                IgnoreAwardStatusesFromEval = new List<string>(){"H", "j"},
                AverageGradGrantAmount = 1000,
                AverageGradLoanAmount = 2000,
                AverageGradScholarshipAmount = 3000,
                AverageUndergradGrantAmount = 500,
                AverageUndergradLoanAmount = 5000,
                AverageUndergradScholarshipAmount = 50000,
                AcceptedAwardAction = "A",
                AcceptedAwardCommunicationCode = "WEBACCEPT",
                AcceptedAwardCommunicationStatus = "R",
                RejectedAwardAction = "R",
                RejectedAwardCommunicationCode = "WEBREJECT",
                RejectedAwardCommunicationStatus = "R",
                AllowNegativeUnmetNeedBorrowing = "y",
                SuppressInstanceData = "y",
                IsAwardLetterHistoryActive = "y",               
                AllowLoanChanges = "y",
                AllowLoanChangeIfAccepted = "y",
                AllowDeclineZeroOutIfAccepted = "Y",
                NewLoanCommunicationCode = "WEBNEWLOAN",
                NewLoanCommunicationStatus = "W",
                LoanChangeCommunicationCode = "WEBLOANCHANGE",
                LoanChangeCommunicationStatus = "R",
                PaperCopyOptionText = "Paper copy option text",
                ReviewDeclinedAwards = "N",
                ReviewLoanChanges = "N",
                CounselorPhoneType = "BU",
                CreateChecklistItemsForNewStudent = "Y",
                UseDefaultContact = "Y",
                SuppressMaximumLoanLimits = "Y",
                UseDocumentStatusDescription = "Y",
                DisplayPellLifetimeEarningsUsed = "Y",
                SuppressAccountSummaryDisplay = "Y",
                SuppressAverageAwardPackageDisplay = "N",
                SuppressAwardLetterAcceptance = "",
                SuppressDisbursementInfoDisplay = "",
                IgnoreAwardsOnChecklist = new List<string>(),
                IgnoreActionStatusesOnChecklist = new List<string>(),
                IgnoreAwardCategoriesOnChecklist = new List<string>(),
                ShowBudgetDetailsOnAwardLetter = "N",
                StudentAwardLetterBudgetDetailsDescription = "2017/2018 Budget breakdown",
                BlankStatusText = "For when the status is blank 7",
                BlankDueDateText = "For when the due date is blank 7",
                ShowAslaInfo = "Y"
            }
        };

        #endregion

        #region SAP Parameters
        public class AcademicProgressParameters
        {
            public string officeId;
            public string isAcademicProgressActive;
            public string isAcademicProgressHistoryActive;
            public int? numberOfAcademicProgressHistoryRecordsToDisplay;
            public string maxCreditOption;
            public string maxCreditDescription;
            public string maxCreditLabel;
            public string cumulativeCompletedGpaOption;
            public string cumulativeCompletedGpaDescription;
            public string cumulativeCompletedGpaLabel;
            public string cumulativeCompletedCreditsOption;
            public string cumulativeCompletedCreditsDescription;
            public string cumulativeCompletedCreditsLabel;
            public string cumulativeAttemptedCreditsOption;
            public string cumulativeAttemptedCreditsDescription;
            public string cumulativeAttemptedCreditsLabel;
            public string cumulativePaceOption;
            public string cumulativePaceDescription;
            public string cumulativePaceLabel;
            public string cumulativeCompletedCreditsExcludingRemedialOption;
            public string cumulativeCompletedCreditsExcludingRemedialDescription;
            public string cumulativeCompletedCreditsExcludingRemedialLabel;
            public string cumulativeAttemptedCreditsExcludingRemedialOption;
            public string cumulativeAttemptedCreditsExcludingRemedialDescription;
            public string cumulativeAttemptedCreditsExcludingRemedialLabel;
            public string cumulativePaceExcludingRemedialOption;
            public string cumulativePaceExcludingRemedialDescription;
            public string cumulativePaceExcludingRemedialLabel;
            public string evaluationPeriodCompletedGpaOption;
            public string evaluationPeriodCompletedGpaDescription;
            public string evaluationPeriodCompletedGpaLabel;
            public string evaluationPeriodCompletedCreditsOption;
            public string evaluationPeriodCompletedCreditsDescription;
            public string evaluationPeriodCompletedCreditsLabel;
            public string evaluationPeriodAttemptedCreditsOption;
            public string evaluationPeriodAttemptedCreditsDescription;
            public string evaluationPeriodAttemptedCreditsLabel;
            public string evaluationPeriodPaceOption;
            public string evaluationPeriodPaceDescription;
            public string evaluationPeriodPaceLabel;
        }

        public List<AcademicProgressParameters> academicProgressParameterData = new List<AcademicProgressParameters>()
        {
            new AcademicProgressParameters()
            {
                officeId = "MAIN",
                isAcademicProgressActive = "Y",
                isAcademicProgressHistoryActive = "Y",
                numberOfAcademicProgressHistoryRecordsToDisplay = 3,
                maxCreditOption = "y",
                maxCreditDescription = "Max Credits",
                maxCreditLabel = "Maximum Credits",
                cumulativeCompletedGpaOption = "y",
                cumulativeCompletedGpaDescription = "Cumulative GPA",
                cumulativeCompletedGpaLabel = "GPA",
                cumulativeCompletedCreditsOption = "y",
                cumulativeCompletedCreditsDescription = "Cumulative Completed Credits",
                cumulativeCompletedCreditsLabel = "CumCmplCred",
                cumulativeAttemptedCreditsOption = "y",
                cumulativeAttemptedCreditsDescription = "Attempted Credits, Cumulative",
                cumulativeAttemptedCreditsLabel = "AttCredCum",
                cumulativePaceOption = "y",
                cumulativePaceDescription = "Cumulative Pace",
                cumulativePaceLabel = "PaceCumulative",
                cumulativeCompletedCreditsExcludingRemedialOption = "y",
                cumulativeCompletedCreditsExcludingRemedialDescription = "Cumulative Completed Credits Excluding Remedial",
                cumulativeCompletedCreditsExcludingRemedialLabel = "CumCmplCredExlRem",
                cumulativeAttemptedCreditsExcludingRemedialOption = "y",
                cumulativeAttemptedCreditsExcludingRemedialDescription = "Cumulative Attempted Credits Excluding Remedial",
                cumulativeAttemptedCreditsExcludingRemedialLabel = "Label for CumAttCredExlRem",
                cumulativePaceExcludingRemedialOption = "y",
                cumulativePaceExcludingRemedialDescription = "Cumulative Pace Excluding Remedial",
                cumulativePaceExcludingRemedialLabel = "Label for No remedial pace cumulative",                
                evaluationPeriodCompletedGpaOption = "y",
                evaluationPeriodCompletedGpaDescription = "Completed GPA for Eval Period",
                evaluationPeriodCompletedGpaLabel = "Label for Completed GPA Eval Period",
                evaluationPeriodCompletedCreditsOption = "y",
                evaluationPeriodCompletedCreditsDescription = "Evaluation Period Completed Credits",
                evaluationPeriodCompletedCreditsLabel = "Completed Credits in Evaluation Period",
                evaluationPeriodAttemptedCreditsOption = "y",
                evaluationPeriodAttemptedCreditsDescription = "Evaluation Period Attempted Credits",
                evaluationPeriodAttemptedCreditsLabel = "Label for AttCredEval",
                evaluationPeriodPaceOption = "y",
                evaluationPeriodPaceDescription = "Pace for Eval Period",
                evaluationPeriodPaceLabel = "Eval Period Pace",                
            },
            new AcademicProgressParameters()
            {
                officeId = "LAW",
                isAcademicProgressActive = "",
                isAcademicProgressHistoryActive = "",
                numberOfAcademicProgressHistoryRecordsToDisplay = null,
                maxCreditOption = "n",
                maxCreditDescription = "Max Credits",
                maxCreditLabel = "Maximum Credits",
                cumulativeCompletedGpaOption = "N",
                cumulativeCompletedGpaDescription = "Cumulative GPA",
                cumulativeCompletedGpaLabel = "GPA",
                cumulativeCompletedCreditsOption = "",
                cumulativeCompletedCreditsDescription = "Cumulative Completed Credits",
                cumulativeCompletedCreditsLabel = "CumCmplCred",
                cumulativeAttemptedCreditsOption = string.Empty,
                cumulativeAttemptedCreditsDescription = "Attempted Credits, Cumulative",
                cumulativeAttemptedCreditsLabel = "AttCredCum",
                cumulativePaceOption = null,
                cumulativePaceDescription = "Cumulative Pace",
                cumulativePaceLabel = "PaceCumulative",
                cumulativeCompletedCreditsExcludingRemedialOption = "n",
                cumulativeCompletedCreditsExcludingRemedialDescription = "Cumulative Completed Credits Excluding Remedial",
                cumulativeCompletedCreditsExcludingRemedialLabel = "CumCmplCredExlRem",
                cumulativeAttemptedCreditsExcludingRemedialOption = "N",
                cumulativeAttemptedCreditsExcludingRemedialDescription = "Cumulative Attempted Credits Excluding Remedial",
                cumulativeAttemptedCreditsExcludingRemedialLabel = "Label for CumAttCredExlRem",
                cumulativePaceExcludingRemedialOption = "",
                cumulativePaceExcludingRemedialDescription = "Cumulative Pace Excluding Remedial",
                cumulativePaceExcludingRemedialLabel = "Label for No remedial pace cumulative",                
                evaluationPeriodCompletedGpaOption = string.Empty,
                evaluationPeriodCompletedGpaDescription = "Completed GPA for Eval Period",
                evaluationPeriodCompletedGpaLabel = "Label for Completed GPA Eval Period",
                evaluationPeriodCompletedCreditsOption = null,
                evaluationPeriodCompletedCreditsDescription = "Evaluation Period Completed Credits",
                evaluationPeriodCompletedCreditsLabel = "Completed Credits in Evaluation Period",
                evaluationPeriodAttemptedCreditsOption = "n",
                evaluationPeriodAttemptedCreditsDescription = "Evaluation Period Attempted Credits",
                evaluationPeriodAttemptedCreditsLabel = "Label for AttCredEval",
                evaluationPeriodPaceOption = "N",
                evaluationPeriodPaceDescription = "Pace for Eval Period",
                evaluationPeriodPaceLabel = "Eval Period Pace",                
            },
        };

        #endregion

        #region Office Records

        public class OfficeRecord
        {
            public string Id;
            public string Name;
            public List<string> Address;
            public string City;
            public string State;
            public string Zip;
            public string PhoneNumber;
            public string Email;
            public string DirectorName;
            public string OpeId;
            public string TitleIVCode;
            public string DefaultDisplayYearCode;
        }

        public List<OfficeRecord> officeRecordData = new List<OfficeRecord>()
        {
            new OfficeRecord()
            {
                Id = "MAIN",
                Name = "Main Office",
                Address = new List<string>() {"2375 Fair Lakes Court"},
                City = "Fairfax",
                State = "VA",
                Zip = "22033",
                PhoneNumber = "555-555-5555",
                Email = "mainfaoffice@ellucian.edu",
                DirectorName = "Cindy Lou",
                OpeId = string.Empty,
                TitleIVCode = "U5364",
                DefaultDisplayYearCode = "2016"
            },
            new OfficeRecord()
            {
                Id = "LAW",
                Name = "Law Office",
                Address = new List<string>() {"33 Legal Lane", "Building 5"},
                City = "Malvern",
                State = "PA",
                Zip = "00000",
                PhoneNumber = "777-777-6666",
                Email = "lawfaoffice@ellucian.edu",
                DirectorName = "J.S. Bach",
                OpeId = "00111222",
                TitleIVCode = "U5364",
                DefaultDisplayYearCode = "2015"
            }
        };

        #endregion

        #region Other Records

        public class LocationRecord
        {
            public string Id;
            public string OfficeId;
        }

        public List<LocationRecord> locationRecordData = new List<LocationRecord>()
        {
            new LocationRecord() {Id = "MC", OfficeId = "MAIN"},
            new LocationRecord() {Id = "LC", OfficeId = "LAW"},
            new LocationRecord() {Id = "TD", OfficeId = "LAW"},
            new LocationRecord() {Id = "DD", OfficeId = "LAW"}
        };

        //This mimics a combination of FahubParms and FaSysParams and STWEB.DEFAULTS
        public class DefaultSystemParametersRecord
        {
            public string MainOfficeId;
            public string OpeId;
            public string TitleIVCode;
        }

        public DefaultSystemParametersRecord defaultSystemParametersRecordData = new DefaultSystemParametersRecord()
        {
            MainOfficeId = "MAIN",
            OpeId = "00999888",
            TitleIVCode = "M563425"
        };

        #endregion

        public IEnumerable<FinancialAidOffice> GetFinancialAidOffices()
        {
            //Build Offices
            var officeList = new List<FinancialAidOffice>();
            foreach (var officeRecord in officeRecordData)
            {
                var officeAddress = new List<string>();
                if (officeRecord.Address != null &&
                                officeRecord.Address.Count() > 0 &&
                                officeRecord.Address.Any(a => !string.IsNullOrEmpty(a)) &&
                                !string.IsNullOrEmpty(officeRecord.City) &&
                                !string.IsNullOrEmpty(officeRecord.State) &&
                                !string.IsNullOrEmpty(officeRecord.Zip))
                {
                    officeAddress.AddRange(officeRecord.Address);

                    var csz = string.Format("{0}, {1} {2}", officeRecord.City, officeRecord.State, officeRecord.Zip);
                    officeAddress.Add(csz);
                }

                var officeLocationRecords = locationRecordData.Where(l => l.OfficeId == officeRecord.Id);

                var office = new FinancialAidOffice(officeRecord.Id)
                {
                    Name = officeRecord.Name,
                    AddressLabel = officeAddress,
                    PhoneNumber = officeRecord.PhoneNumber,
                    EmailAddress = officeRecord.Email,
                    DirectorName = officeRecord.DirectorName,
                    LocationIds = officeLocationRecords.Select(loc => loc.Id).ToList(),
                    IsDefault = (defaultSystemParametersRecordData.MainOfficeId == officeRecord.Id),
                    OpeId = (!string.IsNullOrEmpty(officeRecord.OpeId)) ? officeRecord.OpeId : defaultSystemParametersRecordData.OpeId,
                    TitleIVCode = (!string.IsNullOrEmpty(officeRecord.TitleIVCode)) ? officeRecord.TitleIVCode : defaultSystemParametersRecordData.TitleIVCode,
                    DefaultDisplayYearCode = officeRecord.DefaultDisplayYearCode
                };

                officeList.Add(office);

                //extract the parameters specific to this office
                var officeParameters = officeParameterRecordData.Where(p => p.OfficeCode == office.Id);
                var shoppingSheetParameters = shoppingSheetParameterData.Where(p => p.OpeId == office.OpeId);
                var academicProgressParameters = academicProgressParameterData.FirstOrDefault(p => p.officeId == office.Id);

                //build a list of award years for which parameter records exist
                var parameterYears = officeParameters.Select(op => op.AwardYear).Concat(shoppingSheetParameters.Select(sp => sp.AwardYear)).Distinct();

                //for each year, build a configuration object
                var configurations = parameterYears.Select(year =>
                        BuildOfficeConfiguration(office.Id, year, officeParameters.FirstOrDefault(p => p.AwardYear == year), shoppingSheetParameters.FirstOrDefault(p => p.AwardYear == year))
                    );

                office.AddConfigurationRange(configurations);

                office.AcademicProgressConfiguration = BuildAcademicProgressConfiguration(office.Id, academicProgressParameters);
            }

            return officeList;

        }

        public Task<IEnumerable<FinancialAidOffice>> GetFinancialAidOfficesAsync()
        {
            //Build Offices
            var officeList = new List<FinancialAidOffice>();
            foreach (var officeRecord in officeRecordData)
            {
                var officeAddress = new List<string>();
                if (officeRecord.Address != null &&
                                officeRecord.Address.Count() > 0 &&
                                officeRecord.Address.Any(a => !string.IsNullOrEmpty(a)) &&
                                !string.IsNullOrEmpty(officeRecord.City) &&
                                !string.IsNullOrEmpty(officeRecord.State) &&
                                !string.IsNullOrEmpty(officeRecord.Zip))
                {
                    officeAddress.AddRange(officeRecord.Address);

                    var csz = string.Format("{0}, {1} {2}", officeRecord.City, officeRecord.State, officeRecord.Zip);
                    officeAddress.Add(csz);
                }

                var officeLocationRecords = locationRecordData.Where(l => l.OfficeId == officeRecord.Id);

                var office = new FinancialAidOffice(officeRecord.Id)
                {
                    Name = officeRecord.Name,
                    AddressLabel = officeAddress,
                    PhoneNumber = officeRecord.PhoneNumber,
                    EmailAddress = officeRecord.Email,
                    DirectorName = officeRecord.DirectorName,
                    LocationIds = officeLocationRecords.Select(loc => loc.Id).ToList(),
                    IsDefault = (defaultSystemParametersRecordData.MainOfficeId == officeRecord.Id),
                    OpeId = (!string.IsNullOrEmpty(officeRecord.OpeId)) ? officeRecord.OpeId : defaultSystemParametersRecordData.OpeId,
                    TitleIVCode = (!string.IsNullOrEmpty(officeRecord.TitleIVCode)) ? officeRecord.TitleIVCode : defaultSystemParametersRecordData.TitleIVCode,
                    DefaultDisplayYearCode = officeRecord.DefaultDisplayYearCode
                };

                officeList.Add(office);

                //extract the parameters specific to this office
                var officeParameters = officeParameterRecordData.Where(p => p.OfficeCode == office.Id);
                var shoppingSheetParameters = shoppingSheetParameterData.Where(p => p.OpeId == office.OpeId);
                var academicProgressParameters = academicProgressParameterData.FirstOrDefault(p => p.officeId == office.Id);

                //build a list of award years for which parameter records exist
                var parameterYears = officeParameters.Select(op => op.AwardYear).Concat(shoppingSheetParameters.Select(sp => sp.AwardYear)).Distinct();

                //for each year, build a configuration object
                var configurations = parameterYears.Select(year =>
                        BuildOfficeConfiguration(office.Id, year, officeParameters.FirstOrDefault(p => p.AwardYear == year), shoppingSheetParameters.FirstOrDefault(p => p.AwardYear == year))
                    );

                office.AddConfigurationRange(configurations);

                office.AcademicProgressConfiguration = BuildAcademicProgressConfiguration(office.Id, academicProgressParameters);
            }

            return Task.FromResult(officeList.AsEnumerable());

        }

        private AcademicProgressConfiguration BuildAcademicProgressConfiguration(string officeId, AcademicProgressParameters parameterRecord)
        {
            if (parameterRecord == null)
            {
                return new AcademicProgressConfiguration(officeId)
                {
                    IsSatisfactoryAcademicProgressActive = false,
                    DetailPropertyConfigurations = new List<AcademicProgressPropertyConfiguration>(),
                };
            }

            var config = new AcademicProgressConfiguration(officeId)
            {
                IsSatisfactoryAcademicProgressActive = !string.IsNullOrEmpty(parameterRecord.isAcademicProgressActive) && parameterRecord.isAcademicProgressActive.ToUpper() == "Y",
                DetailPropertyConfigurations = new List<AcademicProgressPropertyConfiguration>(),
            };

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.MaximumProgramCredits)
                {
                    Label = parameterRecord.maxCreditLabel,
                    Description = parameterRecord.maxCreditDescription,
                    IsHidden = string.IsNullOrEmpty(parameterRecord.maxCreditOption) || parameterRecord.maxCreditOption.ToUpper() != "Y",
                });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodRateOfCompletion)
            {
                Label = parameterRecord.evaluationPeriodPaceLabel,
                Description = parameterRecord.evaluationPeriodPaceDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.evaluationPeriodPaceOption) || parameterRecord.evaluationPeriodPaceOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodOverallGpa)
            {
                Label = parameterRecord.evaluationPeriodCompletedGpaLabel,
                Description = parameterRecord.evaluationPeriodCompletedGpaDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.evaluationPeriodCompletedGpaOption) || parameterRecord.evaluationPeriodCompletedGpaOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodCompletedCredits)
            {
                Label = parameterRecord.evaluationPeriodCompletedCreditsLabel,
                Description = parameterRecord.evaluationPeriodCompletedCreditsDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.evaluationPeriodCompletedCreditsOption) || parameterRecord.evaluationPeriodCompletedCreditsOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.EvaluationPeriodAttemptedCredits)
            {
                Label = parameterRecord.evaluationPeriodAttemptedCreditsLabel,
                Description = parameterRecord.evaluationPeriodAttemptedCreditsDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.evaluationPeriodAttemptedCreditsOption) || parameterRecord.evaluationPeriodAttemptedCreditsOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeRateOfCompletionExcludingRemedial)
            {
                Label = parameterRecord.cumulativePaceExcludingRemedialLabel,
                Description = parameterRecord.cumulativePaceExcludingRemedialDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativePaceExcludingRemedialOption) || parameterRecord.cumulativePaceExcludingRemedialOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeRateOfCompletion)
            {
                Label = parameterRecord.cumulativePaceLabel,
                Description = parameterRecord.cumulativePaceDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativePaceOption) || parameterRecord.cumulativePaceOption.ToUpper() != "Y",
            });
            
            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeOverallGpa)
            {
                Label = parameterRecord.cumulativeCompletedGpaLabel,
                Description = parameterRecord.cumulativeCompletedGpaDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativeCompletedGpaOption) || parameterRecord.cumulativeCompletedGpaOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeCompletedCreditsExcludingRemedial)
            {
                Label = parameterRecord.cumulativeCompletedCreditsExcludingRemedialLabel,
                Description = parameterRecord.cumulativeCompletedCreditsExcludingRemedialDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativeCompletedCreditsExcludingRemedialOption) || parameterRecord.cumulativeCompletedCreditsExcludingRemedialOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeCompletedCredits)
            {
                Label = parameterRecord.cumulativeCompletedCreditsLabel,
                Description = parameterRecord.cumulativeCompletedCreditsDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativeCompletedCreditsOption) || parameterRecord.cumulativeCompletedCreditsOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCreditsExcludingRemedial)
            {
                Label = parameterRecord.cumulativeAttemptedCreditsExcludingRemedialLabel,
                Description = parameterRecord.cumulativeAttemptedCreditsExcludingRemedialDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativeAttemptedCreditsExcludingRemedialOption) || parameterRecord.cumulativeAttemptedCreditsExcludingRemedialOption.ToUpper() != "Y",
            });

            config.DetailPropertyConfigurations.Add(new AcademicProgressPropertyConfiguration(AcademicProgressPropertyType.CumulativeAttemptedCredits)
            {
                Label = parameterRecord.cumulativeAttemptedCreditsLabel,
                Description = parameterRecord.cumulativeAttemptedCreditsDescription,
                IsHidden = string.IsNullOrEmpty(parameterRecord.cumulativeAttemptedCreditsOption) || parameterRecord.cumulativeAttemptedCreditsOption.ToUpper() != "Y",
            });

            return config;
        }

        private FinancialAidConfiguration BuildOfficeConfiguration(string officeId, string awardYear, OfficeParameterRecord officeParameterRecord, ShoppingSheetParameterRecord shoppingSheetParameterRecord)
        {
            if (officeParameterRecord == null && shoppingSheetParameterRecord == null)
            {
                return null;
            }

            var singleConfiguration = new FinancialAidConfiguration(officeId, awardYear);

            if (officeParameterRecord != null)
            {
                singleConfiguration.AwardYearDescription = officeParameterRecord.AwardYearDescription;

                try
                {
                    singleConfiguration.UndergraduatePackage =
                        new AverageAwardPackage(officeParameterRecord.AverageUndergradGrantAmount, officeParameterRecord.AverageUndergradLoanAmount, officeParameterRecord.AverageUndergradScholarshipAmount, officeParameterRecord.AwardYear);
                }
                catch (Exception)
                {
                    singleConfiguration.UndergraduatePackage = null;
                }

                try
                {
                    singleConfiguration.GraduatePackage =
                        new AverageAwardPackage(officeParameterRecord.AverageGradGrantAmount, officeParameterRecord.AverageGradLoanAmount, officeParameterRecord.AverageGradScholarshipAmount, officeParameterRecord.AwardYear);
                }
                catch (Exception)
                {
                    singleConfiguration.GraduatePackage = null;
                }

                singleConfiguration.IsSelfServiceActive = !string.IsNullOrEmpty(officeParameterRecord.IsSelfServiceActive) && officeParameterRecord.IsSelfServiceActive.ToUpper() == "Y";
                singleConfiguration.IsAwardingActive = !string.IsNullOrEmpty(officeParameterRecord.IsAwardingActive) && officeParameterRecord.IsAwardingActive.ToUpper() == "Y";
                singleConfiguration.AreAwardChangesAllowed = !string.IsNullOrEmpty(officeParameterRecord.CanMakeAwardChanges) && officeParameterRecord.CanMakeAwardChanges.ToUpper() == "Y";
                singleConfiguration.AllowAnnualAwardUpdatesOnly = (!string.IsNullOrEmpty(officeParameterRecord.AnnualAccRejOnly) && officeParameterRecord.AnnualAccRejOnly.ToUpper() == "Y");

                singleConfiguration.IsProfileActive = !string.IsNullOrEmpty(officeParameterRecord.IsProfileActive) && officeParameterRecord.IsProfileActive.ToUpper() == "Y";
                singleConfiguration.IsAwardLetterActive = !string.IsNullOrEmpty(officeParameterRecord.IsAwardLetterActive) && officeParameterRecord.IsAwardLetterActive.ToUpper() == "Y";
                singleConfiguration.IsShoppingSheetActive = !string.IsNullOrEmpty(officeParameterRecord.IsShoppingSheetActive) && officeParameterRecord.IsShoppingSheetActive.ToUpper() == "Y";
                singleConfiguration.AreLoanRequestsAllowed = !string.IsNullOrEmpty(officeParameterRecord.AreLoanRequestsAllowed) && officeParameterRecord.AreLoanRequestsAllowed.ToUpper() == "Y";

                singleConfiguration.ExcludeAwardStatusCategoriesView =
                    TranslateCodeToAwardStatusCategory(officeParameterRecord.AwardStatusCategoriesToExcludeFromView).ToList();

                singleConfiguration.ExcludeAwardCategoriesView = officeParameterRecord.AwardCategoriesToExcludeFromView;
                singleConfiguration.ExcludeAwardPeriods = officeParameterRecord.AwardPeriodsToExcludeFromView;
                singleConfiguration.ExcludeAwardsView = officeParameterRecord.AwardsToExcludeFromView;
                singleConfiguration.ExcludeAwardStatusesFromChange = officeParameterRecord.ActionStatusesToPreventChanges;

                singleConfiguration.ExcludeAwardStatusCategoriesFromChange =
                    TranslateCodeToAwardStatusCategory(officeParameterRecord.AwardStatusCategoriesToPreventChanges).ToList();
                singleConfiguration.ExcludeAwardCategoriesFromChange = officeParameterRecord.AwardCategoriesToPreventChanges;
                singleConfiguration.ExcludeAwardsFromChange = officeParameterRecord.AwardsToPreventChanges;
                singleConfiguration.ChecklistItemCodes = officeParameterRecord.ChecklistItemCodes;
                singleConfiguration.ChecklistItemControlStatuses = officeParameterRecord.ChecklistItemControlStatuses;
                singleConfiguration.ChecklistItemDefaultFlags = officeParameterRecord.ChecklistItemDefaultFlags;
                singleConfiguration.IgnoreAwardStatusesFromEval = officeParameterRecord.IgnoreAwardStatusesFromEval;
                singleConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet =
                    TranslateCodeToAwardStatusCategory(officeParameterRecord.AwardStatusCategoriesToExcludeFromAwardLetter).ToList();
                singleConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet = officeParameterRecord.AwardsToExcludeFromAwardLetter;
                singleConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = officeParameterRecord.AwardPeriodsToExcludeFromAwardLetter;
                singleConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = officeParameterRecord.AwardCategoriesToExcludeFromAwardLetter;

                singleConfiguration.AcceptedAwardStatusCode = officeParameterRecord.AcceptedAwardAction;
                singleConfiguration.AcceptedAwardCommunicationCode = officeParameterRecord.AcceptedAwardCommunicationCode;
                singleConfiguration.AcceptedAwardCommunicationStatus = officeParameterRecord.AcceptedAwardCommunicationStatus;
                singleConfiguration.RejectedAwardStatusCode = officeParameterRecord.RejectedAwardAction;
                singleConfiguration.RejectedAwardCommunicationCode = officeParameterRecord.RejectedAwardCommunicationCode;
                singleConfiguration.RejectedAwardCommunicationStatus = officeParameterRecord.RejectedAwardCommunicationStatus;

                singleConfiguration.AllowNegativeUnmetNeedBorrowing = !string.IsNullOrEmpty(officeParameterRecord.AllowNegativeUnmetNeedBorrowing) && officeParameterRecord.AllowNegativeUnmetNeedBorrowing.ToUpper() == "Y";
                singleConfiguration.AllowLoanChanges = !string.IsNullOrEmpty(officeParameterRecord.AllowLoanChanges) && officeParameterRecord.AllowLoanChanges.ToUpper() == "Y";
                singleConfiguration.AllowLoanChangeIfAccepted =!string.IsNullOrEmpty(officeParameterRecord.AllowLoanChangeIfAccepted) && officeParameterRecord.AllowLoanChangeIfAccepted.ToUpper() == "Y";
                singleConfiguration.AllowDeclineZeroOfAcceptedLoans = !string.IsNullOrEmpty(officeParameterRecord.AllowDeclineZeroOutIfAccepted) && officeParameterRecord.AllowDeclineZeroOutIfAccepted.ToUpper() == "Y";
                singleConfiguration.SuppressInstanceData = !string.IsNullOrEmpty(officeParameterRecord.SuppressInstanceData) && officeParameterRecord.SuppressInstanceData.ToUpper() == "Y";
                singleConfiguration.IsAwardLetterHistoryActive = !string.IsNullOrEmpty(officeParameterRecord.IsAwardLetterHistoryActive) && officeParameterRecord.IsAwardLetterHistoryActive.ToUpper() == "Y";
                singleConfiguration.NewLoanCommunicationCode = officeParameterRecord.NewLoanCommunicationCode;
                singleConfiguration.NewLoanCommunicationStatus = officeParameterRecord.NewLoanCommunicationStatus;
                singleConfiguration.LoanChangeCommunicationCode = officeParameterRecord.LoanChangeCommunicationCode;
                singleConfiguration.LoanChangeCommunicationStatus = officeParameterRecord.LoanChangeCommunicationStatus;
                singleConfiguration.PaperCopyOptionText = officeParameterRecord.PaperCopyOptionText;

                singleConfiguration.IsDeclinedStatusChangeRequestRequired = (!string.IsNullOrEmpty(officeParameterRecord.ReviewDeclinedAwards) && officeParameterRecord.ReviewDeclinedAwards.ToUpper() == "Y");
                singleConfiguration.IsLoanAmountChangeRequestRequired = (!string.IsNullOrEmpty(officeParameterRecord.ReviewLoanChanges) && officeParameterRecord.ReviewLoanChanges.ToUpper() == "Y");
                singleConfiguration.CounselorPhoneType = officeParameterRecord.CounselorPhoneType;
                singleConfiguration.UseDefaultContact = !string.IsNullOrEmpty(officeParameterRecord.UseDefaultContact) && officeParameterRecord.UseDefaultContact.ToUpper() == "Y";
                singleConfiguration.SuppressMaximumLoanLimits = !string.IsNullOrEmpty(officeParameterRecord.SuppressMaximumLoanLimits) && officeParameterRecord.SuppressMaximumLoanLimits.ToUpper() == "Y";

                singleConfiguration.DisplayPellLifetimeEarningsUsed = !string.IsNullOrEmpty(officeParameterRecord.DisplayPellLifetimeEarningsUsed) && officeParameterRecord.DisplayPellLifetimeEarningsUsed.ToUpper() == "Y";
                singleConfiguration.SuppressAwardLetterAcceptance = !string.IsNullOrEmpty(officeParameterRecord.SuppressAwardLetterAcceptance) && officeParameterRecord.SuppressAwardLetterAcceptance.ToUpper() == "Y";
                singleConfiguration.SuppressDisbursementInfoDisplay = !string.IsNullOrEmpty(officeParameterRecord.SuppressDisbursementInfoDisplay) && officeParameterRecord.SuppressDisbursementInfoDisplay.ToUpper() == "Y";

                singleConfiguration.IgnoreAwardCategoriesOnChecklist = officeParameterRecord.IgnoreAwardCategoriesOnChecklist;
                singleConfiguration.IgnoreAwardsOnChecklist = officeParameterRecord.IgnoreAwardsOnChecklist;
                singleConfiguration.IgnoreAwardStatusesOnChecklist = officeParameterRecord.IgnoreActionStatusesOnChecklist;

                singleConfiguration.ShowAslaInfo = !string.IsNullOrEmpty(officeParameterRecord.ShowAslaInfo) && officeParameterRecord.ShowAslaInfo.ToUpper() == "Y";
            }

            if (shoppingSheetParameterRecord != null)
            {
                var officeType = ShoppingSheetOfficeType.BachelorDegreeGranting;
                if (!string.IsNullOrEmpty(shoppingSheetParameterRecord.OfficeType))
                {
                    switch (shoppingSheetParameterRecord.OfficeType.ToUpper())
                    {
                        case "1":
                            officeType = ShoppingSheetOfficeType.BachelorDegreeGranting;
                            break;
                        case "2":
                            officeType = ShoppingSheetOfficeType.AssociateDegreeGranting;
                            break;
                        case "3":
                            officeType = ShoppingSheetOfficeType.CertificateGranting;
                            break;
                        case "4":
                            officeType = ShoppingSheetOfficeType.GraduateDegreeGranting;
                            break;
                        case "5":
                            officeType = ShoppingSheetOfficeType.NonDegreeGranting;
                            break;
                    }
                }

                var efcOption = ShoppingSheetEfcOption.IsirEfc;
                var useProfileImEfc = !string.IsNullOrEmpty(shoppingSheetParameterRecord.UseProfileImEfc) && shoppingSheetParameterRecord.UseProfileImEfc.ToUpper() == "Y";
                var useProfileImUntilIsirIsFederal = !string.IsNullOrEmpty(shoppingSheetParameterRecord.UseProfileImUntilIsirIsFederal) && shoppingSheetParameterRecord.UseProfileImUntilIsirIsFederal.ToUpper() == "Y";
                if (singleConfiguration.IsProfileActive && useProfileImEfc)
                {
                    if (useProfileImUntilIsirIsFederal)
                    {
                        efcOption = ShoppingSheetEfcOption.ProfileEfcUntilIsirExists;
                    }
                    else
                    {
                        efcOption = ShoppingSheetEfcOption.ProfileEfc;
                    }
                }

                singleConfiguration.ShoppingSheetConfiguration = new ShoppingSheetConfiguration()
                {
                    CustomMessageRuleTableId = shoppingSheetParameterRecord.CustomMessageRuleTableId,
                    GraduationRate = shoppingSheetParameterRecord.GraduationRate,
                    LoanDefaultRate = shoppingSheetParameterRecord.LoanDefaultRate,
                    NationalLoanDefaultRate = shoppingSheetParameterRecord.NationalLoanDefaultRate,
                    MedianBorrowingAmount = shoppingSheetParameterRecord.MedianBorrowingAmount,
                    MedianMonthlyPaymentAmount = shoppingSheetParameterRecord.MedianMonthlyPaymentAmount,
                    OfficeType = officeType,
                    LowToMediumBoundary = shoppingSheetParameterRecord.LowToMediumBoundary,
                    MediumToHighBoundary = shoppingSheetParameterRecord.MediumToHighBoundary,
                    EfcOption = efcOption,
                    InstitutionRepaymentRate = shoppingSheetParameterRecord.InstitutionRepaymentRate,
                    NationalRepaymentRateAverage = shoppingSheetParameterRecord.NationalRepaymentRateAverage
                };



            }

            return singleConfiguration;
        }

        private IEnumerable<AwardStatusCategory> TranslateCodeToAwardStatusCategory(IEnumerable<string> codes)
        {
            var awardStatusCategories = new List<AwardStatusCategory>();
            if (codes == null) return awardStatusCategories;

            foreach (var code in codes)
            {
                var category = TranslateCodeToAwardStatusCategory(code);
                if (category.HasValue) awardStatusCategories.Add(category.Value);
            }

            return awardStatusCategories;
        }

        private AwardStatusCategory? TranslateCodeToAwardStatusCategory(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            switch (code.ToUpper())
            {
                case "A":
                    return AwardStatusCategory.Accepted;
                case "P":
                    return AwardStatusCategory.Pending;
                case "E":
                    return AwardStatusCategory.Estimated;
                case "R":
                    return AwardStatusCategory.Rejected;
                case "D":
                    return AwardStatusCategory.Denied;
                default:
                    return null;
            }
        }


        public Task<IEnumerable<FinancialAidOfficeItem>> GetFinancialAidOfficesAsync(bool ignoreCache)
        {
            throw new NotImplementedException();
        }
    }
}
