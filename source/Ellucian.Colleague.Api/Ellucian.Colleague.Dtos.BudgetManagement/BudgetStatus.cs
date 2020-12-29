// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// This enumeration contains all of the status values that are available for a budget.
    /// </summary>
    [Serializable]
    public enum BudgetStatus
    {
        /// <summary>
        /// This is a new budget that has never been generated.
        /// </summary>
        New,

        /// <summary>
        /// This is a budget that was in the generation process but did not complete.
        /// Some error happened and the generation process aborted before completion.
        /// </summary>
        Generated,

        /// <summary>
        /// This is a budget that has been posted to the General Ledger.
        /// </summary>
        Posted,

        /// <summary>
        /// This is a budget that is in the development process. It may be generated
        /// more times before it is posted.
        /// </summary>
        Working
    }
}
