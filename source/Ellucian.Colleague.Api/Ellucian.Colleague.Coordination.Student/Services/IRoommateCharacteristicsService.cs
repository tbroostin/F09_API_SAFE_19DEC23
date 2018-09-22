//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for RoommateCharacteristics services
    /// </summary>
    public interface IRoommateCharacteristicsService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.RoommateCharacteristics> GetRoommateCharacteristicsByGuidAsync(string id);
    }
}
