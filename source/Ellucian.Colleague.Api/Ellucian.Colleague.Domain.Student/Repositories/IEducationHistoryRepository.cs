using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IEducationHistoryRepository
    {
        Task<IEnumerable<EducationHistory>> GetAsync(IEnumerable<string> studentIds);
        Task<IEnumerable<OtherDegree>> GetOtherDegreesAsync();
    }
}
