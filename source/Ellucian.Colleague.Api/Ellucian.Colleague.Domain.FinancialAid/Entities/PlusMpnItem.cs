using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An object of PLUS Loan MPN data
    /// </summary>
    [Serializable]
    public class PlusMpnItem
    {
        /// <summary>
        /// Direct PLUS or Graduate PLUS Loan Master Promissory Note (MPN) expiration date
        /// If null, student has no Direct PLUS or GradPLUS loan MPN
        /// </summary>
        public DateTime? PlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// The person ID retrieved from the Cod Person record
        /// </summary>
        public string PlusLoanMpnPersonId { get; set; }
    }
}
