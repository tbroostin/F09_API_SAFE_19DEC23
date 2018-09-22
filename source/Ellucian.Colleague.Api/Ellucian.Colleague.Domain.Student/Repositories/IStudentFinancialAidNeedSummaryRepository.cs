﻿// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a FinancialAidApplicationOutcome repository
    /// </summary>
    public interface IStudentFinancialAidNeedSummaryRepository
    {
        Task<StudentNeedSummary> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<StudentNeedSummary>, int>> GetAsync(int offset, int limit, bool bypassCache, List<string> faSuiteYears);

        Task<string> GetIsirCalcResultsGuidFromIdAsync(string id);
    }
}
