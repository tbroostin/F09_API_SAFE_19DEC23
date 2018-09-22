/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A PayableDepositDirective directs how and when the AccountsPayable or EChecks system should deposit money to a PayableDepositAccount
    /// </summary>
    public class PayableDepositDirective
    {
        /// <summary>
        /// The unique system-generated id of this PayableDepositDirective
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The unique Id of the payee, either a Person or an Organization associated with this PayableDepositDirective
        /// </summary>
        public string PayeeId { get; set; }

        /// <summary>
        /// The Routing Id of the bank associated with this PayableDepositDirective, if it's a United States Bank. The routing Id is 
        /// 9 digits in length and can be verified as "valid" by using a checksum algorithm - a quick internet search will reveal this 
        /// algorithm. Any adds or updates will be rejected if the routing id is invalid.
        /// </summary>
        public string RoutingId { get; set; }

        /// <summary>
        /// The Institution Id of the bank associated with this PayableDepositDirective, if it's a Canadian Bank. Unlike the routing 
        /// Id, there's no checksum algorithm, but the InstitutionId must be 3 characters long. Any adds or updates will be rejected if 
        /// the Institution Id does not meet this criterion.
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// The Branch Number of the bank associated with this PayableDepositDirective, if it's a Canadian Bank. The Branch Number must 
        /// be 5 characters long. Any adds or updates will be rejected if the Branch Number does not meet this criterion.
        /// </summary>
        public string BranchNumber { get; set; }

        /// <summary>
        /// The official name of the bank associated with this PayableDepositDirective
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// The type of the bank account associated with this PayableDepositDirective
        /// </summary>
        public BankAccountType BankAccountType { get; set; }

        /// <summary>
        /// The Bank Account Id of a new PayableDepositDirective or an updated Bank Account Id of an existing but unverified PayableDepositDirective.
        /// </summary>
        public string NewAccountId { get; set; }

        /// <summary>
        /// The Last four characters of Bank Account Id of a new PayableDepositDirective or an updated Bank Account Id of an existing but unverified PayableDepositDirective.
        /// </summary>
        public string AccountIdLastFour { get; set; }

        /// <summary>
        /// The user defined nickname of the bank account associated with this PayableDepositDirective. The length of the nickname is restricted to 50 characters.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Indicates whether this bank account has been verified by the business office. "Verified" is also known as "prenoted" in back-office parlance.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Address Id associated with this PayableDepositDirective
        /// </summary>
        public string AddressId { get; set; }

        /// <summary>
        /// The date this deposit should become active
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date this deposit should become inactive
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this deposit is electronic
        /// </summary>
        public bool IsElectronicPaymentRequested { get; set; }

        /// <summary>
        /// Timestamp for record add and change (read only)
        /// </summary>
        public Timestamp Timestamp { get; set; }
        
    }
}
