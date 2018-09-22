//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interfaces exposes Financial Aid Offices
    /// </summary>
    public interface IStudentFinancialAidOfficeRepository
    {
        /// <summary>
        /// Get a collection of financial aid offices
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid offices</returns>
        Task<IEnumerable<FinancialAidOfficeItem>> GetFinancialAidOfficesAsync(bool ignoreCache = false);
    }
}
