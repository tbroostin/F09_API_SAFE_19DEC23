//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for RoomRates services
    /// </summary>
    public interface IRoomRatesService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.RoomRates>> GetRoomRatesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.RoomRates> GetRoomRatesByGuidAsync(string id);
    }
}
