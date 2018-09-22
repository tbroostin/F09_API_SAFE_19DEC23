// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a general ledger transaction repository
    /// </summary>
    public interface IGrantsRepository
    {
        Task<Tuple<IEnumerable<ProjectCF>, int>> GetGrantsAsync(int offset, int limit, string reportingSegment = "", string fiscalYearId = "", bool bypassCache = false);
        Task<ProjectCF> GetProjectsAsync(string guid);
        Task<string> GetHostCountryAsync();
        Task<IDictionary<string, string>> GetProjectCFGuidsAsync(string[] projectIds);
        Task<List<string>> GetProjectCFIdsAsync(string[] projectGuids);
    }
}
