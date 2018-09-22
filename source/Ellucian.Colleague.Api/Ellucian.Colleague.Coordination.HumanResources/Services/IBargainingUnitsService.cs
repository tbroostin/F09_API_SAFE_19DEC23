//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for Bargaining Units services
    /// </summary>
    public interface IBargainingUnitsService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.BargainingUnit>> GetBargainingUnitsAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.BargainingUnit> GetBargainingUnitsByGuidAsync(string id);
    }
}
