// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Finance query summary DTO.
    /// </summary>
    public class FinanceQuery
    {
        /// <summary>
        /// The total budget amount for the filtered gl account list.
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// The total actuals amount for the filtered gl account list.
        /// </summary>
        public decimal TotalActuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for the filtered gl account list.
        /// </summary>
        public decimal TotalEncumbrances { get; set; }

        /// <summary>
        /// The total requisitions amount for the filtered gl account list.
        /// </summary>
        public decimal TotalRequisitions { get; set; }
        
        /// <summary>
        /// list of subtotal entities.
        /// </summary>
        public List<FinanceQuerySubtotal> SubTotals { get; set; }

    }
}

