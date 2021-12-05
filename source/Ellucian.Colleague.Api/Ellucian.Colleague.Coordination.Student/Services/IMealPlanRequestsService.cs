//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for MealPlanRequests services
    /// </summary>
    public interface IMealPlanRequestsService : IBaseService
    {

        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRequests>, int>> GetMealPlanRequestsAsync(int offset, int limit, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.MealPlanRequests> GetMealPlanRequestsByGuidAsync(string guid, bool bypassCache = false);

        Task<Dtos.MealPlanRequests> PutMealPlanRequestsAsync(string guid, Dtos.MealPlanRequests MealPlanRequestsDto);

        Task<Dtos.MealPlanRequests> PostMealPlanRequestsAsync(Dtos.MealPlanRequests MealPlanRequestsDto);
    }
}
