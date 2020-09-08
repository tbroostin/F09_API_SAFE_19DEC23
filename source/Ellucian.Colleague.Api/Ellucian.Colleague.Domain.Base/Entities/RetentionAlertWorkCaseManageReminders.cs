// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseManageReminders
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// List of Case Items Ids to clear the Reminder Date out of.
        /// </summary>
        public IEnumerable<RetentionAlertWorkCaseManageReminder> Reminders { get; set; }

        public RetentionAlertWorkCaseManageReminders(string updatedBy, IEnumerable<RetentionAlertWorkCaseManageReminder> reminders)
        {
            if (reminders == null )
            {
                throw new ArgumentNullException("reminders");
            }

            UpdatedBy = updatedBy;
            Reminders = reminders;
        }
    }
}
