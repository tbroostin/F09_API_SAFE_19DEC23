//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for ContributionPayrollDeductions services
    /// </summary>
    public interface IContributionPayrollDeductionsService : IBaseService
    {
        Task<Ellucian.Colleague.Dtos.ContributionPayrollDeductions> GetContributionPayrollDeductionsByGuidAsync(string id);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ContributionPayrollDeductions>, int>> GetContributionPayrollDeductionsAsync(int offset, int limit,
            string arrangement = "", string deductedOn = "", Dictionary<string, string> filterQualifiers = null, bool bypassCache = false);   
    }
}