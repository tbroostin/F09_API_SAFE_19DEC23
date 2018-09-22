/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This object is obsolete as of API 1.16 for security reasons
    /// </summary>
    [Obsolete("Obsolete as of API 1.16")]
    public class PayableDepositAccount : BankAccount
    {
        /// <summary>
        /// The unique Id of the payee, either a Person or an Organization
        /// </summary>
        public string PayeeId { get; set; }

        /// <summary>
        /// Indicates whether this account has been verified by the business office. "Verified" is also known as "prenoted" in back-office parlance
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// The list of deposit directives for this account
        /// </summary>
        public List<PayableDeposit> PayableDeposits { get; set; }
    }
}
