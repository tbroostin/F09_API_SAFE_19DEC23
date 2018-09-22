using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IProgramRepository
    {
        Task<IEnumerable<Program>> GetAsync();
        Task<Program> GetAsync(string id);
        Task<StudentProgram> GetAsync(string id, string programCode);
    }
}
