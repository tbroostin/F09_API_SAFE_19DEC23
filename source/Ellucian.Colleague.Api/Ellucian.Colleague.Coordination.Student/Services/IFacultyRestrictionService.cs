using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IFacultyRestrictionService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetFacultyRestrictionsAsync(string facultyId);
    }
}
