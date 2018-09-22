//Copyright 2013-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Provides read-only access to fundamental Financial Aid data
    /// </summary>
    public interface IFinancialAidFundRepository
    {
        /// <summary>
        /// Get a collection of financial aid funds
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid funds</returns>
        Task<Tuple<IEnumerable<FinancialAidFund>, int>> GetFinancialAidFundsAsync(int offset, int limit, bool bypassCache);

        Task<FinancialAidFund> GetFinancialAidFundByIdAsync(string id);

        /// <summary>
        /// Get a collection of financial aid funds
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid funds</returns>
        Task<IEnumerable<FinancialAidFund>> GetFinancialAidFundsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of financial aid fund financials
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund financials</returns>
        Task<IEnumerable<FinancialAidFundsFinancialProperty>> GetFinancialAidFundFinancialsAsync(string awardId, IEnumerable<string> fundYears, string hostCountry);
    }
}
