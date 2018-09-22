//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for HousingResidentTypes services
    /// </summary>
    public interface IHousingResidentTypesService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.HousingResidentTypes>> GetHousingResidentTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.HousingResidentTypes> GetHousingResidentTypesByGuidAsync(string id);
    }
}
