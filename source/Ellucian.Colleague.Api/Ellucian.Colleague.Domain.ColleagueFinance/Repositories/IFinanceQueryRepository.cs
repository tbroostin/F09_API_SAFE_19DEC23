// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of the methods that must be implemented for a Finance query repository.
    /// </summary>
    public interface IFinanceQueryRepository
    {
        /// <summary>
        /// Get a list of GL accounts assigned to the user logged in.
        /// finance query filter criteria is used to filter the GL accounts.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="glAccountStructure">GL Account structure domain entity.</param>
        /// <param name="glClassConfiguration">GL class configuration structure domain entity.</param>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>List of finance query gl account line item entities.</returns>
        Task<IEnumerable<FinanceQueryGlAccountLineItem>> GetGLAccountsListAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, FinanceQueryCriteria criteria, string personId);
    }
}