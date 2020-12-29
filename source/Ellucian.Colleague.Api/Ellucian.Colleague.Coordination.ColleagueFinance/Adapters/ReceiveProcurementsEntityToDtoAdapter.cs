using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using AutoMapper;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Custom Adapter for ReceiveProcurements Entity to ReceiveProcurements DTO
    /// </summary>
    public class ReceiveProcurementsEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.ReceiveProcurementSummary, Dtos.ColleagueFinance.ReceiveProcurementSummary>
    {
        public ReceiveProcurementsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }
        /// <summary>
        /// Convert a receive procurement summary domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Source ReceiveProcurementSummary domain entity to be converted</param>
        /// <returns>ReceiveProcurementSummary DTO</returns>
        public ReceiveProcurementSummary MapToType(Domain.ColleagueFinance.Entities.ReceiveProcurementSummary Source)
        {
            var receiveProcurementSummaryDto = new Dtos.ColleagueFinance.ReceiveProcurementSummary();

            receiveProcurementSummaryDto.Id = Source.Id;
            receiveProcurementSummaryDto.Number = Source.Number;
            receiveProcurementSummaryDto.LineItemInformation = new List<LineItemSummary>();
            receiveProcurementSummaryDto.Requisitions = new List<RequisitionLinkSummary>();
            var lineItemSummaryDtoAdapter = new LineItemSummaryEntityToDtoAdapter(adapterRegistry, logger);
            var vendorInfoDtoAdapter = new VendorInfoEntityToDtoAdapter(adapterRegistry, logger);
            var requisitionDtoAdapter = new RequisitionLinkSummaryEntityToDtoAdapter(adapterRegistry, logger);

            foreach (var item in Source.LineItemInformation) {
                var lineItemSummaryDto = lineItemSummaryDtoAdapter.MapToType(item);
                receiveProcurementSummaryDto.LineItemInformation.Add(lineItemSummaryDto);
            }

            foreach (var req in Source.Requisitions)
            {
                var requisitionSummaryDto = requisitionDtoAdapter.MapToType(req);
                receiveProcurementSummaryDto.Requisitions.Add(requisitionSummaryDto);
            }

            var vendorInfoDto = vendorInfoDtoAdapter.MapToType(Source.VendorInformation);
            receiveProcurementSummaryDto.VendorInformation = vendorInfoDto;

            return receiveProcurementSummaryDto;
        }
    }
}
