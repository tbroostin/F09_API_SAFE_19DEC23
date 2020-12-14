//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidFunds services
    /// </summary>
    public interface IFinancialAidFundsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFunds>, int>> GetFinancialAidFundsAsync(int offset, int limit, Dtos.Filters.FinancialAidFundsFilter criteriaFilter, bool bypassCache = false);
        //Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFunds>> GetFinancialAidFundsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.FinancialAidFunds> GetFinancialAidFundsByGuidAsync(string id);
    }
}