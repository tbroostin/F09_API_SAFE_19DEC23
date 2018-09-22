//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student
{
    /// <summary>
    /// Interface for StudentAcademicStandings services
    /// </summary>
    public interface IStudentAcademicStandingsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicStandings>, int>> GetStudentAcademicStandingsAsync(int offset, int limit, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.StudentAcademicStandings> GetStudentAcademicStandingsByGuidAsync(string id);
    }
}
