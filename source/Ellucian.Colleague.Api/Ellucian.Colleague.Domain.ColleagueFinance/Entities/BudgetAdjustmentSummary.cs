// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// The properties for a list of draft and non draft budget adjustments for a user.
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentSummary
    {
        /// <summary>
        /// Justificaton for the adjustment.
        /// </summary>
        public string Reason { get { return reason; } }
        private readonly string reason;

        /// <summary>
        /// Person ID for the user that created or updated this adjustment.
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// Draft budget adjustment ID.
        /// </summary>
        public string DraftBudgetAdjustmentId { get; set; }

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
        /// <param name="reason">Justification for the adjustment.</param>
        /// <param name="personId">Person ID for the user that created or updated this adjustment.</param>
        public BudgetAdjustmentSummary(string reason, string personId)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException("reason", "reason is required");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId is required");
            }
            this.reason = reason;
            this.personId = personId;
        }
    }
}