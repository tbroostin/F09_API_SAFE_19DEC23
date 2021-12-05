// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Interface to the GL account balances repository.
    /// </summary>
    public interface IGlAccountBalancesRepository
    {
        /// <summary>
        /// This method gets the list of gl account balances for a fiscal year.
        /// </summary>
        /// <param name="glAccounts">List of GL accounts in internal format</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <param name="generalLedgerUser">General Ledger User record.</param>
        /// <param name="glAccountStructure">General Ledger Account Structure.</param>
        /// <param name="glClassConfiguration">General Ledger Class Configuration</param>
        /// <returns>Returns the list of GL account balances domain entities for the fiscal year.</returns>
        Task<IEnumerable<GlAccountBalances>> QueryGlAccountBalancesAsync(List<string> glAccounts, string fiscalYear, GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration);
    }
}
