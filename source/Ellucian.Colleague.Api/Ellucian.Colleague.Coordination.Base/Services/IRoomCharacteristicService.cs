// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IRoomCharacteristicService : IBaseService
    {
        Task<IEnumerable<Dtos.RoomCharacteristic>> GetRoomCharacteristicsAsync(bool bypassCache = false);
        Task<Dtos.RoomCharacteristic> GetRoomCharacteristicByGuidAsync(string guid);
    }
}
