// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the AP type service.
    /// </summary>
    public interface IAccountsPayableTypeService
    {
        /// <summary>
        /// Returns a list of AP type codes.
        /// </summary>
        /// <returns>A list of AP type codes.</returns>
        Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypesAsync();
    }
}