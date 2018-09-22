//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for ShippingMethods services
    /// </summary>
    public interface IShippingMethodsService: IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.ShippingMethods>> GetShippingMethodsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.ShippingMethods> GetShippingMethodsByGuidAsync(string id);
    }
}
