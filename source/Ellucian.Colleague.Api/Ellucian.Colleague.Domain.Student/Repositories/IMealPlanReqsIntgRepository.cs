// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a StudentMealPlan repository
    /// </summary>
    public interface IMealPlanReqsIntgRepository : IEthosExtended
    {
        Task<MealPlanReqsIntg> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<MealPlanReqsIntg>, int>> GetAsync(int offset, int limit, bool bypassCache);

       Task<MealPlanReqsIntg> UpdateMealPlanReqsIntgAsync(MealPlanReqsIntg MealPlanReqsIntgEntity);

       Task<MealPlanReqsIntg> CreateMealPlanReqsIntgAsync(MealPlanReqsIntg MealPlanReqsIntgEntity);

    }
}
