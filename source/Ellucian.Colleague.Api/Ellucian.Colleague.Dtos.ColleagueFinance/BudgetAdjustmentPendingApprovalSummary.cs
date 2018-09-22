// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Summary information to list budget adjustments pending approval for a user.
    /// </summary>
    public class BudgetAdjustmentPendingApprovalSummary
    {
        /// <summary>
        /// The reason for the budget adjustment.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The budget adjustment initiator name.
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// Budget adjustment number.
        /// </summary>
        public string BudgetAdjustmentNumber { get; set; }

        /// <summary>
        /// The transaction date in the adjustment. It can be null for a draft.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// The total amount of the ToAmounts (credits) in the budget adjustment.
        /// </summary>
        public decimal ToAmount { get; set; }

        /// <summary>
        /// Status of the budget adjustment.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BudgetEntryStatus Status { get; set; }
    }
}