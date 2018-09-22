/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A PayrollDepositDirective is an instruction to your Payroll office for how they should distribute your paycheck to
    /// your various bank accounts.
    /// </summary>
    public class PayrollDepositDirective
    {
        /// <summary>
        /// The unique identifier of this record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The person (employee) to whom the record is associated
        /// </summary>     
        public string PersonId { get; set; }

        /// <summary>
        /// The bank routing identifier, populated if this directive is associated to a US bank
        /// </summary>
        public string RoutingId { get; set; }

        /// <summary>
        /// The bank institution identifier, populated if this directive is associated to a CA bank
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// The bank brannch number, populated if this directive is associated to a CA bank
        /// </summary>
        public string BranchNumber { get; set; }

        /// <summary>
        /// The name of the bank associated to this directive
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// The type of bank account
        /// </summary>
        public BankAccountType BankAccountType { get; set; }

        /// <summary>
        /// On GET, this will be null. For security reasons, we never expose the bank account number.
        /// Populate this attribute with the new account id when updating or creating a payroll deposit directive.
        /// </summary>
        public string NewAccountId { get; set; }

        /// <summary>
        /// The last four characters of a bank account identifier. 
        /// </summary>
        public string AccountIdLastFour { get; set; }

        /// <summary>
        /// A user chosen nickname for the associated bank account
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Indicates whether this deposit has been prenoted
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// This deposit's priority in relation to other deposits; a deposit with priority 999 is considered remainer
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The dollar amount allocated to the deposit; it must have a value unless the deposit is remainder
        /// </summary>
        public decimal? DepositAmount { get; set; }

        /// <summary>
        /// The start date for this deposit
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date for this deposit; a null value indicates this deposit will continue indefinitely
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Record timestamp metadata
        /// </summary>
        public Timestamp Timestamp { get; set; }
    }
}
