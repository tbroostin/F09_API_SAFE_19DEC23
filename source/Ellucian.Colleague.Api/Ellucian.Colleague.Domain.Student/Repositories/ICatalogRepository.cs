using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ICatalogRepository
    {
        Task<ICollection<Catalog>> GetAsync();
        Task<ICollection<Catalog>> GetAsync(bool bypassCache = false);
    }
}
