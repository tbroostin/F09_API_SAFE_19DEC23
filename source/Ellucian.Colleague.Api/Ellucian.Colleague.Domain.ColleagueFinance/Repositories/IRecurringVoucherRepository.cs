// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a recurring voucher repository.
    /// </summary>
    public interface IRecurringVoucherRepository
    {
        /// <summary>
        /// Get a specific recurring voucher
        /// </summary>
        /// <param name="recurringVoucherId">the requested recurring voucher ID</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="glAccessAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>Recurring voucher domain entity</returns>
        Task<RecurringVoucher> GetRecurringVoucherAsync(string recurringVoucherId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts);
    }
}
