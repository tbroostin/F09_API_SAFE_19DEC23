using Ellucian.Colleague.Domain.F09.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    
    public interface IF09EvalSelectRepository
    {
        Task<domainF09EvalSelectResponse> GetF09EvalSelectAsync(string personId);
    }
}
