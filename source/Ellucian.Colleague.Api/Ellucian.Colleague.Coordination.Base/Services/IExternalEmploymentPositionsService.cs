﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for ExternalEmploymentPositions services
    /// </summary>
    public interface IExternalEmploymentPositionsService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentPositions>> GetExternalEmploymentPositionsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.ExternalEmploymentPositions> GetExternalEmploymentPositionsByGuidAsync(string id);
    }
}