// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Summary information to list draft and non draft budget adjustments for a user.
    /// </summary>
    public class BudgetAdjustmentSummary
    {
        /// <summary>
        /// Draft budget adjustment ID.
        /// </summary>
        public string DraftBudgetAdjustmentId { get; set; }

        /// <summary>
        /// Budget adjustment number.
        /// </summary>
        public string BudgetAdjustmentNumber { get; set; }

        /// <summary>
        /// The transaction date in the adjustment. It can be null for a draft.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// The total amount of the ToAmounts (credits) in the adjustment.
        /// </summary>
        public decimal ToAmount { get; set; }

        /// <summary>
        /// The reason for the adjustment.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Flag to indicate if document has attachment/s associated
        /// </summary>
        public bool AttachmentsIndicator { get; set; }

        /// <summary>
        /// Status of the budget adjustment.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BudgetEntryStatus Status { get; set; }
    }
}