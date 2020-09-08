//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for HousingResidentTypes services
    /// </summary>
    public interface IHousingResidentTypesService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.HousingResidentTypes>> GetHousingResidentTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.HousingResidentTypes> GetHousingResidentTypesByGuidAsync(string id);
    }
}
