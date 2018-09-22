// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Dtos.Finance.AccountDue;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IAccountDueService
    {
        /// <summary>
        /// Get the account due info for a student by term
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account due detail</returns>
        AccountDue GetAccountDue(string studentId);

        /// <summary>
        /// Get the account due info for a student by period (PCF)
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Account due detail</returns>
        AccountDuePeriod GetAccountDuePeriod(string studentId);
    }
}
