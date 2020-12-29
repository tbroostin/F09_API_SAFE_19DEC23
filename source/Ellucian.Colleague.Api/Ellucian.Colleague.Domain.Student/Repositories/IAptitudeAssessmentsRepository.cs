// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IAptitudeAssessmentsRepository
    {
        Task<IEnumerable<NonCourse>> GetAptitudeAssessmentsAsync(bool bypassCache);
        Task<Dictionary<string, string>> GetAptitudeAssessmentGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys);
        Task<NonCourse> GetAptitudeAssessmentByIdAsync(string guid);
        Task<string> GetAptitudeAssessmentsGuidAsync(string code);
        Task<string> GetAptitudeAssessmentsIdFromGuidAsync(string guid);
    }
}