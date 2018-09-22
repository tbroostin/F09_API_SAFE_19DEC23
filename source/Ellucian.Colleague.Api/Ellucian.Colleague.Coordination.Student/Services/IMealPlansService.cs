//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for MealPlans services
    /// </summary>
    public interface IMealPlansService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlans>> GetMealPlansAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.MealPlans> GetMealPlansByGuidAsync(string id);
    }
}
