﻿//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for VeteranStatuses services
    /// </summary>
    public interface IVeteranStatusesService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.VeteranStatuses>> GetVeteranStatusesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.VeteranStatuses> GetVeteranStatusesByGuidAsync(string id);
    }
}
