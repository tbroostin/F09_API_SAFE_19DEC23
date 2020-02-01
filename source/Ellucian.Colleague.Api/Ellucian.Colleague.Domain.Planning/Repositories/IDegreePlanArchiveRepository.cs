using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Planning.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Planning.Repositories
{
    public interface IDegreePlanArchiveRepository
    {
        Task<DegreePlanArchive> AddAsync(Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive degreePlanArchive);
        Task<IEnumerable<DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId);
        Task<DegreePlanArchive> GetDegreePlanArchiveAsync(int degreePlanArchiveId);
    }
}
