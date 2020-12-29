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
    public class ReceiveProcurementsResponseEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse, Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse>
    {
        public ReceiveProcurementsResponseEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public ProcurementAcceptReturnItemInformationResponse MapToType(Domain.ColleagueFinance.Entities.ProcurementAcceptReturnItemInformationResponse Source)
        {
            var receiveProcurementResponseDto = new Dtos.ColleagueFinance.ProcurementAcceptReturnItemInformationResponse();

            receiveProcurementResponseDto.WarningOccurred = Source.WarningOccurred;
            receiveProcurementResponseDto.WarningMessages = Source.WarningMessages;
            receiveProcurementResponseDto.ErrorMessages = Source.ErrorMessages;
            receiveProcurementResponseDto.ErrorOccurred = Source.ErrorOccurred;
            receiveProcurementResponseDto.ProcurementItemsInformationResponse = new List<ProcurementItemInformationResponse>();
            var itemInformationDtoAdapter = new ProcurementItemsInformationResponseEntityToDtoAdapter(adapterRegistry, logger);

            foreach (var item in Source.ProcurementItemsInformationResponse)
            {
                var itemSummaryDto = itemInformationDtoAdapter.MapToType(item);
                receiveProcurementResponseDto.ProcurementItemsInformationResponse.Add(itemSummaryDto);
            }

            return receiveProcurementResponseDto;
        }
    }
}
