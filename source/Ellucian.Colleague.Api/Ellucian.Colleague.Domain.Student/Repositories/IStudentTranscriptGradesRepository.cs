// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentTranscriptGradesRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<StudentTranscriptGrades>, int>> GetStudentTranscriptGradesAsync(int offset, int limit, string student = "", bool bypassCache = false);
        Task<StudentTranscriptGrades> GetStudentTranscriptGradesByGuidAsync(string guid);
        Task<string> GetStudentTranscriptGradesIdFromGuidAsync(string guid);
        Task<StudentTranscriptGrades> GetStudentTranscriptGradesByIdAsync(string id);

        Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename);
        Task<StudentTranscriptGrades> UpdateStudentTranscriptGradesAdjustmentsAsync(StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments);
    }
}
