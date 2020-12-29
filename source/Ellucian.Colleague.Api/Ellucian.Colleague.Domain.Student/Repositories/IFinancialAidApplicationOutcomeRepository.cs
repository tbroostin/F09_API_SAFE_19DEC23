// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a FinancialAidApplicationOutcome repository
    /// </summary>
    public interface IFinancialAidApplicationOutcomeRepository
    {
        Task<Fafsa> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<Fafsa>, int>> GetAsync(int offset, int limit, bool bypassCache, string applicantId, string aidYear, string methodology, string applicationId, List<string> faSuiteYears);

        /// <summary>
        /// Get the financial aid application ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The person ID</returns>
        Task<string> GetApplicationIdFromGuidAsync(string guid);
    }
}
