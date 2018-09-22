// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains all of the status values that are available for budget entries.
    /// </summary>
    [Serializable]
    public enum BudgetEntryStatus
    {
        /// <summary>
        /// Draft means the budget entry is still a draft created through self-service.
        /// </summary>
        Draft,

        /// <summary>
        /// Unfinished status means that the budget entry is still unfinished
        /// and it has been created in Colleague.
        /// </summary>
        Unfinished,

        /// <summary>
        /// NotApproved status means that approvals are required for the budget entry, and that
        /// the budget entry does not have sufficient approval signatures to make it complete.
        /// </summary>
        NotApproved,

        /// <summary>
        /// Complete status means that the budget entry amounts have been reflected in the
        /// general ledger.
        /// </summary>
        Complete
    }
}
