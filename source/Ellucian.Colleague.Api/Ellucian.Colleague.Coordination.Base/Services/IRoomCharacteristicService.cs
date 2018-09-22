// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IRoomCharacteristicService
    {
        Task<IEnumerable<Dtos.RoomCharacteristic>> GetRoomCharacteristicsAsync(bool bypassCache = false);
        Task<Dtos.RoomCharacteristic> GetRoomCharacteristicByGuidAsync(string guid);
    }
}
