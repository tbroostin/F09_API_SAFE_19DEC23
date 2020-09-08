// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseSetReminder
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Reminder Date
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Summary of the Case Item
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Internal/Detailed Notes to add to the Case Item being recorded (added) to the the case
        /// </summary>
        public IEnumerable<string> Notes { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RetentionAlertWorkCaseSetReminder()
        {
            Notes = new List<string>();
        }

        public RetentionAlertWorkCaseSetReminder(string updatedBy, DateTime? reminderDate, string summary, IEnumerable<string> notes)
        {
            if (reminderDate == null)
            {
                throw new ArgumentNullException("reminderDate");
            }
            if (string.IsNullOrEmpty(summary))
            {
                throw new ArgumentNullException("summary");
            }
            if (notes == null)
            {
                notes = new List<string>();
            }
            if (!string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }
            UpdatedBy = updatedBy;
            ReminderDate = reminderDate;
            Summary = summary;
            Notes = notes;
            
        }
    }
}
