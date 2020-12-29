// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseManageReminder
    {
        /// <summary>
        /// Case Items Id
        /// </summary>
        public string CaseItemsId { get; set; }

        /// <summary>
        /// Clear the reminder date on the case item
        /// </summary>
        public string ClearReminderDateFlag { get; set; }
    }
}
