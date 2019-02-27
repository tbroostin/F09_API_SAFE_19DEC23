// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentTranscriptGradesOptionsRepository
    {
        Task<Tuple<IEnumerable<StudentTranscriptGradesOptions>, int>> GetStudentTranscriptGradesOptionsAsync(int offset, int limit, string student = "", bool bypassCache = false);
        Task<StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByGuidAsync(string guid);
        Task<string> GetStudentTranscriptGradesOptionsIdFromGuidAsync(string guid);
        Task<StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByIdAsync(string id);
        
    }
}
