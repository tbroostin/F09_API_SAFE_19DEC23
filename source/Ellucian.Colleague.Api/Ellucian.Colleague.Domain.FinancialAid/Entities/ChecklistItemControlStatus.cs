/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public enum ChecklistItemControlStatus
    {
        RemovedFromChecklist,
        CompletionRequiredLater,
        CompletionRequired
    }
}
