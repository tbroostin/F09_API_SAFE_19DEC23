// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert case history
    /// </summary>
    [Serializable]
    public class RetentionAlertCaseHistory
    {
        /// <summary>
        /// Case history created date
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Case history created time
        /// </summary>
        public DateTime? TimeCreated { get; set; }

        /// <summary>
        /// Case updated by
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Case summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Case detailed note
        /// </summary>
        public IEnumerable<string> DetailedNote { get; set; }

        /// <summary>
        /// Type of the case item
        /// </summary>
        public string CaseItemType { get; set; }

        /// <summary>
        ///  Case Item Id assigned to this work item
        /// </summary>
        public string CaseItemId { get; set; }

        /// <summary>
        /// The contact method
        /// </summary>
        public string ContactMethod { get; set; }

        /// <summary>
        /// Case type.
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Case Closure reason
        /// </summary>
        public string CaseClosureReason { get; set; }

        /// <summary>
        /// Reminder Date
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionAlertCaseHistory"/> class.
        /// </summary>
        public RetentionAlertCaseHistory()
        {
        }
    }
}
