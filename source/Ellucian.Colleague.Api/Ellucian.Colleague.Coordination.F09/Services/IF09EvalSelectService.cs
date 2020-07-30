using Ellucian.Colleague.Dtos.F09;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    
    public interface IF09EvalSelectService
    {
        Task<dtoF09EvalSelectResponse> GetF09EvalSelectAsync(string personId);
    }
}
