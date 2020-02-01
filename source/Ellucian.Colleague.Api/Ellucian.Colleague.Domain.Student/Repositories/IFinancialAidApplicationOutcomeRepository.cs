// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

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
        Task<Tuple<IEnumerable<Fafsa>, int>> GetAsync(int offset, int limit, bool bypassCache, string applicantId, string aidYear, List<string> faSuiteYears);        
    }
}
