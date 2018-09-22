// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Parameters that control allowed/required business rules for budget adjustments.
    /// </summary>
    [Serializable]
    public class BudgetAdjustmentParameters
    {
        /// <summary>
        /// Same cost center is required.
        /// </summary>
        public bool SameCostCenterRequired { get { return sameCostCenterRequired; } }
        private readonly bool sameCostCenterRequired;

        public bool ApprovalRequired { get { return approvalRequired; } }
        private readonly bool approvalRequired;

        public bool SameCostCenterApprovalRequired { get { return sameCostCenterApprovalRequired; } }
        private readonly bool sameCostCenterApprovalRequired;

        /// <summary>
        /// Initialize the entity
        /// </summary>
        /// <param name="sameCostCenterRequired">Flag indicating that budget adjustments must remain in a single cost center.</param>
        /// <param name="approvalRequired">Flag indicating that budget adjustments require approval.</param>
        /// <param name="sameCostCenterApprovalRequired">Flag indicating that budget adjustments in a single cost center require approval.</param>
        public BudgetAdjustmentParameters(bool sameCostCenterRequired, bool approvalRequired, bool sameCostCenterApprovalRequired)
        {
            this.sameCostCenterRequired = sameCostCenterRequired;
            this.approvalRequired = approvalRequired;
            this.sameCostCenterApprovalRequired = sameCostCenterApprovalRequired;
        }
    }
}