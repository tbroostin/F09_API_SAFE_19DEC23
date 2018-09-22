using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentTermService
    {
        Task<IEnumerable<Dtos.Student.StudentTerm>> QueryStudentTermsAsync(Dtos.Student.StudentTermsQueryCriteria criteria);
        Task<IEnumerable<Dtos.Student.PilotStudentTermLevelGpa>> QueryPilotStudentTermsGpaAsync(IEnumerable<string> studentIds, string term);
    }
}
