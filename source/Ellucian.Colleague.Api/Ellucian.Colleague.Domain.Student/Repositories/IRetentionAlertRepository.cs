// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for Retention alert
    /// </summary>
    public interface IRetentionAlertRepository
    {
        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <param name="StudentIds">Student Ids</param>
        /// <param name="CaseIds">Case Ids</param>
        /// <returns>Retention Alert Work Case list</returns>
        Task<List<Base.Entities.RetentionAlertWorkCase>> GetRetentionAlertCasesAsync(string advisorId, IEnumerable<string> StudentIds, IEnumerable<string> CaseIds);

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <param name="StudentIds">Student Ids</param>
        /// <param name="CaseIds">Case Ids</param>
        /// <param name="IsIncludeClosedCases">Is Include Closed Cases</param>
        /// <returns>Retention Alert Work Case 2 list</returns>
        Task<List<Base.Entities.RetentionAlertWorkCase2>> GetRetentionAlertCases2Async(string advisorId, IEnumerable<string> StudentIds, IEnumerable<string> CaseIds, IEnumerable<string> roleIds, bool IsIncludeClosedCases);

        /// <summary>
        /// Retrieves retention alert contributions
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <param name="contributionsQueryCriteria"></param>
        /// <returns>Retention alert contributions list</returns>
        Task<List<Base.Entities.RetentionAlertWorkCase>> GetRetentionAlertContributionsAsync(string advisorId, ContributionsQueryCriteria contributionsQueryCriteria);

        /// <summary>
        /// Add retention alert case
        /// </summary>
        /// <param name="retentionAlertCase"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertCaseCreateResponse> AddRetentionAlertCaseAsync(Base.Entities.RetentionAlertCase retentionAlertCase);

        /// <summary>
        /// Update retention alert case
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="retentionAlertCase"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertCaseCreateResponse> UpdateRetentionAlertCaseAsync(string caseId, Base.Entities.RetentionAlertCase retentionAlertCase);

        /// <summary>
        /// Add a note to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertNote"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseNoteAsync(string caseId, Base.Entities.RetentionAlertWorkCaseNote retentionAlertNote);

        /// <summary>
        /// Add a FollowUp to a Retention Alert Case, this will not updat the Case Owner; only add a Note to the Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseNote"></param>
        /// <returns></returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseFollowUpAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertWorkCaseNote);
        
            /// <summary>
        /// Add a communication code to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCommCode"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseCommCodeAsync(string caseId, Base.Entities.RetentionAlertWorkCaseCommCode retentionAlertCaseCommCode);

        /// <summary>
        /// Add a case type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseTypeAsync(string caseId, Base.Entities.RetentionAlertWorkCaseType retentionAlertCaseType);

        /// <summary>
        /// Add a case type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCasePriority"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> ChangeRetentionAlertCasePriorityAsync(string caseId, Base.Entities.RetentionAlertWorkCasePriority retentionAlertCasePriority);


        /// <summary>
        /// Close a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseClose"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> CloseRetentionAlertCaseAsync(string caseId, Base.Entities.RetentionAlertWorkCaseClose retentionAlertCaseClose);
        
        /// <summary>
        /// Gets the retention alert case detail.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        /// <returns>Retention Alert Case Detail</returns>
        Task<Base.Entities.RetentionAlertCaseDetail> GetRetentionAlertCaseDetailAsync(string caseId);

        /// <summary>
        /// Sends a mail for the Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> SendRetentionAlertWorkCaseMailAsync(string caseId, Base.Entities.RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail);

        /// <summary>
        /// Set a Case Reminder Date
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="reminder"></param>
        /// <returns>Retention alert work case action response</returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseReminderAsync(string caseId, Base.Entities.RetentionAlertWorkCaseSetReminder reminder);

        /// <summary>
        /// Manage Retention Alert Case Reminders
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> ManageRetentionAlertCaseRemindersAsync(string caseId, Base.Entities.RetentionAlertWorkCaseManageReminders entity);

        /// <summary>
        /// Retrieves retention alert open cases
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <returns>A list of retention alert open cases</returns>
        Task<List<Base.Entities.RetentionAlertOpenCase>> GetRetentionAlertOpenCasesAsync(string advisorId);

        /// <summary>
        /// Reassigns the retention alert work case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="RetentionAlertWorkCaseReassign"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        Task<Base.Entities.RetentionAlertWorkCaseActionResponse> ReassignRetentionAlertWorkCaseAsync(string caseId, Base.Entities.RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign);

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="caseCategoryIds">Retention Alert Case Category Id</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that category</returns> 
        Task<Base.Entities.RetentionAlertGroupOfCasesSummary> GetRetentionAlertCaseOwnerSummaryAsync(string caseCategoryId);

        /// <summary>
        /// Get the Org Role settings for Case Categories.
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids</param>
        /// <returns>A List of Retention Alert Case Category Roles</returns>
        Task<IEnumerable<Base.Entities.RetentionAlertCaseCategoryOrgRoles>> GetRetentionAlertCaseCategoryOrgRolesAsync(IEnumerable<string> caseCategoryIds);

        /// <summary>
        /// Retrieves retention alert closed cases grouped by closure reason
        /// </summary>
        /// <param name="categoryId">Case Category Id</param>
        /// <returns>A list of retention alert open cases</returns>
        Task<List<Base.Entities.RetentionAlertClosedCasesByReason>> GetRetentionAlertClosedCasesByReasonAsync(string categoryId);

        /// <summary>
        /// Set the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <param name="sendEmailPreference"></param>
        /// <returns>Retention Alert Send Email Preference</returns>
        Task<RetentionAlertSendEmailPreference> SetRetentionAlertEmailPreferenceAsync(string orgEntityId, RetentionAlertSendEmailPreference sendEmailPreference);

        /// <summary>
        /// Get the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <returns>Retention Alert Send Email Preference</returns>
        Task<RetentionAlertSendEmailPreference> GetRetentionAlertEmailPreferenceAsync(string orgEntityId);

        /// <summary>
        /// Retrieves List of matching student IDs
        /// </summary>
        /// <param name="lastName">Last Name</param>
        /// <param name="firstName">First Name</param>
        /// <param name="middleName">Middle Name</param>
        /// <returns>Student ID list</returns>
        Task<IEnumerable<string>> SearchStudentsByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null);
    }
}
