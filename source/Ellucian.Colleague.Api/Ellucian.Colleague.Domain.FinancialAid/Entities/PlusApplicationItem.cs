//Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// A Plus Loan Item object containing the associataed year, full name and flag for status
    /// </summary>
    [Serializable]
    public class PlusApplicationItem
    {
        /// <summary>
        /// FA Year Associated with the PlusLoanItem
        /// </summary>
        public string FaYear { get; set; }

        /// <summary>
        /// First name associated with the plus loan item
        /// </summary>
        public string PlusApplicationFirstName { get; set; }

        /// <summary>
        /// Last name associated with the plus loan item
        /// </summary>
        public string PlusApplicationLastName { get; set; }

        /// <summary>
        /// Status associated with PlusLoanItem
        /// True if complete, False if incomplete
        /// </summary>
        public bool IsLoanApplicationItemComplete { get; set; }

        /// <summary>
        /// Flag set to true if application is GPLUS
        /// </summary>
        public bool IsGPLUS { get; set; }
    }
}
