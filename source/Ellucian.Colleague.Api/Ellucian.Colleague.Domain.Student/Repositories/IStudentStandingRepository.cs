// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentStandingRepository
    {
        Task<IEnumerable<StudentStanding>> GetAsync(IEnumerable<string> studentIds, string term = null);
        Task<Dictionary<string, List<StudentStanding>>> GetGroupedAsync(IEnumerable<string> studentIds);

        Task<Tuple<IEnumerable<StudentStanding>, int>> GetStudentStandingsAsync(int offset, int limit);
        Task<StudentStanding> GetStudentStandingByGuidAsync(string guid);
    }
}