using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IGradeRepository
    {
        Task<ICollection<Grade>> GetAsync();
        Task<ICollection<Grade>> GetHedmAsync(bool bypassCache);
        Task<Grade> GetHedmGradeByIdAsync(string id);
    }
}
