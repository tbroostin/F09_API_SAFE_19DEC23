using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IF09EvalFormRepository
    {
        Task<domainF09EvalFormResponse> GetF09EvalFormAsync(string stcId);
        Task<domainF09EvalFormResponse> UpdateF09EvalFormAsync(domainF09EvalFormRequest request);
    }
}
