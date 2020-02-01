//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentMealPlans services
    /// </summary>
    public interface IStudentMealPlansService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>> GetStudentMealPlansAsync(int offset, int limit, StudentMealPlans criteriaFilter, bool bypassCache = false);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans2>, int>> GetStudentMealPlans2Async(int offset, int limit, StudentMealPlans2 criteriaFilter, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.StudentMealPlans> GetStudentMealPlansByGuidAsync(string id);
        Task<Ellucian.Colleague.Dtos.StudentMealPlans2> GetStudentMealPlansByGuid2Async(string guid);
        Task<StudentMealPlans> PostStudentMealPlansAsync(Dtos.StudentMealPlans StudentMealPlansDto);
        Task<StudentMealPlans2> PostStudentMealPlans2Async(Dtos.StudentMealPlans2 studentMealPlansDto2);
        Task<StudentMealPlans> PutStudentMealPlansAsync(string guid, Dtos.StudentMealPlans StudentMealPlansDto);
        Task<StudentMealPlans2> PutStudentMealPlans2Async(string guid, Dtos.StudentMealPlans2 studentMealPlansDto2);
    }
}
