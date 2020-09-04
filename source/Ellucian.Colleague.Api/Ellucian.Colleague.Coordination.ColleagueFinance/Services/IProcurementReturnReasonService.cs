using Ellucian.Colleague.Dtos.ColleagueFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the Procurement Return Reason service.
    /// </summary>
    public interface IProcurementReturnReasonService
    {
        Task<IEnumerable<ProcurementReturnReason>> GetProcurementReturnReasonsAsync();
    }
}
