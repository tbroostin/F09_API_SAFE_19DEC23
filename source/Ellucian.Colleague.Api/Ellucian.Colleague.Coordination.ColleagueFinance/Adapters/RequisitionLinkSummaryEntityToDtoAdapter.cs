using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class RequisitionLinkSummaryEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.RequisitionLinkSummary, Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionLinkSummary>
    {
        public RequisitionLinkSummaryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
        public RequisitionLinkSummary MapToType(Domain.ColleagueFinance.Entities.RequisitionLinkSummary Source)
        {
            var requisitionSummaryDto = new RequisitionLinkSummary();
            requisitionSummaryDto.Id = Source.Id;
            requisitionSummaryDto.Number = Source.Number;
            return requisitionSummaryDto;
        }
    }
}
