using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09KaGradingService
    {
        Task<dtoF09KaGradingResponse> GetF09KaGradingAsync(string stcId);

        Task<dtoF09KaGradingResponse> UpdateF09KaGradingAsync(dtoF09KaGradingRequest request);
    }
}
