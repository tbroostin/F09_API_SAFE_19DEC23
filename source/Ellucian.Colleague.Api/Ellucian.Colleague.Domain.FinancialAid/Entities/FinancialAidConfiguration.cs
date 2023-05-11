/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// A FinancialAidConfiguration object contains data about how a Financial Aid office
    /// wants to expose data and control certain actions for a given award year
    /// </summary>
    [Serializable]
    public class FinancialAidConfiguration
    {
        /// <summary>
        /// The Id of the office that this configuration is assigned to.
        /// </summary>
        public string OfficeId { get { return _OfficeId; } }
        private readonly string _OfficeId;

        /// <summary>
        /// Award year for this office configuration parameter
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// Award year description
        /// </summary>
        public string AwardYearDescription { get; set; }

        /// <summary>
        /// Flag indicating if Self Service is active for this office/year
        /// </summary>
        public bool IsSelfServiceActive { get; set; }

        /// <summary>
        /// Flag indicating if the SelfService My Awards page is active for this office/year
        /// </summary>
        public bool IsAwardingActive { get; set; }

        /// <summary>
        /// Flag indicating if awarding changes are allowed for this office/year
        /// </summary>
        public bool AreAwardChangesAllowed { get; set; }

        /// <summary>
        /// Flag indicating whether students are allowed to accept/reject awards at annual 
        /// or award period level
        /// </summary>
        public bool AllowAnnualAwardUpdatesOnly { get; set; }

        /// <summary>
        /// Flag indicating if the SelfService Award Letter page is active for this office/year
        /// </summary>
        public bool IsAwardLetterActive { get; set; }

        /// <summary>
        /// Flag indicating if the PROFILE checklist item should be active for this office/year
        /// </summary>
        public bool IsProfileActive { get; set; }

        /// <summary>
        /// Flag indicating if the ShoppingSheet is active for this office/year
        /// It's still possible for the ShoppingSheetConfiguration object to be null
        /// even if the shopping sheet is active.
        /// </summary>
        public bool IsShoppingSheetActive { get; set; }

        /// <summary>
        /// Flag indicating if Loan Requests are allowed
        /// </summary>
        public bool AreLoanRequestsAllowed { get; set; }

        /// <summary>
        /// Award Periods to Exclude from Viewing
        /// </summary>
        public List<string> ExcludeAwardPeriods { get; set; }

        /// <summary>
        /// Action Categories to Exclude from Viewing
        /// </summary>
        public List<AwardStatusCategory> ExcludeAwardStatusCategoriesView { get; set; }

        /// <summary>
        /// Award Categories to Exclude from View
        /// </summary>
        public List<string> ExcludeAwardCategoriesView { get; set; }

        /// <summary>
        /// Award Codes to Exclude from View
        /// </summary>
        public List<string> ExcludeAwardsView { get; set; }

        /// <summary>
        /// Action Categories to Exclude from Changes
        /// </summary>
        public List<AwardStatusCategory> ExcludeAwardStatusCategoriesFromChange { get; set; }

        /// <summary>
        /// Award Categories to Exclude from Changes
        /// </summary>
        public List<string> ExcludeAwardCategoriesFromChange { get; set; }

        /// <summary>
        /// Award Codes to Exclude from Changes
        /// </summary>
        public List<string> ExcludeAwardsFromChange { get; set; }

        /// <summary>
        /// An average award package for graduate students
        /// </summary>
        public AverageAwardPackage GraduatePackage { get; set; }

        /// <summary>
        /// An average award package for undergraduate students
        /// </summary>
        public AverageAwardPackage UndergraduatePackage { get; set; }

        /// <summary>
        /// Award Status to use for Accepting an award
        /// </summary>
        public string AcceptedAwardStatusCode { get; set; }

        /// <summary>
        /// Communications Management Code to use when Accepting an award
        /// </summary>
        public string AcceptedAwardCommunicationCode { get; set; }

        /// <summary>
        /// Status to apply to the Accepted Award Communication Code record when Accepting an award
        /// </summary>
        public string AcceptedAwardCommunicationStatus { get; set; }

        /// <summary>
        /// Award Status to use when Rejecting an award
        /// </summary>
        public string RejectedAwardStatusCode { get; set; }

        /// <summary>
        /// Communications Management Code to use when Rejecting an award
        /// </summary>
        public string RejectedAwardCommunicationCode { get; set; }

        /// <summary>
        /// Status to apply to the Rejected Award Communication Code record when Rejecting an award
        /// </summary>
        public string RejectedAwardCommunicationStatus { get; set; }

        /// <summary>
        /// Flag indicating if the student can borrow in excess of Unmet Need
        /// </summary>
        public bool AllowNegativeUnmetNeedBorrowing { get; set; }

        /// <summary>
        /// Flag indicating if changes to loans are allowed
        /// </summary>
        public bool AllowLoanChanges { get; set; }

        /// <summary>
        /// Flag indicating if changes are allowed to loans once they are accepted
        /// </summary>
        public bool AllowLoanChangeIfAccepted { get; set; }

        /// <summary>
        /// Flag indicating if a student can Decline or Zero Out an already Accepted loan
        /// </summary>
        public bool AllowDeclineZeroOfAcceptedLoans { get; set; }

        /// <summary>
        /// Flag indicating if document instances are to be displayed in the web view
        /// to students
        /// </summary>
        public bool SuppressInstanceData { get; set; }

        /// <summary>
        /// Communications Management Code to use when applying for a new loan
        /// </summary>
        public string NewLoanCommunicationCode { get; set; }

        /// <summary>
        /// Status to apply to the NewLoanCommunicationCode when applying for a new loan
        /// </summary>
        public string NewLoanCommunicationStatus { get; set; }

        /// <summary>
        /// Communications Management Code to use when submitting a change to a loan
        /// </summary>
        public string LoanChangeCommunicationCode { get; set; }

        /// <summary>
        /// Status to apply to the LoanChangeCommunicationCode when change an existing loan
        /// </summary>
        public string LoanChangeCommunicationStatus { get; set; }

        /// <summary>
        /// Text explaining paper copy option
        /// </summary>
        public string PaperCopyOptionText { get; set; }

        /// <summary>
        /// Flag indicating whether it's necessary to create a change request for a loan amount change (true), or to update the loan amount
        /// in a student's package immediately (false)
        /// </summary>
        public bool IsLoanAmountChangeRequestRequired { get; set; }

        /// <summary>
        /// Flag indicating whether it's necessary to create a change request for a Declined Status change (true), or to update the status
        /// of an award in the student's package immediately (false)
        /// </summary>
        public bool IsDeclinedStatusChangeRequestRequired { get; set; }

        /// <summary>
        /// Action Categories to Exclude from the Award Letter/Shopping Sheet
        /// </summary>
        public List<AwardStatusCategory> ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet { get; set; }

        /// <summary>
        /// Award codes to Exclude from the Award Letter/Shopping Sheet
        /// </summary>
        public List<string> ExcludeAwardsFromAwardLetterAndShoppingSheet { get; set; }

        /// <summary>
        /// Award categories to Exclude from the Award Letter/Shopping Sheet
        /// </summary>
        public List<string> ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet { get; set; }

        /// <summary>
        /// Award periods to Exclude from the Award Letter/Shopping Sheet
        /// </summary>
        public List<string> ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet { get; set; }

        /// <summary>
        /// ShoppingSheet Configuration contains data about how to structure shopping sheets produced by this office and award year
        /// </summary>
        public ShoppingSheetConfiguration ShoppingSheetConfiguration { get; set; }

        /// <summary>
        /// Configuration option defining which phone type to use for the student's FA Counselor.
        /// </summary>
        public string CounselorPhoneType { get; set; }

        /// <summary>
        /// Configuration option telling us whether we should display award letter history records to students
        /// </summary>
        public bool IsAwardLetterHistoryActive { get; set; }

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
        /// Indicates whether to suppress maximum loan limits for the office/year
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
        /// Flag indicating whether to suppress the displaying of student account summary data 
        /// </summary>

        public bool SuppressAccountSummaryDisplay { get; set; }

        /// <summary>
        /// Flag indicating whether to suppress the displaying of average award package data 
        /// </summary>

        public bool SuppressAverageAwardPackageDisplay { get; set; }

        /// <summary>
        /// Flag indicating whether to suppress disburseent info display
        /// </summary>
        public bool SuppressDisbursementInfoDisplay { get; set; }

        /// <summary>
        /// List of checklist item codes defined for the office/year
        /// </summary>
        public List<string> ChecklistItemCodes { get; set; }

        /// <summary>
        /// List of control statuses for each of the checklist item codes
        /// </summary>
        public List<string> ChecklistItemControlStatuses { get; set; }

        /// <summary>
        /// List of default flags for each of the checklist items (if the flag is "Y" -
        /// checklist item is default and should be included, "N" - 
        /// it is not default and should be excluded)
        /// </summary>
        public List<string> ChecklistItemDefaultFlags { get; set; }

        /// <summary>
        /// Action Statuses to ignore on the award letter
        /// </summary>
        public List<string> IgnoreAwardStatusesFromEval { get; set; }

        /// <summary>
        /// List of Awards to ignore on the award letter
        /// </summary>
        public List<string> IgnoreAwardsFromEval { get; set; }

        /// <summary>
        /// List of Award Categories to ignore on the award letter
        /// </summary>
        public List<string> IgnoreAwardCategoriesFromEval { get; set; }

        /// <summary>
        /// Flag indicating whether students are required to accept award letter
        /// </summary>
        public bool SuppressAwardLetterAcceptance { get; set; }

        /// <summary>
        /// Action Statuses to ignore on checklist
        /// </summary>
        public List<string> IgnoreAwardStatusesOnChecklist { get; set; }

        /// <summary>
        /// List of Awards to ignore on checklist
        /// </summary>
        public List<string> IgnoreAwardsOnChecklist { get; set; }

        /// <summary>
        /// List of Award Categories to ignore on checklist
        /// </summary>
        public List<string> IgnoreAwardCategoriesOnChecklist { get; set; }

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
        /// Flag that indicates whether to display the Student Loan Checklist Items
        /// </summary>
        public bool ShowStudentLoanInfo { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the Parent Loan Checklist Items
        /// </summary>
        public bool ShowParentLoanInfo { get; set; }

        /// <summary>
        /// Flag that indicates whether to display the Plus Application Information
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
        /// Rule table ID to determine if a student can see FA Credits
        /// </summary>
        public string FaCreditsVisibilityRule { get; set; }
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
        /// Configuration constructor
        /// </summary>
        /// <param name="officeId">The id of the office to which this configuration applies</param>
        /// <param name="awardYear">The award year to which this configuration applies</param>
        public FinancialAidConfiguration(string officeId, string awardYear)
        {
            if (string.IsNullOrEmpty(officeId))
            {
                throw new ArgumentNullException("officeId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            _OfficeId = officeId;
            _AwardYear = awardYear;

            ExcludeAwardPeriods = new List<string>();
            ExcludeAwardStatusCategoriesView = new List<AwardStatusCategory>();
            ExcludeAwardCategoriesView = new List<string>();
            ExcludeAwardsView = new List<string>();
            ExcludeAwardStatusCategoriesFromChange = new List<AwardStatusCategory>();
            ExcludeAwardCategoriesFromChange = new List<string>();
            ExcludeAwardsFromChange = new List<string>();
            ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet = new List<AwardStatusCategory>();
            ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet = new List<string>();
            ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet = new List<string>();
            ExcludeAwardsFromAwardLetterAndShoppingSheet = new List<string>();
            ExcludeAwardStatusesFromChange = new List<string>();
            ChecklistItemCodes = new List<string>();
            ChecklistItemControlStatuses = new List<string>();
            ChecklistItemDefaultFlags = new List<string>();
            IgnoreAwardStatusesFromEval = new List<string>();
            IgnoreAwardCategoriesFromEval = new List<string>();
            IgnoreAwardsFromEval = new List<string>();
            IgnoreAwardStatusesOnChecklist = new List<string>();
            IgnoreAwardCategoriesOnChecklist = new List<string>();
            IgnoreAwardsOnChecklist = new List<string>();
        }

        /// <summary>
        /// Set an enumeration value for the Award Status Category
        /// </summary>
        /// <param name="category">The category of the action status.</param>
        /// <returns>updated award status code or an empty string</returns>
        public string GetUpdatedAwardStatusCode(AwardStatusCategory category)
        {
            if (category == AwardStatusCategory.Accepted)
            {
                return AcceptedAwardStatusCode;
            }
            else if (category == AwardStatusCategory.Rejected || category == AwardStatusCategory.Denied)
            {
                return RejectedAwardStatusCode;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Award Status Category field from a Colleague Action Status.
        /// </summary>
        /// <param name="status">Status code to use for lookup.</param>
        /// <returns>updated award status code or an empty string</returns>
        public string GetUpdatedAwardStatusCode(AwardStatus status)
        {
            return GetUpdatedAwardStatusCode(status.Category);
        }

        /// <summary>
        /// Two configuration objects are equal if their office ids and award years are equal
        /// </summary>
        /// <param name="obj">Configuration object to compare to this</param>
        /// <returns>True if the object's office Ids and AwardYears are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var config = obj as FinancialAidConfiguration;

            if (config.OfficeId == this.OfficeId &&
                config.AwardYear == this.AwardYear)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the HashCode of this object based on the officeId and AwardYear
        /// </summary>
        /// <returns>The HashCode of this object</returns>
        public override int GetHashCode()
        {
            return OfficeId.GetHashCode() ^ AwardYear.GetHashCode();
        }

        /// <summary>
        /// Returns the office Id and award year of this object, which identifies it from other 
        /// </summary>
        /// <returns>The OfficeId and awardYear of this object</returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", OfficeId, AwardYear);
        }

    }
}
