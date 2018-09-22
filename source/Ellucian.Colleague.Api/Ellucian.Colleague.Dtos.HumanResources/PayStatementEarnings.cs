/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Represents an Earnings line item on the Pay Statement
    /// </summary>
    public class PayStatementEarnings
    {
        /// <summary>
        /// The Id of the EarningsType
        /// </summary>
        public string EarningsTypeId { get; set; }

        /// <summary>
        /// The description of the EarningsType
        /// </summary>
        public string EarningsTypeDescription { get; set; }

        /// <summary>
        /// The number of hours or units worked. If employee is salaried, this will be null
        /// </summary>
        public decimal? UnitsWorked { get; set; }

        /// <summary>
        /// The rate at which the employee is paid for this earnings type
        /// </summary>
        public decimal? Rate { get; set; }

        /// <summary>
        /// The amount the employee was paid for this earnings type for this pay period
        /// </summary>
        public decimal? PeriodPaymentAmount { get; set; }

        /// <summary>
        /// The year to date amount the employee was paid for this earnings type
        /// </summary>
        public decimal YearToDatePaymentAmount { get; set; }
    }
}
