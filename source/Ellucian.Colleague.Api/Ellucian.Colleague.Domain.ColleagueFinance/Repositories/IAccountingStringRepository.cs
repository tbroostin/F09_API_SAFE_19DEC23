// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of the methods that must be implemented for a Accounting String repository.
    /// </summary>
    public interface IAccountingStringRepository
    {
        /// <summary>
        /// Check if a single accounting code and project number are valid and return it
        /// </summary>
        /// <param name="accountingString">AccountingString domain entity</param>
        /// <returns>Single AccountingString if valid</returns>
        Task<AccountingString> GetValidAccountingString(AccountingString accountingString);

    }
}
