// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work list item
    /// </summary>
    public class RetentionAlertWorkCase
    {
        /// <summary>
        ///  Case Id assigned to this work item
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Case Item Id assigned to this work item
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
        /// The type of the case ids
        /// </summary>
        public IEnumerable<string> CaseTypeIds { get; set; }

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
        public string LastActionDate { get; set; }

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


    }
}
