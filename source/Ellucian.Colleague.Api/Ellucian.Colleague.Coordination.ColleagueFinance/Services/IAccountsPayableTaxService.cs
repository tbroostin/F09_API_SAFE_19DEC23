// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the AccountsPayable Tax service.
    /// </summary>
    public interface IAccountsPayableTaxService
    {
        /// <summary>
        /// Returns a list of Accounts Payable Tax codes.
        /// </summary>
        /// <returns>A list of Accounts Payable Tax codes.</returns>
        Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxesAsync();
    }
}
