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
    public class BankAccount
    {
        /// <summary>
        /// The Routing Id of bank, if it's a United States Bank. The routing Id is 9 digits in length and can be verified as "valid" 
        /// by using a checksum algorithm - a quick internet search will reveal this algorithm. Any adds or updates will be rejected if 
        /// the routing id is invalid.
        /// </summary>
        public string RoutingId { get; set; }

        /// <summary>
        /// The Institution Id of the bank, if it's a Canadian Bank. Unlike the routing Id, there's no checksum algorithm, but the Institution
        /// Id must be 3 characters long. Any adds or updates will be rejected if 
        /// the Institution Id does not meet this criterion.
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// The Branch Number of the bank, if it's a Canadian Bank. The Branch Number must be 5 characters long. Any adds or updates will be rejected if 
        /// the Branch Number does not meet this criterion.
        /// </summary>
        public string BranchNumber { get; set; }

        /// <summary>
        /// The official name of the bank
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// The Account Id of the bank account
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// The user defined nickname of the account. The length of the nickname is restricted to 50 characters.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The type of the account
        /// </summary>
        public BankAccountType Type { get; set; }
    }
}
