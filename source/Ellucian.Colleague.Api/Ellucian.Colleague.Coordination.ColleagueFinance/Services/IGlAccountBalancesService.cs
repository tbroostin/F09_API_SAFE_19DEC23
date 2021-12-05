// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of methods that must be implemented for a GL account balances service.
    /// </summary>
    public interface IGlAccountBalancesService
    {
        /// <summary>
        /// Returns the GL account balances DTOs for the list of GL accounts and fiscal year.
        /// </summary>
        /// <param name="glAccounts">List of GL accounts.</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>List of GL account balances DTO for the fiscal year.</returns>
        Task<IEnumerable<Dtos.ColleagueFinance.GlAccountBalances>> QueryGlAccountBalancesAsync(List<string> glAccounts, string fiscalYear);
    }
}

