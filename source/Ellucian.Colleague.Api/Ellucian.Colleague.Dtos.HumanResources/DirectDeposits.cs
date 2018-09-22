/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    ///  This object is obsolete as of API 1.16 for security reasons
    /// </summary>
    [Obsolete("Obsolete as of API 1.16")]
    public class DirectDeposits
    {
        /// <summary>
        /// The Id of the employee to whom this information belongs
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// The list of the employee's Bank Accounts used by the Payroll office in which paycheck money is deposited.
        /// This includes past, active and future accounts. Not all bank accounts are active at the same time.
        /// </summary>
        public List<PayrollDepositAccount> PayrollDepositAccounts { get; set; }
    }
}
