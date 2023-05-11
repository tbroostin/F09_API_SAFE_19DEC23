// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// A Projects Accounting project DTO 
    /// (this class is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Project Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Project number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Project title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Project status.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectStatus Status { get; set; }

        /// <summary>
        /// The total budget amount for all revenue-type line items on the project.
        /// </summary>
        public decimal TotalBudgetRevenue { get; set; }

        /// <summary>
        /// The total actuals amount for all revenue-type line items on the project.
        /// </summary>
        public decimal TotalActualsRevenue { get; set; }

        /// <summary>
        /// The total encumbrance amount for all revenue-type line items on the project.
        /// </summary>
        public decimal TotalEncumbrancesRevenue { get; set; }

        /// <summary>
        /// The total budget amount for all expense-type line items on the project.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// The total expense amount for all expense-type line items on the project.
        /// This includes the actuals, encumbrances, and requisition amounts.
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// The total actuals amount for all expense-type line items on the project.
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// The total encumbrance amount for all expense-type line items on the project.
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// The total actuals amount for all asset-type line items on the project.
        /// </summary>
        public decimal TotalActualsAssets { get; set; }

        /// <summary>
        /// The total encumbranc amount for all asset-type line items on the project.
        /// </summary>
        public decimal TotalEncumbrancesAssets { get; set; }

        /// <summary>
        /// The total actuals amount for all liability-type line items on the project.
        /// </summary>
        public decimal TotalActualsLiabilities { get; set; }

        /// <summary>
        /// The total encumbrances amount for all liability-type line items on the project.
        /// </summary>
        public decimal TotalEncumbrancesLiabilities { get; set; }

        /// <summary>
        /// The total actuals amount for all fund balance-type line items on the project.
        /// </summary>
        public decimal TotalActualsFundBalance { get; set; }

        /// <summary>
        /// The total encumbrances amount for all fund balance-type line items on the project.
        /// </summary>
        public decimal TotalEncumbrancesFundBalance { get; set; }

        /// <summary>
        /// List of line items for the project.
        /// </summary>
        public List<ProjectLineItem> LineItems { get; set; }

        /// <summary>
        /// List of budget periods for the project.
        /// </summary>
        public List<ProjectBudgetPeriod> BudgetPeriods { get; set; }

        /// <summary>
        /// Attachments Indicator
        /// </summary>
        public bool AttachmentsIndicator { get; set; }
    }
}
