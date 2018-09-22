/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The ChecklistItemControlStatus indicates when a student must complete the checklist item 
    /// </summary>
    public enum ChecklistItemControlStatus
    {
        /// <summary>
        /// Removed From Checklist. Indicates this checklist item is not part of the student's checklist
        /// </summary>
        RemovedFromChecklist,

        /// <summary>
        /// Completion Required Later (skipped for now). Indicates the student can skip this check list item for now, but still must
        /// complete the item in order to get Financial Aid.
        /// </summary>
        CompletionRequiredLater,

        /// <summary>
        /// Completion Required. Indicates the student must complete the checklist item before completing any other checklist items.
        /// </summary>
        CompletionRequired
    }
}
