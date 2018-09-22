// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentTermRepository
    {
        Task<IDictionary<string, List<StudentTerm>>> GetStudentTermsByStudentIdsAsync(IEnumerable<string> studentIds, string termId, string academicLevelId);
        Task<StudentTerm> GetStudentTermByGuidAsync(string guid);
        Task<Tuple<IEnumerable<StudentTerm>, int>> GetStudentTermsAsync(int offset, int limit, bool bypassCache, string newPerson, string newAcademicPeriod);

    }
}