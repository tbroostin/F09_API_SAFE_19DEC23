using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;

namespace Ellucian.Colleague.Domain.F09.Repositories
{
    public interface IF09ReportRepository
    {        
        Task<domF09ReportResponse> GetF09ReportAsync(domF09ReportRequest request);
    }
}
