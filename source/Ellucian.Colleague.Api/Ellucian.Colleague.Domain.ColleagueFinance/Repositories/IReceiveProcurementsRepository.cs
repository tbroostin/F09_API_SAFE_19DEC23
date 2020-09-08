using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IReceiveProcurementsRepository
    {
        Task<IEnumerable<ReceiveProcurementSummary>> GetReceiveProcurementsByPersonIdAsync(string personId);

        Task<ProcurementAcceptReturnItemInformationResponse> AcceptOrReturnProcurementItemsAsync(ProcurementAcceptReturnItemInformationRequest procurementAcceptReturnItemInformationRequest);
    }
}
