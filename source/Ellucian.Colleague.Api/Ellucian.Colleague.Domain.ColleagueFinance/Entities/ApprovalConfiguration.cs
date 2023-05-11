// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Contains the configuration parameters for approvals.
    /// </summary>
    [Serializable]
    public class ApprovalConfiguration
    {
        /// <summary>
        /// Do budget entries use approval roles.
        /// </summary>
        public bool BudgetEntriesUseApprovalRoles { get; set; }

        /// <summary>
        /// Do journal entries use approval roles.
        /// </summary>
        public bool JournalEntriesUseApprovalRoles { get; set; }

        /// <summary>
        /// Do requisitions use approval roles.
        /// </summary>
        public bool RequisitionsUseApprovalRoles { get; set; }

        /// <summary>
        /// Do blanket POs use approval roles.
        /// </summary>
        public bool BlanketPurchaseOrdersUseApprovalRoles { get; set; }

        /// <summary>
        /// Do purchase orders use approval roles.
        /// </summary>
        public bool PurchaseOrdersUseApprovalRoles { get; set; }

        /// <summary>
        /// Do recurring vouchers use approval roles.
        /// </summary>
        public bool RecurringVouchersUseApprovalRoles { get; set; }

        /// <summary>
        /// Do vouchers use approval roles.
        /// </summary>
        public bool VouchersUseApprovalRoles { get; set; }

        /// <summary>
        /// Do AR vouchers use approval roles.
        /// </summary>
        public bool ArVouchersUseApprovalRoles { get; set; }

        /// <summary>
        /// Initialize the approval configuration object.
        /// </summary>
        public ApprovalConfiguration()
        {
        }
    }
}