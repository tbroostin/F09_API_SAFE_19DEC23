//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for ShipToDestinations services
    /// </summary>
    public interface IShipToDestinationsService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.ShipToDestinations>> GetShipToDestinationsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.ShipToDestinations> GetShipToDestinationsByGuidAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ShipToCode>> GetShipToCodesAsync();

    }
}
