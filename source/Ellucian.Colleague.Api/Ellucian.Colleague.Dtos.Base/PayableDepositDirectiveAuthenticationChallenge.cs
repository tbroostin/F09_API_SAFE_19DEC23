using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Use this object as input to get an authentication token to modify
    /// payable deposit directives.
    /// </summary>
    public class PayableDepositDirectiveAuthenticationChallenge
    {
        /// <summary>
        /// The value you must provide to validate against the account number of the deposit directive 
        /// identified by PayableDepositDirectiveId. The challenge value is not required if the CurrentUser
        /// has no directives assigned to the given address.
        /// </summary>
        public string ChallengeValue { get; set; }
        
        /// <summary>
        /// The AddressId of the payable deposit directives. For employees this value should be null.
        /// For Vendors this property can have a value.
        /// </summary>
        public string AddressId { get; set; }


        /// <summary>
        /// The id of the Payable Deposit Directive to authenticate against. If obtaining authentication
        /// for a new employee or new vendor, this attribute can be null. If the current user already has
        /// deposit directives for the given addressId, this attribute cannot be null.
        /// </summary>
        public string PayableDepositDirectiveId { get; set; }


    }
}
