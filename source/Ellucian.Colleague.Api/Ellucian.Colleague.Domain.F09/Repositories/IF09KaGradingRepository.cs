using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IF09KaGradingRepository
    {
        Task<domF09KaGradingResponse> GetF09KaGradingAsync(string stcId);
        Task<domF09KaGradingResponse> UpdateF09KaGradingAsync(domF09KaGradingRequest request);
    }
}
