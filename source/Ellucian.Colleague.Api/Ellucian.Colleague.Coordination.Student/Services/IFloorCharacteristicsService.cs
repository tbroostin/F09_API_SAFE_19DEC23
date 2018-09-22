//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FloorCharacteristics services
    /// </summary>
    public interface IFloorCharacteristicsService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.FloorCharacteristics>> GetFloorCharacteristicsAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.FloorCharacteristics> GetFloorCharacteristicsByGuidAsync(string id);
    }
}
