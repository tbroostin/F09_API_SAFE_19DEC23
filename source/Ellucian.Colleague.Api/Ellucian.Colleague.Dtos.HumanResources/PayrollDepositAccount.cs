using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// This object is obsolete as of API 1.16 for security reasons
    /// </summary>
    [Obsolete("Obsolete as of API 1.16")]
    public class PayrollDepositAccount : BankAccount
    {
        /// <summary>
        /// The list of directives indicating when and how to deposit money to this bank account. During adds or updates, accounts with 
        /// no payroll deposit directives will be ignored.
        /// </summary>
        public List<PayrollDeposit> PayrollDeposits { get; set; }
        
        /// <summary>
        /// Indicates whether this account has been verified by the payroll office. "Verified" may also known as "prenoted" in payroll parlance
        /// </summary>
        public bool IsVerified { get; set; }
    }
}
