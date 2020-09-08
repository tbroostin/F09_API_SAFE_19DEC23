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
   public class ProcurementItemsInformationResponseEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.ProcurementItemInformationResponse, Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementItemInformationResponse>
    {
        public ProcurementItemsInformationResponseEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
        public ProcurementItemInformationResponse MapToType(Domain.ColleagueFinance.Entities.ProcurementItemInformationResponse Source)
        {
            var itemSummaryDto = new ProcurementItemInformationResponse();

            itemSummaryDto.PurchaseOrderId = Source.PurchaseOrderId;
            itemSummaryDto.PurchaseOrderNumber = Source.PurchaseOrderNumber;
            itemSummaryDto.ItemId = Source.ItemId;
            itemSummaryDto.ItemDescription = Source.ItemDescription;
            return itemSummaryDto;
        }
    }
}
