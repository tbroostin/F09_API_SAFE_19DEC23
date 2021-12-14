// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a requisition repository
    /// </summary>
    public interface IFixedAssetsRepository
    {
        /// <summary>
        /// Returns fixed assets.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<FixedAssets>, int>> GetFixedAssetsAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Returns a single fixed asset record.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<FixedAssets> GetFixedAssetByIdAsync(string guid);

       
    }
}
