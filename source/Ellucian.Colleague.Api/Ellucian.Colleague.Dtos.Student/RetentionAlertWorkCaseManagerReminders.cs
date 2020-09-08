﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work the case action
    /// </summary>
    public class RetentionAlertWorkCaseManageReminders
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// List of Case Item Ids to clear the Reminder Dates from.
        /// </summary>
        public IEnumerable<RetentionAlertWorkCaseManageReminder> Reminders { get; set; }
    }
}
