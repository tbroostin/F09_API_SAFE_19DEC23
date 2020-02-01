//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidApplicationOutcomes services
    /// </summary>
    public interface IFinancialAidApplicationOutcomeService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome>, int>> GetAsync(int offset, int limit, FinancialAidApplicationOutcome filterDto, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome> GetByIdAsync(string id, bool bypassCache = true);
    }
}