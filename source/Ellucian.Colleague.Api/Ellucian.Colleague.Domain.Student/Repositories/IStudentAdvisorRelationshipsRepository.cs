using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentAdvisorRelationshipsRepository
    {
        Task<Tuple<IEnumerable<StudentAdvisorRelationship>, int>> GetStudentAdvisorRelationshipsAsync(int offset, int limit, bool bypassCache = false,
            string student = "", string advisor = "", string advisorType = "");

        Task<StudentAdvisorRelationship> GetStudentAdvisorRelationshipsByGuidAsync(string guid);
    }
}
