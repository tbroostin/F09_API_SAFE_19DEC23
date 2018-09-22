//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EarningTypes services
    /// </summary>
    public interface IEarningTypesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.EarningTypes>> GetEarningTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EarningTypes> GetEarningTypesByGuidAsync(string id);
    }
}
