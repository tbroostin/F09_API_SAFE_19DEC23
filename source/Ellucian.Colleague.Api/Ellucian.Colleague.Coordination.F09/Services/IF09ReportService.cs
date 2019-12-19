using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;


namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IF09ReportService
    {
        Task<dtoF09ReportResponse> GetF09ReportAsync(dtoF09ReportRequest request);
    }
}
