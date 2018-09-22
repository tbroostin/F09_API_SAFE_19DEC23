/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
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
    public class PayrollDeposit
    {
        /// <summary>
        /// The priority of this deposit. Multiple deposits are ordered by priority number
        /// and money is deposited in this priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The bank name used at the time this deposit was active. Only populated if this deposit has an end date and a 
        /// prior bank description is found on or before the end date.
        /// </summary>
        public string HistoricalBankName { get; set; }

        /// <summary>
        /// The amount of the employee's paycheck to deposit. If null, the remainder of the paycheck
        /// will be deposited into the account.
        /// </summary>
        public decimal? DepositAmount { get; set; }

        /// <summary>
        /// The date this deposit should become active.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date this deposit should become inactive
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The Date and Time this deposit was added. When updating PayrollDeposits, this property is used as part of the unique identifier. Changing it would
        /// delete the PayrollDeposit that this object originated from, and create a new PayrollDeposit. It is recommend that the AddDateTime is not changed for
        /// existing deposits in order to maintain proper traceability.
        /// If the deposit is new, this will be determined automatically. 
        /// Otherwise, this property should always be the original add date and time.
        /// </summary>
        public DateTimeOffset? AddDateTime { get; set; }

        /// <summary>
        /// The user who added the deposit. If the deposit is new, this will be determined automatically. Otherwise,
        /// this property is always the original add operator regardless of the value on input.
        /// </summary>
        public string AddOperator { get; set; }

        /// <summary>
        /// The datetime this deposit was last changed. If the deposit is new or contains an update, this will be determined
        /// automatically. Otherwise, this is always returned as the last change date and time regardless of the value on 
        /// input. Updates will be rejected if this timestamp differs from the timestamp in the database - the Payroll Office likely made a change
        /// to the resource.
        /// </summary>
        public DateTimeOffset? ChangeDateTime { get; set; }

        /// <summary>
        /// The user who changed this deposit. If the deposit is new or contains an update, this will be determined
        /// automatically. Otherwise, this is always returned as the last change operator regardless of the value on 
        /// input.
        /// </summary>
        public string ChangeOperator { get; set; }

    }
}
