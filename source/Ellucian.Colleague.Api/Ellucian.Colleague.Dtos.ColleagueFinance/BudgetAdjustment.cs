// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Describes a budget adjustment.
    /// </summary>
    public class BudgetAdjustment
    {
        /// <summary>
        /// Unique identifier for the budget adjustment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Effective date of the adjustment.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Justificaton for the adjustment.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// ID of the person that created the budget adjustment.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Status of the budget adjustment.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BudgetEntryStatus Status { get; set; }

        /// <summary>
        /// Person initiating the adjustment. May not be the same as the person creating it.
        /// </summary>
        public string Initiator { get; set; }

        /// <summary>
        /// ID of draft budget adjustment that this budget adjustment originated from.
        /// </summary>
        public string DraftBudgetAdjustmentId { get; set; }

        /// <summary>
        /// Whether the draft (from which the budget adjustment was created) could be deleted. If the budget adjustment was not created from a draft, this will always be true.
        /// </summary>
        public bool DraftDeletionSuccessfulOrUnnecessary { get; set; }

        /// <summary>
        /// Additional comments for the adjustment.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// List of approvers for this budget adjustment.
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// List of next approvers for this budget adjustment.
        /// </summary>
        public List<NextApprover> NextApprovers { get; set; }

        /// <summary>
        /// List of objects that describe how much money is being moved from or to each account.
        /// </summary>
        public List<AdjustmentLine> AdjustmentLines { get; set; }
    }
}