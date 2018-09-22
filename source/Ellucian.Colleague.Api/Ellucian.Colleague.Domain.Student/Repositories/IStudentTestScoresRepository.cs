// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentTestScoresRepository : IEthosExtended
    {
        Task<StudentTestScores> GetStudentTestScoresByGuidAsync(string id);
        Task<Tuple<IEnumerable<StudentTestScores>, int>> GetStudentTestScoresAsync(string studentFilter, int offset, int limit, bool bypassCache = false);

        Task<StudentTestScores> UpdateStudentTestScoresAsync(StudentTestScores studentTestScores);
        Task<StudentTestScores> CreateStudentTestScoresAsync(StudentTestScores studentTestScores);
             
        Task<string> GetStudentTestScoresIdFromGuidAsync(string id);

        Task DeleteAsync(string id);
    }
}
