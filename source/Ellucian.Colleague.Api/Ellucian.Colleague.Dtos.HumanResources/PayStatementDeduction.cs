/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// A deduction item for a tax, benefit, or other deduction
    /// </summary>
    public class PayStatementDeduction
    {
        /// <summary>
        /// The Deduction code.
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The description of the deduction
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The type of deduction
        /// </summary>
        public PayStatementDeductionType Type { get; set; }
        /// <summary>
        /// The amount deducted from the employee's earnings for this deduction line item for the period
        /// </summary>
        public decimal? EmployeePeriodAmount { get; set; }
        /// <summary>
        /// The amount deducted from the employee's earnings for this deduction line item, year to date.
        /// </summary>
        public decimal? EmployeeYearToDateAmount { get; set; }
        /// <summary>
        /// The amount the employer paid for this deduction line item for the period.
        /// </summary>
        public decimal? EmployerPeriodAmount { get; set; }
        /// <summary>
        /// The amount the employer paid for this deduction line item, year to date
        /// </summary>
        public decimal? EmployerYearToDateAmount { get; set; }
        /// <summary>
        /// The basis amount on which the deduction amount was calculated.
        /// </summary>
        public decimal? ApplicableGrossPeriodAmount { get; set; }
        /// <summary>
        /// The total basis amount on which the deduction amounts are calculated.
        /// </summary>
        public decimal? ApplicableGrossYearToDateAmount { get; set; }
    }
}
