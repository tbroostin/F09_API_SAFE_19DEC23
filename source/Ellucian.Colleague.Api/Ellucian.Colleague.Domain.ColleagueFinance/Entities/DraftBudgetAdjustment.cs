// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a draft budget adjustment.
    /// </summary>
    [Serializable]
    public class DraftBudgetAdjustment
    {
        /// <summary>
        /// Unique identifier of this draft adjustment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Justificaton for the draft adjustment.
        /// </summary>
        public string Reason { get { return reason; } }
        private readonly string reason;

        /// <summary>
        /// Return the person ID for the creator of the draft adjustment.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Effective date for the draft adjustment.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Person initiating the draft adjustment. May not be the same as the person creating it.
        /// </summary>
        public string Initiator { get; set; }

        /// <summary>
        /// Additional comments for the draft adjustment.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// List of next approvers for this draft budget adjustment.
        /// </summary>
        public List<NextApprover> NextApprovers { get; set; }

        /// <summary>
        /// List of objects that describe how much money is being moved from or to each account.
        /// </summary>
        public List<DraftAdjustmentLine> AdjustmentLines { get; set; }

        /// <summary>
        /// Describes why the draft adjustment could not be created.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Initialize the budget adjustment to be created.
        /// </summary>
        /// <param name="reason">Justification for the adjustment.</param>
        public DraftBudgetAdjustment(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException("reason", "reason is required");
            }

            this.reason = reason;
            this.ErrorMessages = new List<string>();
            this.NextApprovers = new List<NextApprover>();
            this.AdjustmentLines = new List<DraftAdjustmentLine>();

        }
    }
}