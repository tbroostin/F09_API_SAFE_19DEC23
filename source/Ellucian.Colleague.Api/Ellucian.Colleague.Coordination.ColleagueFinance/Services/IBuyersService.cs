//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Buyers services
    /// </summary>
    public interface IBuyersService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Buyers>, int>> GetBuyersAsync(int offset, int limit, bool bypassCache);

        Task<Ellucian.Colleague.Dtos.Buyers> GetBuyersByGuidAsync(string id);
    }
}
