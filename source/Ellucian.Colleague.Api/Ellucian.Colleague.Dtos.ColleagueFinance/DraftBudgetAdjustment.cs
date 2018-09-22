// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Describes a draft budget adjustment
    /// </summary>
    public class DraftBudgetAdjustment
    {
        /// <summary>
        /// Unique identifier for the draft budget adjustment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Effective date of the draft budget adjustment.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Person initiating the adjustment. May not be the same as the person creating it.
        /// </summary>
        public string Initiator { get; set; }

        /// <summary>
        /// Justificaton for the adjustment.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Additional comments for the adjustment.
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
    }
}