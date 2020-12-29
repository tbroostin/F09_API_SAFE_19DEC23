// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentAcademicPeriodRepository
    {

        Task<Tuple<IEnumerable<StudentAcademicPeriod>, int>> GetStudentAcademicPeriodsAsync(int offset, int limit, bool bypassCache = false,
           string person = "", string academicPeriod = "", string[] filterPersonIds = null, List<string> statuses = null);

        //Task<IDictionary<string, List<StudentTerm>>> GetStudentTermsByStudentIdsAsync(IEnumerable<string> studentIds, string termId, string academicLevelId);
        //sk<StudentTerm> GetStudentAcademicPeriodByGuidAsync(string guid);
        //Task<Tuple<IEnumerable<StudentTerm>, int>> GetStudentTermsAsync(int offset, int limit, bool bypassCache, string newPerson, string newAcademicPeriod);
        Task<StudentAcademicPeriod> GetStudentAcademicPeriodByGuidAsync(string guid);


    }
}