// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Summary information for a budget adjustment pending approval for a user.
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentPendingApprovalSummary
    {
        /// <summary>
        /// Justification for the adjustment.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The budget adjustment initiator person ID.
        /// For budget adjustments created in UI it will contain
        /// the person ID for the InitiatorLoginId.
        /// </summary>
        public string InitiatorId { get; set; }

        /// <summary>
        /// The budget adjustment initiator login ID.
        /// For budget adjustments created in UI.
        /// </summary>
        public string InitiatorLoginId { get; set; }

        /// <summary>
        /// The budget adjustment initiator name.
        /// </summary>
        public string InitiatorName { get; set; }

        /// <summary>
        /// Budget adjustment number.
        /// </summary>
        public string BudgetAdjustmentNumber { get; set; }

        /// <summary>
        /// The transaction date in the adjustment.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// The total amount of the ToAmounts (credits) in the adjustment.
        /// </summary>
        public decimal ToAmount { get; set; }

        /// <summary>
        /// The status of the budget adjustment.
        /// </summary>
        public BudgetEntryStatus Status { get; set; }

        /// <summary>
        /// Initialize the budget adjustment summary to be created.
        /// </summary>
        public BudgetAdjustmentPendingApprovalSummary()
        {

        }
    }
}