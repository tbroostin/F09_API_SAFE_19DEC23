using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IAptitudeAssessmentsRepository
    {
        Task<IEnumerable<NonCourse>> GetAptitudeAssessmentsAsync(bool bypassCache);
        Task<Dictionary<string, string>> GetAptitudeAssessmentGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys);
        Task<NonCourse> GetAptitudeAssessmentByIdAsync(string guid);
    }
}
