//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentUnverifiedGradesRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<StudentUnverifiedGrades>, int>> GetStudentUnverifiedGradesAsync(int offset, int limit, bool bypassCache,
            string student = "", string sectionRegistration = "", string section = "");
        Task<StudentUnverifiedGrades> GetStudentUnverifiedGradeByGuidAsync(string guid);
        Task<string> GetStudentUnverifiedGradesIdFromGuidAsync(string id);
        Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename);

        Task<StudentUnverifiedGrades> UpdateStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGrades studentUnverifiedGradesEntity);       
        Task<StudentUnverifiedGrades> CreateStudentUnverifiedGradesSubmissionsAsync(StudentUnverifiedGrades studentUnverifiedGradesEntity);

        Task<string> GetStudentAcadCredGuidFromIdAsync(string id);
        Task<string> GetStudentAcademicCredIdFromGuidAsync(string guid);
        Task<string> GetStudentAcadCredGradeSchemeFromIdAsync(string id);


    }
}
