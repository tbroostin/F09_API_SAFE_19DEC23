// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work the case action
    /// </summary>
    public class RetentionAlertWorkCaseManageReminder
    {
        /// <summary>
        /// Case Items Id
        /// </summary>
        public string CaseItemsId { get; set; }

        /// <summary>
        /// Clear Reminder Date (Y/N)
        /// </summary>
        public string ClearReminderDate { get; set; }
    }
}
