using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// placeholder
    /// </summary>
    public class PlusMpnItem
    {
        /// <summary>
        /// Direct PLUS or Graduate PLUS Loan Master Promissory Note (MPN) expiration date
        /// If null, student has no Direct PLUS or GradPLUS loan MPN
        /// </summary>
        public DateTime? PlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public string PlusLoanMpnPersonId { get; set; }
    }
}
