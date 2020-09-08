using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Receive Procurements
    /// </summary>
    public interface IReceiveProcurementsService : IBaseService
    {
        /// <summary>
        /// Returns the list of Purchase Order Items object which is for receving for the particular user
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ReceiveProcurementSummary>> GetReceiveProcurementsByPersonIdAsync(string personId);
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse>  AcceptOrReturnProcurementItemsAsync(ProcurementAcceptReturnItemInformationRequest procurementAcceptOrReturnItemInformationRequest);
    }
}
