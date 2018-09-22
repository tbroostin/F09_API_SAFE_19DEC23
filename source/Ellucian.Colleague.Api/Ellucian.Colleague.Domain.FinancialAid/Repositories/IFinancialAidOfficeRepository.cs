//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interfaces exposes Financial Aid Offices
    /// </summary>
    public interface IFinancialAidOfficeRepository
    {
        /// <summary>
        /// Get a list of all Financial Aid Office objects
        /// </summary>
        /// <returns>A list of Financial Aid Office objects</returns>
        Task<IEnumerable<FinancialAidOffice>> GetFinancialAidOfficesAsync();

        /// <summary>
        /// Get a collection of financial aid offices
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid offices</returns>
        Task<IEnumerable<FinancialAidOfficeItem>> GetFinancialAidOfficesAsync(bool ignoreCache);

    }
}
