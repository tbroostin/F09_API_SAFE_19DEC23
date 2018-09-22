
//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentMealPlans>, int>> GetStudentMealPlansAsync(int offset, int limit, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.StudentMealPlans> GetStudentMealPlansByGuidAsync(string id);
        Task<StudentMealPlans> PostStudentMealPlansAsync(Dtos.StudentMealPlans StudentMealPlansDto);
        Task<StudentMealPlans> PutStudentMealPlansAsync(string guid, Dtos.StudentMealPlans StudentMealPlansDto);
    }
}
