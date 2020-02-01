// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentDegreePlanRepository
    {
        Task<DegreePlan> AddAsync(DegreePlan newPlan);
        Task<DegreePlan> UpdateAsync(DegreePlan plan);
        Task<DegreePlan> GetAsync(int planId);
        Task<IEnumerable<DegreePlan>> GetAsync(IEnumerable<string> studentIds);
    }
}
