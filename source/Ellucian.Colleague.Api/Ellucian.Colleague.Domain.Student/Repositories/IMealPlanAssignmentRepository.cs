﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public interface IMealPlanAssignmentRepository : IEthosExtended
    {
        Task<string> GetMealPlanAssignmentIdFromGuidAsync( string guid );
        Task<MealPlanAssignment> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<MealPlanAssignment>, int>> GetAsync(int offset, int limit, string person = "", string term = "", string mealplan = "", string status = "", string startDate = "", string endDate = "");

        Task<MealPlanAssignment> UpdateMealPlanAssignmentAsync(MealPlanAssignment MealPlanAssignmentEntity);

        Task<MealPlanAssignment> CreateMealPlanAssignmentAsync(MealPlanAssignment MealPlanAssignmentEntity);

    }
}
