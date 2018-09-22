// Copyright 2017 Ellucian Company L.P. and its affiliates.using System;

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
    }
}