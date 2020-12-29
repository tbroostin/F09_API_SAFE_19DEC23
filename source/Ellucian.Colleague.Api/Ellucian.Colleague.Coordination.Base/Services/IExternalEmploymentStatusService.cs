//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for ExternalEmploymentStatuses services
    /// </summary>
    public interface IExternalEmploymentStatusesService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses>> GetExternalEmploymentStatusesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses> GetExternalEmploymentStatusesByGuidAsync(string id);
    }
}
