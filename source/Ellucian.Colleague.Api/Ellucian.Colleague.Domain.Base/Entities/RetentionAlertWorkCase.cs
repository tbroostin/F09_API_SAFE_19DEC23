// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert Work list information.
    /// </summary>
    [Serializable]
    public class RetentionAlertWorkCase
    {
        /// <summary>
        ///  Case Id assigned to this work item
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        ///  Case Item Id assigned to this work item
        /// </summary>
        public string CaseItemIds { get; set; }

        /// <summary>
        /// Student Id assigned to this work item
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Category of this work item
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Category description of this work item
        /// </summary>
        public string CategoryDescription { get; set; }

        /// <summary>
        /// Summary of this work item
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Status of this work item
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Status of this work item
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Case owner of this work item
        /// </summary>
        public string CaseOwner { get; set; }
        
        /// <summary>
        /// Case owner ids of this work item
        /// </summary>
        public string CaseOwnerIds { get; set; }

        /// <summary>
        /// Created date of this work item
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// The type of the case.
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// The type of the case Ids.
        /// </summary>
        public List<string> CaseTypeIds { get; set; }

        /// <summary>
        /// Method of contact of the case
        /// </summary>
        public string MethodOfContact { get; set; }

        /// <summary>
        /// Number of days the case is open
        /// </summary>
        public string DaysOpen { get; set; }

        /// <summary>
        /// Last action date of the case
        /// </summary>
        public DateTime? LastActionDate { get; set; }

        /// <summary>
        /// Action count of the case
        /// </summary>
        public string ActionCount { get; set; }

        /// <summary>
        /// Who closed the case
        /// </summary>
        public string ClosedBy { get; set; }

        /// <summary>
        /// Date the case was closed
        /// </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Reminder Notification Date
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        // Defaut constructor
        public RetentionAlertWorkCase()
        {
            CaseTypeIds = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertWorkCase"/> class. 
        /// </summary>
        /// <param name="caseId">Case id</param>
        /// <param name="studentId">student id</param>
        public RetentionAlertWorkCase(string caseId, string studentId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case Id must be specified.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "studentId must be specified.");
            }

            CaseId = caseId;
            StudentId = studentId;
            CaseTypeIds = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the new <see cref="RetentionAlertWorkCase"/> class.
        /// </summary>        /// 
        /// <param name="caseId">Case ID</param>
        /// <param name="status">Case Status</param>
        /// <param name="closedBy">If the case is closed; who closed the case</param>
        /// <param name="closedDate">If the case is closed; the date the case was closed</param>
        public RetentionAlertWorkCase(string caseId, string status, string closedBy, DateTime? closedDate)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException("caseId", "Case Id must be specified.");
            }
            if (!string.IsNullOrEmpty(status) && status == "Closed" && (string.IsNullOrEmpty(closedBy) || closedDate == null))
            {
                throw new ArgumentException("When the Status is Closed then both the ClosedBy and ClosedDate are required.");
            }
            CaseId = caseId;
            Status = status;
            ClosedBy = closedBy;
            ClosedDate = closedDate;
            CaseTypeIds = new List<string>();
        }
    }
}
