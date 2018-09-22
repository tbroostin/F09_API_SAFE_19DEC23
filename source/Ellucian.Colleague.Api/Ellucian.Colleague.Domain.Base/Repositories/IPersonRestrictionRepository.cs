using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPersonRestrictionRepository
    {
        Task<IEnumerable<PersonRestriction>> GetAsync(string personId, bool useCache = true);
        Task<IEnumerable<PersonRestriction>> GetRestrictionsByStudentIdsAsync(IEnumerable<string> studentIds);
        Task<IEnumerable<PersonRestriction>> GetRestrictionsByIdsAsync(IEnumerable<string> ids);
        
    }
}
