//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for MealPlanRates services
    /// </summary>
    public interface IMealPlanRatesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRates>> GetMealPlanRatesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.MealPlanRates> GetMealPlanRatesByGuidAsync(string id);
    }
}
