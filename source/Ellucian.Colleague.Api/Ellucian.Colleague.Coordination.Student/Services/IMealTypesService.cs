//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for MealTypes services
    /// </summary>
    public interface IMealTypesService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.MealTypes>> GetMealTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.MealTypes> GetMealTypesByGuidAsync(string id);
    }
}
