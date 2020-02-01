// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IPlanningStudentRepository
    {
        Task<Domain.Student.Entities.PlanningStudent> GetAsync(string id, bool useCache = true);
        Task<IEnumerable<Domain.Student.Entities.PlanningStudent>> GetAsync(IEnumerable<string> ids, bool useCache = true);
    }
}
