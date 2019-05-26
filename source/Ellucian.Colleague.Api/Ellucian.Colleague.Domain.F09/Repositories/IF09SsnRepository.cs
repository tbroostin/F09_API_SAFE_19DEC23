using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IF09SsnRepository
    {
        Task<F09SsnResponse> GetF09SsnAsync(string Id);
        Task<F09SsnResponse> UpdateF09SsnAsync(F09SsnRequest request);
    }
}
