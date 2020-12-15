// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.BudgetManagement
{
    [Serializable]
    public static class BudgetManagementPermissionCodes
    {

        // View any BudgetCode
        public const string ViewBudgetCode = "VIEW.BUDGET.CODES";

        // View any BudgetPhase
        public const string ViewBudgetPhase = "VIEW.BUDGET.PHASES";

        //View any BudgetPhaseLineItems
        public const string ViewBudgetPhaseLineItems = "VIEW.BUDGET.PHASE.LINE.ITEMS";
    }
}
