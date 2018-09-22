// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IRoomTypesService : IBaseService
    {
        Task<IEnumerable<RoomTypes>> GetRoomTypesAsync(bool bypassCache);
        Task<RoomTypes> GetRoomTypesByGuidAsync(string guid);
    }
}
