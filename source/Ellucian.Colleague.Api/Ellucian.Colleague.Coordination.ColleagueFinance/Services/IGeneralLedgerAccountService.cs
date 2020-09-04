// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    public interface IGeneralLedgerAccountService
    {
        /// <summary>
        /// Retrieves a General ledger account DTO.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <returns>General ledger account DTO.</returns>
        Task<Dtos.ColleagueFinance.GeneralLedgerAccount> GetAsync(string generalLedgerAccountId);

        /// <summary>
        /// Retrieves the list of expense GL account DTOs that the user has access to.
        /// </summary>
        /// <param name="glClass">Optional: null for all the user GL accounts, expense for only the expense type GL accounts.</param>
        /// <returns>A collection of expense GL account DTOs for the user.</returns>
        Task<IEnumerable<Dtos.ColleagueFinance.GlAccount>> GetUserGeneralLedgerAccountsAsync(string glClass);

        /// <summary>
        /// Retrieves a GL account validation response DTO.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>GL account validation response DTO.</returns>
        Task<Dtos.ColleagueFinance.GlAccountValidationResponse> ValidateGlAccountAsync(string generalLedgerAccountId, string fiscalYear);

    }
}