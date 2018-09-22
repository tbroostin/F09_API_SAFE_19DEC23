using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The Token object returned as a response to a successful step up authentication for 
    /// a person's banking information. The Token is required in the header of requests to update
    /// and create Payroll and Payable Deposit Directives (EChecks, Vendor Payments).
    /// </summary>
    public class BankingAuthenticationToken
    {
        /// <summary>
        /// The exact Date and Time that this token expires. After expiration, consumers must reauthorize.
        /// </summary>
        public DateTimeOffset ExpirationDateTimeOffset { get; set; }

        /// <summary>
        /// The token that will accompany a request to update, create or delete banking deposit directives. 
        /// </summary>
        public Guid Token { get; set; }
    }
}
