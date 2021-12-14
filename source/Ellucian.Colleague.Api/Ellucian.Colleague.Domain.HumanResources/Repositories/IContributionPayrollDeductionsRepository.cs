/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IContributionPayrollDeductionsRepository
    {
        Task<string> GetKeyFromGuidAsync(string guid);
        Task<Dictionary<string, string>> GetPerbenGuidsCollectionAsync(IEnumerable<string> perbenIds);
        Task<PayrollDeduction> GetContributionPayrollDeductionByGuidAsync(string guid);
        Task<Tuple<IEnumerable<PayrollDeduction>, int>> GetContributionPayrollDeductionsAsync(int offset, 
            int limit, string arrangement = "", bool bypassCache = false);
    }
}