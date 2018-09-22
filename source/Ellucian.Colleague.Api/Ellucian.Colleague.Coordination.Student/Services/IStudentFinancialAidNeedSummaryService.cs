//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentFinancialAidNeedSummary services
    /// </summary>
    public interface IStudentFinancialAidNeedSummaryService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>> GetAsync(int offset, int limit, bool bypassCache);
        Task<Dtos.StudentFinancialAidNeedSummary> GetByIdAsync(string id);
    }
}
