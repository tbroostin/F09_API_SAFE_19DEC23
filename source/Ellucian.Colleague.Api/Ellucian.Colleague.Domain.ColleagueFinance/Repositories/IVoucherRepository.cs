// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a voucher repository.
    /// </summary>
    public interface IVoucherRepository
    {
        /// <summary>
        /// Get a specific voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher ID.</param>
        /// <param name="personId">Person ID of user.</param>
        /// <param name="glAccessLevel">GL Access level of user.</param>
        /// <param name="glAccessAccounts">Set of GL Accounts to which the user has access.</param>
        /// <param name="versionNumber">Voucher API version number.</param>
        /// <returns>Voucher domain entity.</returns>
        Task<Voucher> GetVoucherAsync(string voucherId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts, int versionNumber);

    }
}
