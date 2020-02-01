// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.using System;

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Lists required methods.
    /// </summary>
    public interface IGeneralLedgerAccountRepository
    {
        /// <summary>
        /// Returns descriptions for the supplied general ledger numbers.
        /// </summary>
        /// <param name="generalLedgerAccountIds">Set of GL account ID.</param>
        /// <param name="glAccountStructure">GL account structure used to parse the GL numbers.</param>
        /// <returns>Dictionary of GL account numbers and corresponding descriptions.</returns>
        Task<Dictionary<string, string>> GetGlAccountDescriptionsAsync(IEnumerable<string> generalLedgerAccountIds, GeneralLedgerAccountStructure glAccountStructure);

        /// <summary>
        /// Retrieves a single general ledger account.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="majorComponentStartPositions">List of positions used to format the GL account ID.</param>
        /// <returns>General ledger account domain entity.</returns>
        Task<GeneralLedgerAccount> GetAsync(string generalLedgerAccountId, IEnumerable<string> majorComponentStartPositions);

        /// <summary>
        /// Retrieves a GL account validation response.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns></returns>
        Task<GlAccountValidationResponse> ValidateGlAccountAsync(string generalLedgerAccountId, string fiscalYear);

        /// <summary>
        /// Retrieves all the component description for supplied gl component ids from respective major component description file
        /// </summary>
        /// <param name="generalLedgerComponentKeys"></param>
        /// <param name="glComponentType"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetGlComponentDescriptionsByIdsAndComponentTypeAsync(IEnumerable<string> generalLedgerComponentIds, GeneralLedgerComponentType glComponentType);

        /// <summary>
        /// Retrieves the list of expense GL account DTOs for which the user has access.
        /// </summary>
        /// <param name="glAccounts">All GL accounts for the user, or just the expense type ones.</param>
        /// <param name="glAccountStructure">GL account structure.</param>
        /// <returns>A collection of expense GL account DTOs for the user.</returns>
        Task<IEnumerable<GlAccount>> GetUserGeneralLedgerAccountsAsync(IEnumerable<string> glAccounts, GeneralLedgerAccountStructure glAccountStructure);

    }
}