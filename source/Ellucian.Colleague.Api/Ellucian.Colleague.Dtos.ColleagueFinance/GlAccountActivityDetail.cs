// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The GL account activity detail DTO.
    /// </summary>
    public class GlAccountActivityDetail
    {
        /// <summary>
        /// The GL account.
        /// </summary>
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// The GL account formatted for display.
        /// </summary>
        public string FormattedGlAccount { get; set; }

        /// <summary>
        /// The GL account description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The cost center ID.
        /// </summary>
        public string CostCenterId { get; set; }

        /// <summary>
        /// The part of the cost center ID that corresponds to the unit component.
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// The cost center name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total for the budget activity for this GL account.
        /// </summary>
        public decimal Budget { get; set; }

        /// <summary>
        /// This is the total memo budget, budget pending posting, for the GL account.
        /// </summary>
        public decimal MemoBudget { get; set; }

        /// <summary>
        /// The total for the encumbrances activity for this GL account.
        /// </summary>
        public decimal Encumbrances { get; set; }

        /// <summary>
        /// The total for the actuals activity for this GL account.
        /// </summary>
        public decimal Actuals { get; set; }

        /// <summary>
        /// This is the total memo actuals, actuals pending posting, for the GL account.
        /// </summary>
        public decimal MemoActuals { get; set; }

        /// <summary>
        /// The estimated opening balance for a GL account.
        /// </summary>
        public decimal EstimatedOpeningBalance { get; set; }

        /// <summary>
        /// The closing year amount for a GL account.
        /// </summary>
        public decimal ClosingYearAmount { get; set; }

        /// <summary>
        /// Set of budget activity detail for the GL account.
        /// </summary>
        public List<GlTransaction> BudgetTransactions { get; set; }

        /// <summary>
        /// Set of actuals activity detail for the GL account.
        /// </summary>
        public List<GlTransaction> ActualsTransactions { get; set; }

        /// <summary>
        /// Set of encumbrance activity detail for the GL account.
        /// </summary>
        public List<GlTransaction> EncumbranceTransactions { get; set; }

        /// <summary>
        /// Boolean flag indicating if justification note indicator should be shown for the GL Account.
        /// </summary>
        public bool ShowJustificationNotes { get; set; }

        /// <summary>
        /// The justification notes for the GL Account.
        /// </summary>
        public string JustificationNotes { get; set; }
    }
}
