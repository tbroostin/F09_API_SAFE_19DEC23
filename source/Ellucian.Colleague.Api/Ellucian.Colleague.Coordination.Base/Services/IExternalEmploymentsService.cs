//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for ExternalEmployments services
    /// </summary>
    public interface IExternalEmploymentsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmployments>, int>> GetExternalEmploymentsAsync(int offset, int limit, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.ExternalEmployments> GetExternalEmploymentsByGuidAsync(string id);
    }
}
