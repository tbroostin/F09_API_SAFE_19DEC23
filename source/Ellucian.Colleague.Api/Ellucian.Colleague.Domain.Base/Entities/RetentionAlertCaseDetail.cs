// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Attributes of retention alert case detail
    /// </summary>
    [Serializable]
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
        /// Type of this case
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
        public List<RetentionAlertCaseHistory> CaseHistory { get; set; }

        /// <summary>
        /// Case Recipient Email Addresses
        /// </summary>
        public List<RetentionAlertCaseRecipEmail> CaseRecipEmails { get; set; }

        /// <summary>
        /// Case Reassignment Detail list
        /// </summary>
        public List<RetentionAlertCaseReassignmentDetail> CaseReassignmentList { get; set; }

        

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertCaseDetail"/> class.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        public RetentionAlertCaseDetail(string caseId)
        {
            CaseId = caseId;
        }
    }
}
