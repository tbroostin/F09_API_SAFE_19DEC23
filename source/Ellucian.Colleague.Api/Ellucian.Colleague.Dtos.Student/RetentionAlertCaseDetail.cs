// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of retention alert case detail
    /// </summary>
    public class RetentionAlertCaseDetail
    {
        /// <summary>
        ///  Case Id assigned to this case
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Student Id assigned to this case
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Status of this case
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Created by of this case
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Case owner of this case
        /// </summary>
        public string CaseOwner { get; set; }

        /// <summary>
        /// Type of the case
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Case type codes of this case
        /// </summary>
        public string CaseTypeCodes { get; set; }

        /// <summary>
        /// Category Name of this case
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Category Id of this case
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// Priority of this case
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Priority Code of this case
        /// </summary>
        public string CasePriorityCode { get; set; }

        /// <summary>
        /// Case history of this case
        /// </summary>
        public IEnumerable<RetentionAlertCaseHistory> CaseHistory { get; set; }

        /// <summary>
        /// Case Recip Emails
        /// </summary>
        public IEnumerable<RetentionAlertCaseRecipEmail> CaseRecipEmails { get; set; }

        /// <summary>
        /// Case Reassignment Detail List
        /// </summary>
        public IEnumerable<RetentionAlertCaseReassignmentDetail> CaseReassignmentList { get; set; }
    }
}
