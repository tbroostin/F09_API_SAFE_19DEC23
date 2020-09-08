//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for RoommateCharacteristics services
    /// </summary>
    public interface IRoommateCharacteristicsService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.RoommateCharacteristics> GetRoommateCharacteristicsByGuidAsync(string id);
    }
}
