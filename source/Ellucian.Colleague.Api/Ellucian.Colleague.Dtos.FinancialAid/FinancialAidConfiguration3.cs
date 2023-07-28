/*Copyright 2016-2019 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A FinancialAidConfiguration3 DTO contains data about how a Financial Aid office
    /// wants to expose data and control certain actions for a given award year
    /// </summary>
    public class FinancialAidConfiguration3
    {
        /// <summary>
        /// The Id of the office that this configuration is assigned to.
        /// </summary>
        public string OfficeId { get; set; }

        /// <summary>
        /// Award year this configuration object applies to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// A custom description of the award year
        /// </summary>
        public string AwardYearDescription { get; set; }

        /// <summary>
        /// Flag indicating if the Self-Service is active for this office/year
        /// </summary>
        public bool IsSelfServiceActive { get; set; }

        /// <summary>
        /// Flag indicating if the SelfService My Awards page is active for this office/year
        /// </summary>
        public bool IsAwardingActive { get; set; }

        /// <summary>
        /// Flag indicating if the SelfService Award Letter page is active for this office/year
        /// </summary>
        public bool IsAwardLetterActive { get; set; }

        /// <summary>
        /// Flag indicating if the SelfService ShoppingSheet page is active for this office/year
        /// It's still possible for the ShoppingSheetConfiguration object to be null
        /// even if the shopping sheet is active. This indicates the office wants to 
        /// display shopping sheets to students, but hasn't configured the shopping sheet
        /// for this year yet.
        /// </summary>
        public bool IsShoppingSheetActive { get; set; }

        /// <summary>
        /// Indicates whether the student needs to complete a profile application for this award year.
        /// </summary>
        public bool IsProfileActive { get; set; }

        /// <summary>
        /// Indicates whether or not to display document instances for the year
        /// to the student
        /// </summary>
        public bool SuppressInstanceData { get; set; }

        /// <summary>
        /// Indicates whether students are allowed to request a loan for a given year.
        /// </summary>
        public bool AreLoanRequestsAllowed { get; set; }

        /// <summary>
        /// Indicates whether students are allowed to make awarding changes for a given year.
        /// </summary>
        public bool AreAwardChangesAllowed { get; set; }

        /// <summary>
        /// Indicates whether students are allowed to accept/reject awards at annual 
        /// or award period level
        /// </summary>
        public bool AllowAnnualAwardUpdatesOnly { get; set; }

        /// <summary>
        /// Text explaining paper copy option
        /// </summary>
        public string PaperCopyOptionText { get; set; }

        /// <summary>
        /// Flag indicating whether to create a change request for a loan amount change (true), or to update the loan amount
        /// in a student's package immediately (false)
        /// </summary>
        public bool IsLoanAmountChangeRequestRequired { get; set; }

        /// <summary>
        /// Flag indicating whether to create a change request for a Declined Status change (true), or to update the status
        /// of an award in the student's package immediately (false)
        /// </summary>
        public bool IsDeclinedStatusChangeRequestRequired { get; set; }

        /// <summary>
        /// Award Status to use for Accepting an award
        /// </summary>
        public string AcceptedAwardStatusCode { get; set; }

        /// <summary>
        /// Award Status to use when Rejecting an award
        /// </summary>
        public string RejectedAwardStatusCode { get; set; }

        /// <summary>
        /// Configuration options for creating a financial aid shopping sheet for this office and award year
        /// </summary>
        public ShoppingSheetConfiguration ShoppingSheetConfiguration { get; set; }

        /// <summary>
        /// Configuration option stating which phone type to use for the student's FA Counselor.
        /// </summary>
        public string CounselorPhoneType { get; set; }

        /// <summary>
        /// Configuration option telling us whether we should display award letter history records to students
        /// </summary>
        public bool IsAwardLetterHistoryActive { get; set; }

        /// <summary>
        /// Configuration option stating whether SAP is available 
        /// </summary>
        public bool IsSatisfactoryAcademicProgressActive { get; set; }

        /// <summary>
        /// Action Statuses to Exclude from Changes
        /// </summary>
        public List<string> ExcludeAwardStatusesFromChange { get; set; }

        /// <summary>
        /// Flag indicating whether to create checklist items for a new student for this FA Office
        /// </summary>
        public bool CreateChecklistItemsForNewStudent { get; set; }

        /// <summary>
        /// Flag indicating if Accepting and Rejecting an award can only be done on the Annual level
        /// </summary>
        public bool AnnualAcceptRejectOnlyFlag { get; set; }

        /// <summary>
        /// Indicates that we should use the default contact information as opposed to the FA Counselor assigned to the student.
        /// </summary>
        public bool UseDefaultContact { get; set; }

        /// <summary>
        /// Indicates if maximum loan limits should be supressed for the office/year
        /// </summary>
        public bool SuppressMaximumLoanLimits { get; set; }

        /// <summary>
        /// Indicates whether custom defined document status description must be used
        /// </summary>
        public bool UseDocumentStatusDescription { get; set; }

        /// <summary>
        /// Indicates if we should display the PELL Lifetime Eligibility Used percentage.
        /// </summary>
        public bool DisplayPellLifetimeEarningsUsed { get; set; }

        /// <summary>
        /// Indicates whether to suppress displaying of student account summary information
        /// </summary>
        public bool SuppressAccountSummaryDisplay { get; set; }

        /// <summary>
        /// Indicates whether to suppress displaying of average award package data
        /// </summary>
        public bool SuppressAverageAwardPackageDisplay { get; set; }

        /// <summary>
        /// Flag indicating if a student can Decline or Zero Out an already Accepted loan
        /// </summary>
        public bool AllowDeclineZeroOfAcceptedLoans { get; set; }

        /// <summary>
        /// Flag indicating whether students are required to accept award letter
        /// </summary>
        public bool SuppressAwardLetterAcceptance { get; set; }

        /// <summary>
        /// Flag indicating whether to suppress disbursement info display
        /// </summary>
        public bool SuppressDisbursementInfoDisplay { get; set; }

        /// <summary>
        /// Flag to indicate whether to display budget breakdown on student award letter
        /// </summary>
        public bool ShowBudgetDetailsOnAwardLetter { get; set; }

        /// <summary>
        /// Budget details description string to display in the budget breakdown view on 
        /// award letter
        /// </summary>
        public string StudentAwardLetterBudgetDetailsDescription { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the Studente Loan Checklist Items
        /// </summary>
        public bool ShowStudentLoanInfo { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the Parent Loan Checklist Items
        /// </summary>
        public bool ShowParentLoanInfo { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the Plus Application Checklist Items
        /// </summary>
        public bool ShowPlusApplicationInfo { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the ASLA Checklist Item
        /// </summary>
        public bool ShowAslaInfo { get; set; }

        /// <summary>
        /// Message to display if no status is assigned for required documents
        /// </summary>
        public string FaBlankStatusText { get; set; }


        /// <summary>
        /// Message to display if no due date is assigned for required documents
        /// </summary>
        public string FaBlankDueDateText { get; set; }

        /// <summary>
        /// Message to display as an alert on all FA pages
        /// </summary>
        public string FspNotificationText { get; set; }

        /// <summary>
        /// Flag indicating if loan amounts are able to be decreased only
        /// </summary>
        public bool AllowLoanDecreaseOnly { get; set; }

        /// <summary>
        /// Flag indicating whether or not to suppress the Course Credits
        /// </summary>
        public bool SuppressCourseCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not to suppress the Inst Credits
        /// </summary>
        public bool SuppressInstCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not to suppress the TIV Credits
        /// </summary>
        public bool SuppressTivCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not to suppress the Pell Credits
        /// </summary>
        public bool SuppressPellCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not to suppress the DL Credits
        /// </summary>
        public bool SuppressDlCredits { get; set; }
        /// <summary>
        /// User defined text to explain what Course Credits are
        /// </summary>
        public string CourseCreditsExplanation { get; set; }
        /// <summary>
        /// User defined text to explain what Inst Credits are
        /// </summary>
        public string InstCreditsExplanation { get; set; }
        /// <summary>
        /// User defined text to explain what TIV Credits are
        /// </summary>
        public string TivCreditsExplanation { get; set; }
        /// <summary>
        /// User defined text to explain what Pell Credits are
        /// </summary>
        public string PellCreditsExplanation { get; set; }
        /// <summary>
        /// User defined text to explain what DL Credits are
        /// </summary>
        public string DlCreditsExplanation { get; set; }
        /// <summary>
        /// Flag indicating whether to suppress the Program display in Enrolled Credits
        /// </summary>
        public bool SuppressProgramDisplay { get; set; }
        /// <summary>
        /// Flag indicating whether to suppress the Degree Audit column in Enrolled Credits
        /// </summary>
        public bool SuppressDegreeAudit { get; set; }
        /// <summary>
        /// User defined text to explain the Degree Audit column in Enrolled Credits
        /// </summary>
        public string DegreeAuditExplanation { get; set; }
        /// <summary>
        /// User defined text to explain the Enrolled Credits page
        /// </summary>
        public string EnrolledCreditsPageExplanation { get; set; }
        /// <summary>
        /// List of housing options to be included on the checklist for selection
        /// </summary>
        public List<string> HousingOptions { get; set; }
    }
}
