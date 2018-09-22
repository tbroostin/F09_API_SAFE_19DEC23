/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Loan request period DTO
    /// </summary>
    public class LoanRequestPeriod
    {
        /// <summary>
        /// Period code
        /// Required in POST
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Period loan amount
        /// Required in POST. Must be greater than zero.
        /// </summary>
        public int LoanAmount { get; set; }
    }
}
