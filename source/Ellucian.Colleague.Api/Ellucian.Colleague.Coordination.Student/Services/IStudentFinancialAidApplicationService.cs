//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidApplications services
    /// </summary>
    public interface IStudentFinancialAidApplicationService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int>> GetAsync(int offset, int limit, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.FinancialAidApplication> GetByIdAsync(string id);
    }
}
