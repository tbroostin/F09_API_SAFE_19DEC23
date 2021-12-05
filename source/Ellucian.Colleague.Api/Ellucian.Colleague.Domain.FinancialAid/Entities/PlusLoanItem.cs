using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An object of PLUS Loan ASLA Data
    /// </summary>
    [Serializable]
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
