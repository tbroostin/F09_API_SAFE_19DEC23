//Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A Plus Loan Item object containing the associataed year, full name and flag for status
    /// </summary>
    public class PlusLoanItem
    {
        /// <summary>
        /// FA Year Associated with the PlusLoanItem
        /// </summary>
        public string FaYear { get; set; }

        /// <summary>
        /// First name associated with the plus loan item
        /// </summary>
        public string PlusLoanFirstName { get; set; }

        /// <summary>
        /// Last name associated with the plus loan item
        /// </summary>
        public string PlusLoanLastName { get; set; }

        /// <summary>
        /// Status associated with PlusLoanItem
        /// True if complete, False if incomplete
        /// </summary>
        public bool IsPlusLoanItemComplete { get; set; }
    }
}
