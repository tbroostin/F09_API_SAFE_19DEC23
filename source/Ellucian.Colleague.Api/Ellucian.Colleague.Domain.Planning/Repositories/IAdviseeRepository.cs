// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Planning.Repositories
{
    public interface IAdviseeRepository
    {
        Task<Student.Entities.PlanningStudent> GetAsync(string id);
        Task<IEnumerable<Student.Entities.PlanningStudent>> GetAsync(IEnumerable<string> ids, int pageSize, int pageIndex);
        Task<IEnumerable<Student.Entities.PlanningStudent>> SearchByNameAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null);
        Task<IEnumerable<Student.Entities.PlanningStudent>> SearchByAdvisorIdsAsync(IEnumerable<string> advisorIds, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null);
        Task<Domain.Student.Entities.PlanningStudent> PostCompletedAdvisementAsync(string studentId, DateTime completionDate, DateTimeOffset completionTime, string advisorId);
        Task<IEnumerable<Student.Entities.PlanningStudent>> SearchByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1, IEnumerable<string> assignedAdvisees = null);


    }
}