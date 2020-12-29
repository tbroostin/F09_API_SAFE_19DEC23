// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a FinancialAidApplications repository
    /// </summary>
    public interface IStudentFinancialAidApplicationRepository
    {
        Task<Fafsa> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<Fafsa>, int>> GetAsync(int offset, int limit, bool bypassCache, string studentId, string aidYear, List<string> faSuiteYears, string methodology = "", string source = "");
        
    }
}
