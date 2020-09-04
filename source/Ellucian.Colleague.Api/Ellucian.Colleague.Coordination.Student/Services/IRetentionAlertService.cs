// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Retention alert related services
    /// </summary>
    public interface IRetentionAlertService
    {
        /// <summary>
        /// Gets the case types asynchronous.
        /// </summary>
        /// <returns>A list of case Types</returns>
        Task<IEnumerable<CaseType>> GetCaseTypesAsync();

        /// <summary>
        /// Gets the case categories asynchronous.
        /// </summary>
        /// <returns>A list of case categories</returns>
        Task<IEnumerable<CaseCategory>> GetCaseCategoriesAsync();

        /// <summary>
        /// Gets the case closure reasons asynchronous.
        /// </summary>
        /// <returns>A list of case closure reasons</returns>
        Task<IEnumerable<CaseClosureReason>> GetCaseClosureReasonsAsync();

        /// <summary>
        /// Gets the case priorities asynchronous.
        /// </summary>
        /// <returns>A list of case priorities</returns>
        Task<IEnumerable<CasePriority>> GetCasePrioritiesAsync();

        /// <summary>
        /// Gets the retention alert permissions asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<RetentionAlertPermissions> GetRetentionAlertPermissionsAsync();

        /// <summary>
        /// Retrieves retention alert contributions
        /// </summary>
        /// <param name="contributionsQueryCriteria">The retention alert contributions query criteria.</param>
        /// <returns>A list of retention alert contributions</returns>
        Task<IEnumerable<RetentionAlertWorkCase>> GetRetentionAlertContributionsAsync(ContributionsQueryCriteria contributionsQueryCriteria);

        /// <summary>
        /// Gets the retention alert cases asynchronous.
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">The retention alert query criteria.</param>
        /// <returns>A list of retention alert work case</returns>
        Task<IEnumerable<RetentionAlertWorkCase>> GetRetentionAlertCasesAsync(RetentionAlertQueryCriteria retentionAlertQueryCriteria);

        /// <summary>
        /// Adds the retention alert case asynchronous.
        /// </summary>
        /// <param name="retentionAlertCase">The retention alert case.</param>
        /// <returns>Retention alert case create response</returns>
        Task<RetentionAlertCaseCreateResponse> AddRetentionAlertCaseAsync(RetentionAlertCase retentionAlertCase);

        /// <summary>
        /// Updates the retention alert case asynchronous.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        /// <param name="retentionAlertCase">The retention alert case.</param>
        /// <returns>Retention alert case create response</returns>
        Task<RetentionAlertCaseCreateResponse> UpdateRetentionAlertCaseAsync(string caseId, RetentionAlertCase retentionAlertCase);

        /// <summary>
        /// Add a Note to a Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertNote">The retention alert case note action.</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseNoteAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertNote);

        /// <summary>
        /// Add a follwoup to a Retention Alert Case, this will not add the user to the case owners list.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseFollowUpAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertCaseNote);

        /// <summary>
        /// Add a Communication Code to a Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCommCode">The retention alert case communication code action.</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseCommCodeAsync(string caseId, RetentionAlertWorkCaseCommCode retentionAlertCaseCommCode);

        /// <summary>
        /// Add a Case Type to a Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType">The retention alert case type action.</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseTypeAsync(string caseId, RetentionAlertWorkCaseType retentionAlertCaseType);

        /// <summary>
        /// Change the priority of a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCasePriority">The retention alert case priority action.</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> ChangeRetentionAlertCasePriorityAsync(string caseId, RetentionAlertWorkCasePriority retentionAlertCasePriority);

        /// <summary>
        /// Close a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseClose">The retention alert case close action.</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> CloseRetentionAlertCaseAsync(string caseId, RetentionAlertWorkCaseClose retentionAlertCaseClose);

        /// <summary>
        /// Gets the retention alert case detail.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        /// <returns>Retention alert case detail</returns>
        Task<RetentionAlertCaseDetail> GetRetentionAlertCaseDetailAsync(string caseId);

        /// <summary>
        /// Sends a mail for the Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail">The retention alert case send mail</param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> SendRetentionAlertWorkCaseMailAsync(string caseId, RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail);

        /// <summary>
        /// Set a Case Reminder Date
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="reminder"></param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseReminderAsync(string caseId, RetentionAlertWorkCaseSetReminder reminder);


        /// <summary>
        /// Retrieves retention alert open cases
        /// </summary>
        /// <returns>A list of retention alert open cases</returns>
        Task<IEnumerable<RetentionAlertOpenCase>> GetRetentionAlertOpenCasesAsync();


        /// <summary>
        /// Reassign the retention alert work case
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="retentionAlertWorkCaseReassign"></param>
        /// <returns>Retention alert work case action response</returns>
        Task<RetentionAlertWorkCaseActionResponse> ReassignRetentionAlertWorkCaseAsync(string caseId, RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign);

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="caseCategoryId">Retention Alert Case Category Id.</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that categor</returns>
        Task<RetentionAlertGroupOfCasesSummary> GetRetentionAlertCaseOwnerSummaryAsync(string caseCategoryId);

        /// <summary>
        /// Get the Org Role settings for Case Categories. 
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids</param>
        Task<IEnumerable<RetentionAlertCaseCategoryOrgRoles>> GetRetentionAlertCaseCategoryOrgRolesAsync(List<string> caseCategoryIds);

        /// <summary>
        /// Get the closed retention alert cases grouped by closure reason.
        /// </summary>
        /// <param name="categoryId">Retention Alert Case Category Id</param>
        /// <returns>List of the closed retention alert cases grouped by closure reason</returns>
        Task<IEnumerable<RetentionAlertClosedCasesByReason>> GetRetentionAlertClosedCasesByReasonAsync(string categoryId);

        /// <summary>
        /// Manage Reminders for RetentionAlert Cases
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="reminders"></param>
        /// <returns></returns>
        Task<RetentionAlertWorkCaseActionResponse> ManageRetentionAlertCaseRemindersAsync(string caseId, RetentionAlertWorkCaseManageReminders reminders);

        /// <summary>
        /// Set the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <param name="sendEmailPreference"></param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        Task<RetentionAlertSendEmailPreference> SetRetentionAlertEmailPreferenceAsync(string orgEntityId, RetentionAlertSendEmailPreference sendEmailPreference);

        /// <summary>
        /// Get the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        Task<RetentionAlertSendEmailPreference> GetRetentionAlertEmailPreferenceAsync(string orgEntityId);
    }

}
