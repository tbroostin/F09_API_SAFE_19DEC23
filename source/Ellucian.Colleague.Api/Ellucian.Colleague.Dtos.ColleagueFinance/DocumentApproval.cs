// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Document approval DTO.
    /// </summary>
    public class DocumentApproval
    {
        /// <summary>
        /// The list of documents that require a user's approval.
        /// </summary>
        public List<ApprovalDocument> ApprovalDocuments { get; set; }

        /// <summary>
        /// Indicates whether the user can override an over-budget condition.
        /// </summary>
        public bool CanOverrideFundsAvailability { get; set; }

        /// <summary>
        /// Indicates whether funds availability is turned on for general ledger accounts or projects.
        /// </summary>
        public bool FundsAvailabilityOn { get; set; }

        /// <summary>
        /// Is Approval Allow Returns is enabled for Procurement documents (true if Enable Approval Returns Flag in APPD is (Y)es)
        /// </summary>
        public bool AllowReturns { get; set; }
    }
}
