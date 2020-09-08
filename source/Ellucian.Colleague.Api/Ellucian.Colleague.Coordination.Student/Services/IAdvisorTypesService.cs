//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdvisorTypes services
    /// </summary>
    public interface IAdvisorTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdvisorTypes>> GetAdvisorTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AdvisorTypes> GetAdvisorTypesByGuidAsync(string id);
    }
}
