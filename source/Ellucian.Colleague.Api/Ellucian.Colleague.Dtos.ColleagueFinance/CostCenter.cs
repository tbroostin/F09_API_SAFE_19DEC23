// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Cost Center DTO.
    /// </summary>
    public class CostCenter
    {
        /// <summary>
        /// The cost center ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The part of the ID that corresponds to the unit component.
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// The cost center name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total budget amount for expense accounts for the cost center.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// The total actuals amount for expense accounts for the cost center.
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for expense accounts for the cost center.
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// The total budget amount for revenue accounts for the cost center.
        /// </summary>
        public decimal TotalBudgetRevenue { get; set; }

        /// <summary>
        /// The total actuals amount for revenue accounts for the cost center.
        /// </summary>
        public decimal TotalActualsRevenue { get; set; }

        /// <summary>
        /// The total encumbrances amount for revenue accounts for the cost center.
        /// </summary>
        public decimal TotalEncumbrancesRevenue { get; set; }

        /// <summary>
        /// List of GL account numbers that make up the cost center.
        /// </summary>
        public List<CostCenterSubtotal> CostCenterSubtotals { get; set; }



    }
}

