using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09EvalFormService
    {
        Task<dtoF09EvalFormResponse> GetF09EvalFormAsync(string key);

        Task<dtoF09EvalFormResponse> UpdateF09EvalFormAsync(dtoF09EvalFormRequest request);
    }
}