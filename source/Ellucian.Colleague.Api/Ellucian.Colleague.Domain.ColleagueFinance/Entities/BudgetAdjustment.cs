// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a budget adjustment.
    /// </summary>
    [Serializable]
    public class BudgetAdjustment
    {
        /// <summary>
        /// Unique identifier of this adjustment.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Effective date the adjustment.
        /// </summary>
        public DateTime TransactionDate { get { return transactionDate; } }
        private readonly DateTime transactionDate;

        /// <summary>
        /// Justificaton for the adjustment.
        /// </summary>
        public string Reason { get { return reason; } }
        private readonly string reason;

        /// <summary>
        /// ID of the person submitting the budget adjustment.
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// The status of the budget adjustment.
        /// </summary>
        public BudgetEntryStatus Status { get; set; }

        /// <summary>
        /// Person initiating the adjustment. May not be the same as the person creating it.
        /// </summary>
        public string Initiator { get; set; }
        
        /// <summary>
        /// ID of a draft budget adjustment that this budget adjustment originated from.
        /// </summary>
        public string DraftBudgetAdjustmentId { get; set; }

        /// <summary>
        /// Whether the draft (from which the budget adjustment was created) could be deleted. True, unless there was an error.
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
        public ReadOnlyCollection<AdjustmentLine> AdjustmentLines { get; private set; }
        private readonly List<AdjustmentLine> adjustmentLines = new List<AdjustmentLine>();

        /// <summary>
        /// Describes why the adjustment could not be created or posted.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Initialize the budget adjustment to be created.
        /// </summary>
        /// <param name="transactionDate">Effective date of the adjustment.</param>
        /// <param name="reason">Justification for the adjustment.</param>
        /// <param name="adjustmentLines">Objects describing where money is being moved from/to.</param>
        public BudgetAdjustment(DateTime transactionDate, string reason, string personId, IEnumerable<AdjustmentLine> adjustmentLines)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException("reason", "reason is required");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId is required");
            }

            if (adjustmentLines == null)
            {
                throw new ArgumentNullException("adjustmentLines", "adjustmentLines is required.");
            }

            if (adjustmentLines.Count() < 2)
            {
                throw new ArgumentException("adjustmentLines must have at least two lines", "adjustmentLines");
            }

            // Check to see if the transaction balances.
            if (adjustmentLines.Sum(x => x.FromAmount) != adjustmentLines.Sum(x => x.ToAmount))
            {
                throw new ArgumentException("The transaction must balance.");
            }

            // Check for dupliate GL numbers.
            var uniqueGlAccountsList = adjustmentLines.Select(x => x.GlNumber).Distinct().ToList();
            if (uniqueGlAccountsList.Count != adjustmentLines.Count())
            {
                throw new ArgumentException("A GL account may only appear once in a transaction.");
            }

            this.id = "";
            this.transactionDate = transactionDate;
            this.reason = reason;
            this.personId = personId;
            this.DraftDeletionSuccessfulOrUnnecessary = true;
            this.ErrorMessages = new List<string>();
            this.Approvers = new List<Approver>();
            this.NextApprovers = new List<NextApprover>();
            this.adjustmentLines = adjustmentLines.ToList();
            AdjustmentLines = this.adjustmentLines.AsReadOnly();
        }

        /// <summary>
        /// Initialize a newly created and posted budget adjustment.
        /// </summary>
        /// <param name="id">ID of the adjustment.</param>
        /// <param name="transactionDate">Effective date of the adjustment.</param>
        /// <param name="reason">Justification for the adjustment.</param>
        /// <param name="adjustmentLines">Objects describing where money is being moved from/to.</param>
        public BudgetAdjustment(string id, DateTime transactionDate, string reason, string personId, IEnumerable<AdjustmentLine> adjustmentLines)
            :this(transactionDate, reason, personId, adjustmentLines)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required");
            }

            this.id = id;
            this.DraftDeletionSuccessfulOrUnnecessary = true;
            this.Approvers = new List<Approver>();
            this.NextApprovers = new List<NextApprover>();
        }

        /// <summary>
        /// Initialize an existing budget adjustment.
        /// </summary>
        /// <param name="id">ID of the adjustment.</param>
        public BudgetAdjustment(string id, string reason, DateTime transactionDate, string personId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id is required");
            }

            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException("reason", "reason is required");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId is required");
            }

            this.id = id;
            this.reason = reason;
            this.transactionDate = transactionDate;
            this.personId = personId;
            this.DraftDeletionSuccessfulOrUnnecessary = true;
            this.adjustmentLines = new List<AdjustmentLine>();
            this.Approvers = new List<Approver>();
            this.NextApprovers = new List<NextApprover>();
            AdjustmentLines = this.adjustmentLines.AsReadOnly();
        }

        /// <summary>
        /// Add an adjustment line to the list of adjustment lines in the budget adjustment.
        /// </summary>
        /// <param name="adjustmentLine">An adjustment line.</param>
        public void AddAdjustmentLine(AdjustmentLine adjustmentLine)
        {
            if (adjustmentLine != null)
            {
                this.adjustmentLines.Add(adjustmentLine);
            }
        }
    }
}