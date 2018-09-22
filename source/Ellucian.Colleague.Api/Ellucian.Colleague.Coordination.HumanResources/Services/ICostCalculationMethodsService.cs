//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for CostCalculationMethods services
    /// </summary>
    public interface ICostCalculationMethodsService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.CostCalculationMethods>> GetCostCalculationMethodsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.CostCalculationMethods> GetCostCalculationMethodsByGuidAsync(string id);
    }
}
